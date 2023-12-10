using System.Net;
using System.Net.Sockets;
using System.Text;
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
                var query = await GetFullContent(socket);

                while(socket.Connected)
                {
                    if (query.Command.Equals(Command.CreateSession))
                    {
                        CreateSession(Encoding.UTF8.GetString(query.Body), socket);
                    }
                    else if ()
                    {

                    }
                    else
                    {
                        throw new Exception();
                    }
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

        async Task<Query> GetFullContent(Socket socket)
        {
            var content = new List<byte>();
            var buffer = new byte[Package.MaxPackageSize];
            int packageLength;
            byte[] command = new byte[4];

            while (socket.Connected)
            {
                packageLength = await socket.ReceiveAsync(buffer, SocketFlags.None);

                if (!Package.IsPackageValid(buffer, packageLength)) throw new Exception();

                command = Package.GetCommand(buffer);
                content.AddRange(Package.GetContent(buffer, packageLength));

                if(Package.IsPartial(buffer))
                {
                    continue;
                }
                break;
            }

            return new Query { Body = content.ToArray(), Command = command };
        }
    }
}
