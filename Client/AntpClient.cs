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
    // TODO: Guid на серваке
    private readonly Player _player = new Player() {Id = Guid.NewGuid()};
    private bool _gameStarted;
    private int _prevScore;
    public Player Player => _player;
    
    private readonly IPAddress _ip = new IPAddress(new byte[] { 127, 0, 0, 1 });
    private readonly int _port = 5051;
    public SessionInfo SessionInfo { get; set; } 
    public bool IsTurn => _player.Id == SessionInfo.CurrentPlayer.Id;
    public delegate void MessageHandler(Message message);
    public delegate void SessionHandler(SessionInfo connectionInfo);
    public delegate void WinHandler(Player winner);
    public delegate void ScoreHandler(int score);
    public SessionHandler OnTurn { get; set; }
    public SessionHandler OnGameStart { get; set; }
    public ScoreHandler OnGetScore { get; set; }
    public WinHandler GameOver { get; set; }
    public MessageHandler MessageReceived { get; set; }
    
    public async Task<ConnectionInfo> StartNewGame()
    {
        try
        {
            await _socket.ConnectAsync(_ip, _port); 
            var connection = await Serialiser.SerialiseToBytesAsync(new ConnectionInfo { PlayerInfo = _player });
            var package = new PackageBuilder(connection.Length)
                .SetCommand(CreateSession)
                .SetFullness(Full)
                .SetQueryType(Request)
                .SetContent(connection)
                .Build();
            await _socket.SendAsync(package, SocketFlags.None);
            var content = await GetFullContent(_socket);
            return await Serialiser.DeserialiseAsync<ConnectionInfo>(content.Body!);
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
            Task.Run(Listen);
        }
    }

    public async Task<ConnectionInfo> JoinGame(string sessionId)
    {
        try
        {
            await _socket.ConnectAsync(_ip, _port);
            var connection = await Serialiser.SerialiseToBytesAsync(new ConnectionInfo
            {
                PlayerInfo = _player,
                SessionId = sessionId,
                IsRandomJoin = false
            });

            var package = new PackageBuilder(connection.Length)
                .SetCommand(Join)
                .SetFullness(Full)
                .SetQueryType(Request)
                .SetContent(connection)
                .Build();
            await _socket.SendAsync(package, SocketFlags.None);
            var content = await GetFullContent(_socket);
            return await Serialiser.DeserialiseAsync<ConnectionInfo>(content.Body!);
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
            Task.Run(Listen);
        }
    }

    private async Task Listen()
    {
        do
        {
            var content = await GetFullContent(_socket);

            if (IsMessage(content.Command!))
                MessageReceived.Invoke(await Serialiser.DeserialiseAsync<Message>(content.Body));

            if (IsScore(content.Command!))
            {
                var sessionInfo = await Serialiser.DeserialiseAsync<SessionInfo>(content.Body);
                OnGetScore.Invoke(sessionInfo.CurrentPlayer.Points-_prevScore);
            }
            
            if (IsPost(content.Command!))
            {
                var sessionInfo = await Serialiser.DeserialiseAsync<SessionInfo>(content.Body);
                SessionInfo = sessionInfo;
                if (sessionInfo.IsWin)
                {
                    GameOver.Invoke(sessionInfo.CurrentPlayer);
                    continue;
                }
                if (!_gameStarted)
                {
                    OnGameStart.Invoke(sessionInfo);
                    _gameStarted = true;
                }
                OnTurn.Invoke(sessionInfo);
            }
        } while (_socket.Connected);
    }

    public async Task ReportLetter(char letter, int potentialScore)
    {
        if (!_socket.Connected)
            throw new ChannelClosedException("Internet connection error");
        var session = await Serialiser.SerialiseToBytesAsync(new SessionInfo
        {
            Letter = letter,
            SessionId = SessionInfo.SessionId,
            CurrentPlayer = _player
        });
        var packages = GetPackages(session, NameTheLetter, Request);
        foreach (var package in packages)
            await _socket.SendAsync(package, SocketFlags.None);
    }
    
    public async Task ReportWord(string word, int potentialScore)
    {
        if (!_socket.Connected)
            throw new ChannelClosedException("Internet connection error");
        var session = await Serialiser.SerialiseToBytesAsync(new SessionInfo
        {
            Word = word.ToCharArray(),
            SessionId = SessionInfo.SessionId,
            CurrentPlayer = _player 
        });
        var packages = GetPackages(session, NameTheWord, Request);
        foreach (var package in packages)
            await _socket.SendAsync(package, SocketFlags.None);
    }
    
    public async Task ReportMessage(string message)
    {
        if (!_socket.Connected)
            throw new ChannelClosedException("Internet connection error");
        var request = await Serialiser.SerialiseToBytesAsync(new Message
        { 
            Player = _player,
            SessionId = SessionInfo.SessionId,
            Content = message
        });
        var packages = GetPackages(request, SendMessage, Request);
        foreach (var package in packages)
            await _socket.SendAsync(package, SocketFlags.None);
    }
    
    public async Task ReportScoreRequest()
    {
        if (!_socket.Connected)
            throw new ChannelClosedException("Internet connection error");
        _prevScore = _player.Points;
        var request = await Serialiser.SerialiseToBytesAsync(new SessionInfo
        { 
            CurrentPlayer = _player,
            SessionId = SessionInfo.SessionId
        });
        var packages = GetPackages(request, Score, Request);
        foreach (var package in packages)
            await _socket.SendAsync(package, SocketFlags.None);
    }

    public async Task Exit()
    { 
        await _socket.DisconnectAsync(false);
        _socket.Dispose();
    }
}