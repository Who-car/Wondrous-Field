using System.Net.Sockets;
using System.Text;
using PackageHelper;
using static PackageHelper.Package;

namespace Client;

public class AntpClient
{
    private readonly Socket _socket = new(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

    public async Task SendRequest(byte[] content, byte[] command)
    {
        GetPackages(content, command, QueryType.Request);
    }
    
    public async Task<string> GetResponse()
    {
        var buffer = new byte[MaxPackageSize];
        var responseContent = new List<byte>();
        do
        {
            var contentLength = await _socket.ReceiveAsync(buffer, SocketFlags.None);

            if (!IsResponse(buffer))
            {
                return "Получили неизвестный ответ от сервера!";
            }

            responseContent.AddRange(GetContent(buffer, contentLength));

        } while (!IsFull(buffer));

        return Encoding.UTF8.GetString(responseContent.ToArray());
    }
}