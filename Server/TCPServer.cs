using ClientServerTransfer;
using PackageHelper;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using static System.Collections.Specialized.BitVector32;

namespace Server
{
    public class TCPServer
    {
        Random random = new();
        readonly Socket _listener;
        readonly Dictionary<string, Session> _waitingSessions;
        readonly Dictionary<string, Session> _processingSessions;

        public TCPServer(IPAddress ipAddress, int port)
        {
            _listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _listener.Bind(new IPEndPoint(ipAddress, port));
            _waitingSessions = new();
            _processingSessions = new();
        }

        public async Task RunServerAsync()
        {
            try
            {
                _listener.Listen();
                Console.WriteLine("...Сервер запущен...");
                do
                {
                    var clientSocket = await _listener.AcceptAsync();

                    //_clients.Add(clientSocket, null!);

                    _ = Task.Run(
                        async() => 
                            await ProcessClientAsync(clientSocket));

                } while (true);
            }
            catch
            {
                //TODO: журналирование
            }
            finally
            {
                _listener.Close();
                Console.WriteLine("...Сервер завершил работу...");
            }
        }

        async Task ProcessClientAsync(Socket socket)
        {
            try
            {
                var received = await Package.GetFullContent(socket);
                if (!received.Command!.SequenceEqual(Command.CreateSession) && !received.Command.SequenceEqual(Command.Join))
                {
                    throw new Exception("Unexpected command");
                }
                else if (received.Command!.SequenceEqual(Command.CreateSession))
                {
                    await CreateSession(await Serialiser.DeserialiseAsync<ConnectionInfo>(received.Body!), socket);
                }
                else if (received.Command.SequenceEqual(Command.Join))
                {
                    await JoinToSession(await Serialiser.DeserialiseAsync<ConnectionInfo>(received.Body!), socket);
                }

                await ListenSocketInLoopAsync(socket);
            }
            catch (Exception ex)
            {
                await socket.DisconnectAsync(false);
                throw new Exception($"Error: {ex}");
            }
        }

        async Task ListenSocketInLoopAsync(Socket socket)
        {
            ReceivedData received;

            while (socket.Connected)
            {
                received = await Package.GetFullContent(socket);

                if (received.Command!.Equals(Command.NameTheLetter))
                {
                    var sessionInfo = await Serialiser.DeserialiseAsync<SessionInfo>(received.Body!);
                    if(_processingSessions.ContainsKey(sessionInfo.SessionId!))
                    {
                        await _processingSessions[sessionInfo.SessionId!].NameTheLetter(socket, sessionInfo.Letter);
                    }
                }
                else if (received.Command!.Equals(Command.NameTheWord))
                {
                    var sessionInfo = await Serialiser.DeserialiseAsync<SessionInfo>(received.Body!);
                    if (_processingSessions.ContainsKey(sessionInfo.SessionId!))
                    {
                        await _processingSessions[sessionInfo.SessionId!].NameTheWord(socket, sessionInfo.Word!);
                    }
                }
                else if (received.Command!.Equals(Command.SendMessage))
                {
                    var messageInfo = await Serialiser.DeserialiseAsync<Message>(received.Body!);
                    if (_processingSessions.ContainsKey(messageInfo.SessionId!))
                    {
                        await _processingSessions[messageInfo.SessionId!].SendMessageToPlayers(socket, messageInfo.Content!);
                    }
                    else
                    {
                        throw new Exception();
                    }
                }
                else if (received.Command!.Equals(Command.Bye))
                {
                    await socket.DisconnectAsync(false);
                }
            }
        }

        async Task<bool> CreateSession(ConnectionInfo connectionInfo, Socket player)
        {
            try
            {
                Session session = new(CreateId(5));
                await session.AddPlayer(connectionInfo.PlayerName!, player);
                _waitingSessions.Add(session.SessionId, session);
                await Package.SendResponseToUser(player, await Serialiser.SerialiseToBytesAsync(new ConnectionInfo { SessionId = session.SessionId, IsSuccessfulJoin = true, PlayerName = connectionInfo.PlayerName }));

                return true;
            }
            catch
            {
                await Package.SendResponseToUser(player, await Serialiser.SerialiseToBytesAsync(new ConnectionInfo { IsSuccessfulJoin = false, PlayerName = connectionInfo.PlayerName }));
                return false;
            }
        }

        async Task<bool> JoinToSession(ConnectionInfo connectionInfo, Socket player)
        {
            try
            {
                if(!connectionInfo.IsRandomJoin && connectionInfo.SessionId != null)
                {
                    if (_waitingSessions.ContainsKey(connectionInfo.SessionId))
                    {
                        await _waitingSessions[connectionInfo.SessionId].AddPlayer(connectionInfo.PlayerName!, player);
                        await Package.SendResponseToUser(player, await Serialiser.SerialiseToBytesAsync(new ConnectionInfo { SessionId = connectionInfo.SessionId, IsSuccessfulJoin = true, PlayerName = connectionInfo.PlayerName}));
                    }
                    else
                    {
                        await Package.SendResponseToUser(player, await Serialiser.SerialiseToBytesAsync(new ConnectionInfo { SessionId = connectionInfo.SessionId, IsSuccessfulJoin = false, PlayerName = connectionInfo.PlayerName }));
                    }
                }
                else if(connectionInfo.IsRandomJoin)
                {
                    var flag = false;
                    foreach(var session in _waitingSessions.Values)
                    {
                        if(!session.IsFull)
                        {
                            await _waitingSessions[session.SessionId].AddPlayer(connectionInfo.PlayerName!, player);
                            await Package.SendResponseToUser(player, await Serialiser.SerialiseToBytesAsync(new ConnectionInfo { SessionId = session.SessionId, IsSuccessfulJoin = true, PlayerName = connectionInfo.PlayerName }));
                            flag = true;
                            break;
                        }
                    }
                    if (!flag)
                    {
                        await CreateSession(connectionInfo, player);
                    }
                }
                else
                {
                    await Package.SendResponseToUser(player, await Serialiser.SerialiseToBytesAsync(new ConnectionInfo { SessionId = connectionInfo.SessionId, IsSuccessfulJoin = false, PlayerName = connectionInfo.PlayerName }));
                    throw new Exception("Exception during join");
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        string CreateId(int len)
        {
            var id = new StringBuilder();

            for (var i = 0; i < len; i++)
            {
                var num = random.Next(55, 90);

                if (num < 65) num -= 7;

                id.Append(Convert.ToChar(num));
            }

            return id.ToString();
        }
    }
}
