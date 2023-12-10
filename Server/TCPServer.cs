using System.Net;
using System.Net.Sockets;
using PackageHelper;

namespace Server
{
    public class TCPServer
    {

        readonly Socket _listener;
        readonly Dictionary<string, Session> _sessions;

        public TCPServer(IPAddress ipAddress, int port)
        {
            _listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _listener.Bind(new IPEndPoint(ipAddress, port));
            _sessions = new();
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
                var buffer = new byte[Package.MaxPackageSize];
                var packageLength = await socket.ReceiveAsync(buffer, SocketFlags.None);

                if (Package.IsPackageValid(buffer, packageLength))
                {
                    if (Package.IsCreateSession(buffer))
                    {

                        CreateSession("", socket);
                    }
                    else if (Package.IsJoin(buffer))
                    {

                    }
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

        bool CreateSession(string name, Socket player)
        {
            try
            {
                Session session = new();
                session.AddPlayer(name, player);

                _sessions.Add(session.SessionId, session);
                return true;
            }
            catch
            {
                return false;
            }
        }

        async Task<List<byte>> GetFullContent(Socket socket)
        {
            var content = new List<byte>();
            while (socket.Connected)
            {
                
            }
        }
    }
}
