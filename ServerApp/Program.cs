using Server;
using System.Net.Sockets;

namespace ServerApp
{
    public class Program
    {
        static async Task Main(string[] args)
        {
            var ip = new System.Net.IPAddress(new byte[] { 127, 0, 0, 1 });
            var port = 5051;
            var server = new TCPServer(ip, port);
            await server.RunServerAsync();
        }
    }
}