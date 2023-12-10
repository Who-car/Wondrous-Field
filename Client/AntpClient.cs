using System.Net.Sockets;
using System.Text;
using static PackageHelper.Package;

namespace Client;

public class AntpClient
{
    private readonly Socket _socket = new(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

    public async Task SendRequest()
    {
        
    }
    
    public async Task<string> GetResponse()
    {
        var buffer = new byte[MaxPacketSize];
        var responseContent = new List<byte>();
        do
        {
            var contentLength = await _socket.ReceiveAsync(buffer, SocketFlags.None);

            // if (!IsResponse(buffer[Query]) || !IsSay(buffer[Command]))
            // {
            //     return "Получили неизвестный ответ от сервера!";
            // }

            responseContent.AddRange(GetContent(buffer, contentLength));

            // } while (!IsFull(buffer[Fullness]));
        } while (true);

        return Encoding.UTF8.GetString(responseContent.ToArray());
    }
}