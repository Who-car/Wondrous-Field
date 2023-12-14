﻿using System.Net;
using System.Net.Sockets;
using System.Threading.Channels;
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
    private readonly IPAddress _ip = new IPAddress(new byte[] { 127, 0, 0, 1 });
    private readonly int _port = 5051;
    private string _playerName;
    private bool _gameStarted;
    public SessionInfo SessionInfo { get; set; }
    public bool IsTurn => _clientId == SessionInfo.CurrentPlayerId;
    public delegate void MessageHandler(Message message);
    public delegate void SessionHandler(SessionInfo connectionInfo);
    public delegate void WinHandler(string winner);
    public SessionHandler OnTurn { get; set; }
    public SessionHandler OnGameStart { get; set; }
    public WinHandler GameOver { get; set; }
    public MessageHandler MessageReceived { get; set; }
    
    
    public async Task<ConnectionInfo> StartNewGame(string playerName="Player")
    {
        try
        {
            await _socket.ConnectAsync(_ip, _port);
            var connection = await Serialiser.SerialiseToBytesAsync(new ConnectionInfo
            {
                PlayerName = playerName, 
                PlayerId = _clientId
            }).ConfigureAwait(false);
            var package = new PackageBuilder(connection.Length)
                .SetCommand(CreateSession)
                .SetFullness(Full)
                .SetQueryType(Request)
                .SetContent(connection)
                .Build();
            await _socket.SendAsync(package, SocketFlags.None).ConfigureAwait(false);
            var buffer = new byte[MaxPackageSize];
            var bufferLength = await _socket.ReceiveAsync(buffer, SocketFlags.None).ConfigureAwait(false);
            if (!IsPackageValid(buffer, bufferLength)
                || !IsResponse(buffer)
                || !IsFull(buffer)
                || !IsCreateSession(buffer))
                throw new Exception("Couldn't create session. Try again later");
            var content = GetContent(buffer, bufferLength);
            _playerName = playerName;
            return await Serialiser.DeserialiseAsync<ConnectionInfo>(content).ConfigureAwait(false);
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
            var listenTask = Listen();
            listenTask.GetAwaiter().GetResult();
        }
    }

    public async Task<ConnectionInfo> JoinGame(string sessionId, string playerName="Player")
    {
        try
        {
            await _socket.ConnectAsync(_ip, _port);
            var connection = await Serialiser.SerialiseToBytesAsync(new ConnectionInfo()
            {
                PlayerName = playerName, 
                PlayerId = _clientId,
                SessionId = sessionId
            });
            var package = new PackageBuilder(connection.Length)
                .SetCommand(Join)
                .SetFullness(Full)
                .SetQueryType(Request)
                .SetContent(connection)
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
            _playerName = playerName;
            return await Serialiser.DeserialiseAsync<ConnectionInfo>(content);
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
                continue;
            var content = GetContent(buffer, bufferLength);

            if (IsMessage(buffer))
                MessageReceived.Invoke(await Serialiser.DeserialiseAsync<Message>(content));

            if (IsPost(buffer))
            {
                var sessionInfo = await Serialiser.DeserialiseAsync<SessionInfo>(content);
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

    public async Task<bool> CheckLetter(char letter, short letterPosition)
    {
        if (!_socket.Connected)
            throw new ChannelClosedException("Internet connection error");
        var session = await Serialiser.SerialiseToBytesAsync(new SessionInfo
        {
            Letter = letter,
            LetterPosition = letterPosition,
            SessionId = SessionInfo.SessionId,
            CurrentPlayerId = _clientId
        });
        var package = new PackageBuilder(session.Length)
            .SetCommand(NameTheLetter)
            .SetFullness(Full)
            .SetQueryType(Request)
            .SetContent(session)
            .Build();
        await _socket.SendAsync(package, SocketFlags.None);
        var buffer = new byte[MaxPackageSize];
        var bufferLength = await _socket.ReceiveAsync(buffer, SocketFlags.None);
        if (!IsPackageValid(buffer, bufferLength)
            || !IsResponse(buffer)
            || !IsFull(buffer)
            || !IsPost(buffer))
            throw new Exception("Couldn't parse value. Try again later");
        var content = await Serialiser.DeserialiseAsync<SessionInfo>(GetContent(buffer, bufferLength));
        if (content.IsWin)
            GameOver.Invoke(_playerName);
        return content.IsGuessed;
    }
    
    public async Task<bool> CheckWord(string word)
    {
        if (!_socket.Connected)
            throw new ChannelClosedException("Internet connection error");
        var session = await Serialiser.SerialiseToBytesAsync(new SessionInfo
        {
            Word = word.ToCharArray(),
            SessionId = SessionInfo.SessionId,
            CurrentPlayerId = _clientId
        });
        var package = new PackageBuilder(session.Length)
            .SetCommand(NameTheWord)
            .SetFullness(Full)
            .SetQueryType(Request)
            .SetContent(session)
            .Build();
        await _socket.SendAsync(package, SocketFlags.None);
        var buffer = new byte[MaxPackageSize];
        var bufferLength = await _socket.ReceiveAsync(buffer, SocketFlags.None);
        if (!IsPackageValid(buffer, bufferLength)
            || !IsResponse(buffer)
            || !IsFull(buffer)
            || !IsPost(buffer))
            throw new Exception("Couldn't parse value. Try again later");
        var content = await Serialiser.DeserialiseAsync<SessionInfo>(GetContent(buffer, bufferLength));
        if (content.IsWin)
            GameOver.Invoke(_playerName);
        return content.IsGuessed;
    }
    
    public async Task ReportMessage(string message)
    {
        if (!_socket.Connected)
            throw new ChannelClosedException("Internet connection error");
        var request = await Serialiser.SerialiseToBytesAsync(new Message()
        {
            PlayerName = _playerName,
            SessionId = SessionInfo.SessionId,
            Content = message
        });
        var packages = GetPackages(request, SendMessage, Request);
        foreach (var package in packages)
            await _socket.SendAsync(package, SocketFlags.None);
    }

    public async Task Exit()
    { 
        await _socket.DisconnectAsync(false);
        _socket.Dispose();
    }
}