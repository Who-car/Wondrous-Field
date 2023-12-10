using System.Net;
using System.Net.Sockets;
using System.Text;
using PackageHelper;
using ClientServerTransfer;
using System.Runtime.InteropServices;

namespace Server
{
    public class TCPServer
    {

        readonly Socket _listener;
        readonly Dictionary<string, Session> _privateSessions;
        readonly Dictionary<string, Session> _freeSessions;
        readonly Dictionary<string, Session> _gameSessions;

        public TCPServer(IPAddress ipAddress, int port)
        {
            _listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _listener.Bind(new IPEndPoint(ipAddress, port));
            _privateSessions = new();
            _freeSessions = new();
            _gameSessions = new();
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
                            await ProcessClientSocketAsync(clientSocket));

                } while (true);
            }
            catch (Exception ex)
            {
                //TODO: журналирование
            }
            finally
            {
                _listener.Close();
                Console.WriteLine("...Сервер завершил работу...");
            }
        }

        async Task ProcessClientSocketAsync(Socket socket)
        {
            try
            {
                var query = await Package.GetFullContent(socket);

                if (query.Command!.Equals(Command.CreateSession))
                {
                    await CreateSession(Encoding.UTF8.GetString(query.Body!), socket);
                    await ListenSocketInLoopAsync(socket);
                }
                else if (query.Command.Equals(Command.Join))
                {
                    await JoinToSession(Encoding.UTF8.GetString(query.Body!), socket, "");
                    await ListenSocketInLoopAsync(socket);
                }
                else
                {
                    throw new Exception();
                }
            }
            catch
            {
                //TODO: Exception
                await socket.DisconnectAsync(false);
            }
        }

        async Task ListenSocketInLoopAsync(Socket socket)
        {
            Query query;

            while (socket.Connected)
            {
                query = await Package.GetFullContent(socket);

                if (query.Command!.Equals(Command.NameTheLetter))
                {
                    var sessionInfo = await Serialiser.Deserialise<SessionInfo>(query.Body!);
                    if(_gameSessions.ContainsKey(sessionInfo.SessionId!))
                    {
                        await _gameSessions[sessionInfo.SessionId!].NameTheLetter(socket, sessionInfo.Letter);
                    }
                }
                else if (query.Command!.Equals(Command.NameTheWord))
                {
                    var sessionInfo = await Serialiser.Deserialise<SessionInfo>(query.Body!);
                    if (_gameSessions.ContainsKey(sessionInfo.SessionId!))
                    {
                        await _gameSessions[sessionInfo.SessionId!].NameTheWord(socket, sessionInfo.Word!);
                    }
                }
                else if (query.Command!.Equals(Command.Post))
                {

                }
                else if (query.Command!.Equals(Command.SendMessage))
                {
                    await SendMessageToPlayers(socket, query.Body!);
                }
            }
        }

        async Task SendMessageToPlayers(Socket socket, byte[] message)
        {
            //TODO: broadcast
        }

        async Task<bool> CreateSession(string name, Socket player)
        {
            try
            {
                Session session = new();
                session.AddPlayer(name, player);
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
                        _privateSessions[sessionId].AddPlayer(name, player);
                        await Package.SendResponseToUser(player, await Serialiser.SerialiseToBytes(new ConnectionInfo { SessionId = "", IsSuccessfulJoin = true }));
                    }
                }
                else
                {
                    var session = _freeSessions.Where(x => !x.Value.SessionIsFull).First().Value;
                    if (session == null) session = new Session();
                    session.AddPlayer(name, player);
                    _freeSessions[session.SessionId] = session;
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
