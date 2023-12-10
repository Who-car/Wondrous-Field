using Server;

namespace ServerApp
{
    public class Program
    {
        static async Task Main(string[] args)
        {
            var server = new TCPServer(new System.Net.IPAddress(1234543), 5050);
            await server.RunServerAsync();
        }
    }
}