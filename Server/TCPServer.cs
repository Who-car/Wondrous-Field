using ClientServerTransfer;
using PackageHelper;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Server
{
    public class TCPServer
    {
        readonly Random random = new();
        readonly Socket _listener;
        readonly Dictionary<string, Session> _waitingSessions;
        readonly Dictionary<string, Session> _processingSessions;

        internal async Task MoveSessionToProcessingSessions(string sessionId)
        {
            try
            {
                await Task.Run(() =>
                {
                    _processingSessions[sessionId] = _waitingSessions[sessionId];
                    _waitingSessions.Remove(sessionId);
                });
            }
            catch
            {

            }
        }

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
                    var clientSocket = await _listener.AcceptAsync().ConfigureAwait(false);

                    //_clients.Add(clientSocket, null!);

                    _ = Task.Run(
                        async() => 
                            await ProcessClientAsync(clientSocket).ConfigureAwait(false));

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
                var received = await Package.GetFullContent(socket).ConfigureAwait(false);
                if (!Package.IsCreateSession(received.Command!) && !Package.IsJoin(received.Command!))
                {
                    throw new Exception("Unexpected command");
                }
                else if (Package.IsCreateSession(received.Command!))
                {
                    await CreateSession(await Serialiser.DeserialiseAsync<ConnectionInfo>(received.Body!).ConfigureAwait(false), socket).ConfigureAwait(false);
                }
                else if (Package.IsJoin(received.Command!))
                {
                    Console.WriteLine(Encoding.UTF8.GetString(received.Body!));
                    await JoinToSession(await Serialiser.DeserialiseAsync<ConnectionInfo>(received.Body!), socket).ConfigureAwait(false);
                }

                await ListenSocketInLoopAsync(socket).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                await socket.DisconnectAsync(false).ConfigureAwait(false);
                throw new Exception($"Error: {ex}");
            }
        }

        async Task ListenSocketInLoopAsync(Socket socket)
        {
            ReceivedData received;

            while (socket.Connected)
            {
                received = await Package.GetFullContent(socket).ConfigureAwait(false);
                if (Package.IsNameLetter(received.Command!))
                {
                    var sessionInfo = await Serialiser.DeserialiseAsync<SessionInfo>(received.Body!);
                    if(_processingSessions.ContainsKey(sessionInfo.SessionId!))
                    {
                        await _processingSessions[sessionInfo.SessionId!].NameTheLetter(socket, sessionInfo.Letter).ConfigureAwait(false);
                    }
                }
                else if (Package.IsNameWord(received.Command!))
                {
                    var sessionInfo = await Serialiser.DeserialiseAsync<SessionInfo>(received.Body!);
                    if (_processingSessions.ContainsKey(sessionInfo.SessionId!))
                    {
                        await _processingSessions[sessionInfo.SessionId!].NameTheWord(socket, sessionInfo.Word!).ConfigureAwait(false);
                    }
                }
                else if (Package.IsMessage(received.Command!))
                {
                    var messageInfo = await Serialiser.DeserialiseAsync<Message>(received.Body!);
                    if (_processingSessions.ContainsKey(messageInfo.SessionId!))
                    {
                        await _processingSessions[messageInfo.SessionId!].SendMessageToPlayers(messageInfo.Content!, socket).ConfigureAwait(false);
                    }
                    /*else
                    {
                        throw new Exception();
                    }*/
                }
                else if (Package.IsBye(received.Command!))
                {
                    await socket.DisconnectAsync(false).ConfigureAwait(false);
                }
            }
        }

        async Task<bool> CreateSession(ConnectionInfo connectionInfo, Socket player)
        {
            try
            {
                Session session = new(CreateId(5), this);
                await Package.SendResponseToUser(player, await Serialiser.SerialiseToBytesAsync(new ConnectionInfo { SessionId = session.SessionId, IsSuccessfulJoin = true, PlayerName = connectionInfo.PlayerName }).ConfigureAwait(false)).ConfigureAwait(false);
                await session.AddPlayer(connectionInfo.PlayerName!, player).ConfigureAwait(false);
                _waitingSessions.Add(session.SessionId, session);

                return true;
            }
            catch
            {
                await Package.SendResponseToUser(player, await Serialiser.SerialiseToBytesAsync(new ConnectionInfo { IsSuccessfulJoin = false, PlayerName = connectionInfo.PlayerName }).ConfigureAwait(false)).ConfigureAwait(false);
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
                        await Package.SendResponseToUser(player, await Serialiser.SerialiseToBytesAsync(new ConnectionInfo { SessionId = connectionInfo.SessionId, IsSuccessfulJoin = true, PlayerName = connectionInfo.PlayerName}).ConfigureAwait(false)).ConfigureAwait(false);
                        await _waitingSessions[connectionInfo.SessionId].AddPlayer(connectionInfo.PlayerName!, player).ConfigureAwait(false);
                    }
                    else
                    {
                        await Package.SendResponseToUser(player, await Serialiser.SerialiseToBytesAsync(new ConnectionInfo { SessionId = connectionInfo.SessionId, IsSuccessfulJoin = false, PlayerName = connectionInfo.PlayerName }).ConfigureAwait(false)).ConfigureAwait(false);
                    }
                }
                else if(connectionInfo.IsRandomJoin)
                {
                    var flag = false;
                    foreach(var session in _waitingSessions.Values)
                    {
                        if(!session.IsFull)
                        {
                            await Package.SendResponseToUser(player, await Serialiser.SerialiseToBytesAsync(new ConnectionInfo { SessionId = session.SessionId, IsSuccessfulJoin = true, PlayerName = connectionInfo.PlayerName }));
                            await _waitingSessions[session.SessionId].AddPlayer(connectionInfo.PlayerName!, player);
                            flag = true;
                            break;
                        }
                    }
                    if (!flag)
                    {
                        await CreateSession(connectionInfo, player).ConfigureAwait(false);
                    }
                }
                else
                {
                    await Package.SendResponseToUser(player, await Serialiser.SerialiseToBytesAsync(new ConnectionInfo { SessionId = connectionInfo.SessionId, IsSuccessfulJoin = false, PlayerName = connectionInfo.PlayerName }).ConfigureAwait(false)).ConfigureAwait(false);
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
