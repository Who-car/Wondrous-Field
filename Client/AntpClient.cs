using System.Net.Sockets;
using ClientServerTransfer;
using PackageHelper;
using static PackageHelper.Command;
using static PackageHelper.PackageFullness;
using static PackageHelper.QueryType;
using static PackageHelper.Package;

namespace Client;

public class AntpClient 
{
    private readonly Socket _socket = new(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
    private readonly Guid _clientId = Guid.NewGuid();
    private bool _gameStarted;
    public SessionInfo SessionInfo { get; set; }
    public bool IsTurn => _clientId == SessionInfo.CurrentPlayerId;
    public delegate void ProgressHandler(SessionInfo sessionInfo);
    public delegate void MessageHandler(Message message);
    public delegate void SessionHandler(SessionInfo connectionInfo);
    public ProgressHandler OnTurn { get; set; }
    public MessageHandler MessageReceived { get; set; }
    public SessionHandler OnGameStart { get; set; }
    
    public async Task<ConnectionInfo> StartNewGame(string playerName="Player")
    {
        try
        {
            await _socket.ConnectAsync("localhost", 5000);
            var package = new PackageBuilder(playerName.Length)
                .SetCommand(CreateSession)
                .SetFullness(Full)
                .SetQueryType(Request)
                .SetContent(await Serialiser.SerialiseToBytes(playerName))
                .Build();
            await _socket.SendAsync(package, SocketFlags.None);
            var buffer = new byte[MaxPackageSize];
            var bufferLength = await _socket.ReceiveAsync(buffer, SocketFlags.None);
            if (!IsPackageValid(buffer, bufferLength)
                || !IsResponse(buffer)
                || !IsFull(buffer)
                || !IsCreateSession(buffer))
                throw new Exception("Couldn't create session. Try again later");
            var content = GetContent(buffer, bufferLength);
            return await Serialiser.Deserialise<ConnectionInfo>(content);
        }
        catch (Exception e)
        {
            Console.WriteLine(e.ToString());
            return new ConnectionInfo()
            {
                IsSuccessfulJoin = false
            };
        }
        finally
        {
            // TODO: как работает getAwaiter? Может надо просто запускать бэкграунд таск
            // TODO: этот блок не должен отрабатывать, если попали в catch. Но вернуть из метода как-то значения надо
            var listenTask = Listen();
            listenTask.GetAwaiter().GetResult();
        }
    }

    public async Task<ConnectionInfo> JoinGame(string sessionId, string playerName="Player")
    {
        try
        {
            await _socket.ConnectAsync("localhost", 5000);
            var package = new PackageBuilder(playerName.Length)
                .SetCommand(Join)
                .SetFullness(Full)
                .SetQueryType(Request)
                .SetContent(await Serialiser.SerialiseToBytes(new ConnectionInfo() {PlayerName = playerName, SessionId = sessionId}))
                .Build();
            await _socket.SendAsync(package, SocketFlags.None);
            var buffer = new byte[MaxPackageSize];
            var bufferLength = await _socket.ReceiveAsync(buffer, SocketFlags.None);
            if (!IsPackageValid(buffer, bufferLength)
                || !IsResponse(buffer)
                || !IsFull(buffer)
                || !IsJoin(buffer))
                throw new Exception("Couldn't join session. Try again later");
            var content = GetContent(buffer, bufferLength);
            return await Serialiser.Deserialise<ConnectionInfo>(content);
        }
        catch (Exception e)
        {
            Console.WriteLine(e.ToString());
            return new ConnectionInfo()
            {
                IsSuccessfulJoin = false
            };
        }
        finally
        {
            var listenTask = Listen();
            listenTask.GetAwaiter().GetResult();
        }
    }

    private async Task Listen()
    {
        do
        {
            var buffer = new byte[MaxPackageSize];
            var bufferLength = await _socket.ReceiveAsync(buffer, SocketFlags.None);
            if (!IsPackageValid(buffer, bufferLength)
                || !IsResponse(buffer))
                throw new Exception("Couldn't join session. Try again later");
            var content = GetContent(buffer, bufferLength);

            if (IsMessage(buffer))
                MessageReceived.Invoke(await Serialiser.Deserialise<Message>(content));

            if (IsPost(buffer))
            {
                var sessionInfo = await Serialiser.Deserialise<SessionInfo>(content);
                SessionInfo = sessionInfo;
                if (_gameStarted)
                {
                    OnTurn.Invoke(sessionInfo);
                }
                else
                {
                    OnGameStart.Invoke(sessionInfo);
                    _gameStarted = true;
                }
            }
        } while (_socket.Connected);
    }

    public async Task Exit()
    { 
        await _socket.DisconnectAsync(false);
        _socket.Dispose();
    }
}