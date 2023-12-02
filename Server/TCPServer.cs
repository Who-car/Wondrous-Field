using System.Net;
using System.Net.Sockets;
using PackageHelper;

namespace Server
{
    public class TCPServer
    {
        readonly Socket _listener;

        public TCPServer(IPAddress ipAddress, int port)
        {
            _listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _listener.Bind(new IPEndPoint(ipAddress, port));
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
                    
                }
                else
                {
                    throw new Exception();
                }
            }
            catch
            {
                await socket.DisconnectAsync(false);
            }
        }

        //TODO: Broadcast
    }
}
