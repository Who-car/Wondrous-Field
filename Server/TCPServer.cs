using ClientServerTransfer;
using PackageHelper;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Server
{
    public class TCPServer
    {

        readonly Socket _listener;
        readonly Dictionary<string, Session> _privateSessions;
        readonly Dictionary<string, Session> _openSessions;
        readonly Dictionary<string, Session> _inGameProcessSessions;

        public TCPServer(IPAddress ipAddress, int port)
        {
            _listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _listener.Bind(new IPEndPoint(ipAddress, port));
            _privateSessions = new();
            _openSessions = new();
            _inGameProcessSessions = new();
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
                var query = await Package.GetFullContent(socket);

                if (query.Command!.Equals(Command.CreateSession))
                {
                    await CreateSession(Encoding.UTF8.GetString(query.Body!), socket);
                }
                else if (query.Command.Equals(Command.Join))
                {
                    await JoinToSession(Encoding.UTF8.GetString(query.Body!), socket, "");
                }

                await ListenSocketInLoopAsync(socket);
            }
            catch
            {
                //TODO: Exception
                await socket.DisconnectAsync(false);
            }
        }

        async Task ListenSocketInLoopAsync(Socket socket)
        {
            ReceivedData query;

            while (socket.Connected)
            {
                query = await Package.GetFullContent(socket);

                if (query.Command!.Equals(Command.NameTheLetter))
                {
                    var sessionInfo = await Serialiser.Deserialise<SessionInfo>(query.Body!);
                    if(_inGameProcessSessions.ContainsKey(sessionInfo.SessionId!))
                    {
                        await _inGameProcessSessions[sessionInfo.SessionId!].NameTheLetter(socket, sessionInfo.Letter);
                    }
                }
                else if (query.Command!.Equals(Command.NameTheWord))
                {
                    var sessionInfo = await Serialiser.Deserialise<SessionInfo>(query.Body!);
                    if (_inGameProcessSessions.ContainsKey(sessionInfo.SessionId!))
                    {
                        await _inGameProcessSessions[sessionInfo.SessionId!].NameTheWord(socket, sessionInfo.Word!);
                    }
                }
                else if (query.Command!.Equals(Command.Post))
                {

                }
                else if (query.Command!.Equals(Command.SendMessage))
                {
                    var messageInfo = await Serialiser.Deserialise<Message>(query.Body!);
                    if (_inGameProcessSessions.ContainsKey(messageInfo.SessionId!))
                    {
                        await _inGameProcessSessions[messageInfo.SessionId!].SendMessageToPlayers(socket, messageInfo.Content);
                    }
                    else
                    {
                        throw new Exception();
                    }
                }
                else if (query.Command!.Equals(Command.Bye))
                {
                    await socket.DisconnectAsync(false);
                }
            }
        }

        

        async Task<bool> CreateSession(string name, Socket player)
        {
            try
            {
                Session session = new();
                await session.AddPlayer(name, player);
                _privateSessions.Add(session.SessionId, session);
                await Package.SendResponseToUser(player, await Serialiser.SerialiseToBytes(new ConnectionInfo { SessionId = "", IsSuccessfulJoin = true }));

                return true;
            }
            catch
            {
                return false;
            }
        }

        async Task<bool> JoinToSession(string name, Socket player, string sessionId)
        {
            try
            {
                if(sessionId != null)
                {
                    if (_privateSessions.ContainsKey(sessionId))
                    {
                        await _privateSessions[sessionId].AddPlayer(name, player);
                        await Package.SendResponseToUser(player, await Serialiser.SerialiseToBytes(new ConnectionInfo { SessionId = "", IsSuccessfulJoin = true }));
                    }
                }
                else
                {
                    var session = _openSessions.Where(x => !x.Value.SessionIsFull).First().Value;
                    if (session == null) session = new Session();
                    await session.AddPlayer(name, player);
                    _openSessions[session.SessionId] = session;
                    await Package.SendResponseToUser(player, await Serialiser.SerialiseToBytes(new ConnectionInfo { IsSuccessfulJoin = true }));
                }

                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
