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
        readonly Dictionary<Socket, Session> _observerSessions;

        public TCPServer(IPAddress ipAddress, int port)
        {
            _listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _listener.Bind(new IPEndPoint(ipAddress, port));
            _waitingSessions = new();
            _processingSessions = new();
            _observerSessions = new();
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

                    _ = Task.Run(
                        async() => 
                            await ProcessClientAsync(clientSocket));

                } while (true);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
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
                if (!Package.IsCreateSession(received.Command!) && !Package.IsJoin(received.Command!))
                {
                    throw new Exception("Unexpected command");
                }
                else if (Package.IsCreateSession(received.Command!))
                {
                    await CreateSession(await Serialiser.DeserialiseAsync<ConnectionInfo>(received.Body!), socket, true);
                }
                else if (Package.IsJoin(received.Command!))
                {

                    await JoinToSession(await Serialiser.DeserialiseAsync<ConnectionInfo>(received.Body!), socket);
                }

                await ListenSocketInLoopAsync(socket);
            }
            catch
            {
                await DeletePlayerFromSession(socket);
                await socket.DisconnectAsync(false);
            }
        }

        async Task ListenSocketInLoopAsync(Socket socket)
        {
            while (socket.Connected)
            {
                ReceivedData received;
                try
                {
                    received = await Package.GetFullContent(socket);
                }
                catch
                {
                    continue;
                }
                if (Package.IsNameLetter(received.Command!))
                {
                    var sessionInfo = await Serialiser.DeserialiseAsync<SessionInfo>(received.Body!);
                    if(_processingSessions.ContainsKey(sessionInfo.SessionId!))
                    {
                        await _processingSessions[sessionInfo.SessionId!].NameTheLetter(socket, sessionInfo.Letter, sessionInfo.CurrentPlayer.Points);
                    }
                }
                else if (Package.IsNameWord(received.Command!))
                {
                    var sessionInfo = await Serialiser.DeserialiseAsync<SessionInfo>(received.Body!);
                    if (_processingSessions.ContainsKey(sessionInfo.SessionId!))
                    {
                        await _processingSessions[sessionInfo.SessionId!].NameTheWord(socket, sessionInfo.Word!, sessionInfo.CurrentPlayer.Points);
                    }
                }
                else if (Package.IsScore(received.Command!))
                {
                    var sessionInfo = await Serialiser.DeserialiseAsync<SessionInfo>(received.Body!);
                    if (_processingSessions.ContainsKey(sessionInfo.SessionId!))
                    {
                        await _processingSessions[sessionInfo.SessionId!].GetScore(socket);
                    }
                }
                else if (Package.IsMessage(received.Command!))
                {
                    var messageInfo = await Serialiser.DeserialiseAsync<Message>(received.Body!);
                    if (_processingSessions.ContainsKey(messageInfo.SessionId!))
                    {
                        await _processingSessions[messageInfo.SessionId!].SendMessageToPlayers(messageInfo, socket);
                    }
                }
                else if (Package.IsBye(received.Command!))
                {
                    //TODO: disconnecting
                    await socket.DisconnectAsync(false);
                }
            }
        }

        async Task<bool> CreateSession(ConnectionInfo connectionInfo, Socket player, bool isPrivate = false)
        {
            try
            {
                Session session = new(CreateId(5), this, isPrivate);
                var result = await session.AddPlayer(connectionInfo.PlayerInfo, player);
                if (result) _observerSessions[player] = session;

                _waitingSessions.Add(session.SessionId, session);
                await Package.SendResponseToUser(player,
                    await Serialiser.SerialiseToBytesAsync(new ConnectionInfo
                    {
                        SessionId = session.SessionId,
                        IsSuccessfulJoin = result,
                        PlayerInfo = connectionInfo.PlayerInfo
                    }));

                return true;
            }
            catch
            {
                await Package.SendResponseToUser(player,
                    await Serialiser.SerialiseToBytesAsync(new ConnectionInfo
                    {
                        IsSuccessfulJoin = false,
                        PlayerInfo = connectionInfo.PlayerInfo
                    }));
                
                return false;
            }
        }

        async Task<bool> JoinToSession(ConnectionInfo connectionInfo, Socket player)
        {
            try
            {
                if(!connectionInfo.IsRandomJoin && connectionInfo.SessionId != null)
                {
                    if (_waitingSessions.ContainsKey(connectionInfo.SessionId) && _waitingSessions[connectionInfo.SessionId].IsPrivate)
                    {
                        await Package.SendResponseToUser(player,
                            await Serialiser.SerialiseToBytesAsync(new ConnectionInfo
                            {
                                SessionId = connectionInfo.SessionId,
                                IsSuccessfulJoin = true,
                                PlayerInfo = connectionInfo.PlayerInfo
                            }));
                        var result = await _waitingSessions[connectionInfo.SessionId].AddPlayer(connectionInfo.PlayerInfo, player);
                        if (result) _observerSessions[player] = _waitingSessions[connectionInfo.SessionId];
                    }
                    else
                    {
                        await Package.SendResponseToUser(player, await Serialiser.SerialiseToBytesAsync(new ConnectionInfo { SessionId = connectionInfo.SessionId, IsSuccessfulJoin = false, PlayerInfo = connectionInfo.PlayerInfo }));
                    }
                }
                else if(connectionInfo.IsRandomJoin)
                {
                    var flag = false;
                    foreach(var session in _waitingSessions.Values)
                    {
                        if(!session.IsFull)
                        {
                            await Package.SendResponseToUser(player,
                                await Serialiser.SerialiseToBytesAsync(new ConnectionInfo
                                {
                                    SessionId = session.SessionId,
                                    IsSuccessfulJoin = true,
                                    PlayerInfo = connectionInfo.PlayerInfo
                                }));
                            var result = await _waitingSessions[session.SessionId].AddPlayer(connectionInfo.PlayerInfo, player);
                            if (result) _observerSessions[player] = _waitingSessions[session.SessionId];
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
                    await Package.SendResponseToUser(player,
                        await Serialiser.SerialiseToBytesAsync(new ConnectionInfo
                        {
                            SessionId = connectionInfo.SessionId,
                            IsSuccessfulJoin = false,
                            PlayerInfo = connectionInfo.PlayerInfo
                        }));
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

            return
                _waitingSessions.ContainsKey(id.ToString()) || _processingSessions.ContainsKey(id.ToString())
                    ? CreateId(len)
                    : id.ToString();
        }

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

        internal async Task DeleteSession(string sessionId)
        {
            try
            {
                await Task.Run(() =>
                {
                    if (_waitingSessions.ContainsKey(sessionId)) _waitingSessions.Remove(sessionId);
                    if (_processingSessions.ContainsKey(sessionId)) _processingSessions.Remove(sessionId);
                });
            }
            catch
            {
            }
        }

        async Task DeletePlayerFromSession(Socket player)
        {
            if(_observerSessions.ContainsKey(player))
            {
                await _observerSessions[player].RemovePlayer(player);
                _observerSessions.Remove(player);
            }
        }
    }
}
