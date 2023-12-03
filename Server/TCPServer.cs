using System.Net;
using System.Net.Sockets;
using PackageHelper;
using DatabaseController;
using System.Text;

namespace Server
{
    public class TCPServer
    {
        Dictionary<string, List<string>> _sessions;
        static Semaphore gamesSem = new Semaphore(3, 3);
        Dictionary<Socket, string> _players;
        readonly Dictionary<Socket, string> _clients;
        DBController _db;
        Distributor _distributor;

        readonly Socket _listener;

        public TCPServer(IPAddress ipAddress, int port)
        {
            _listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _listener.Bind(new IPEndPoint(ipAddress, port));
            _sessions = new();
            _players = new();
            _clients = new();
            _db = new("");
            _distributor = new();
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
                var contentLength = await socket.ReceiveAsync(buffer, SocketFlags.None);

                if (PackageChecker.IsQueryValid(buffer, contentLength))
                {
                    if (PackageChecker.IsSignUp(buffer))
                    {
                        var content = new List<byte>();
                        content.AddRange(Package.GetContent(buffer, contentLength));

                        if (PackageChecker.IsPartial(buffer))
                        {
                            while (socket.Connected)
                            {
                                contentLength = await socket.ReceiveAsync(buffer, SocketFlags.None);
                                if (!PackageChecker.IsPartial(buffer))
                                {
                                    content.AddRange(Package.GetContent(buffer, contentLength));
                                    break;
                                }
                                content.AddRange(Package.GetContent(buffer, contentLength));
                            }
                        }

                        var message = await _db.AddUser(await CustomJsonSerialiser.Deserialise<User>(Encoding.UTF8.GetString(content.ToArray())));

                    }
                    else if (PackageChecker.IsSignIn(buffer))
                    {
                        var content = new List<byte>();
                        content.AddRange(Package.GetContent(buffer, contentLength));

                        if (PackageChecker.IsPartial(buffer))
                        {
                            while (socket.Connected)
                            {
                                contentLength = await socket.ReceiveAsync(buffer, SocketFlags.None);
                                if (!PackageChecker.IsPartial(buffer))
                                {
                                    content.AddRange(Package.GetContent(buffer, contentLength));
                                    break;
                                }
                                content.AddRange(Package.GetContent(buffer, contentLength));
                            }
                        }

                        var message = await _db.CheckUser(await CustomJsonSerialiser.Deserialise<User>(Encoding.UTF8.GetString(content.ToArray())));
                    }
                    else
                    {

                        while(socket.Connected)
                        {
                            if (PackageChecker.IsJoin(buffer))
                            {

                            }

                        }
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
            }
            finally
            {
                _clients.Remove(socket);
                await socket.DisconnectAsync(false);
            }
        }

        async Task ProcessSessionAsync(Socket socket)
        {

        }
        //TODO: Broadcast
    }
}
