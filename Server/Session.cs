﻿using System.Net.Sockets;
using PackageHelper;
using ClientServerTransfer;
using System.Diagnostics.Metrics;
using System.Text;

namespace Server
{
    public class Session
    {
        public string SessionId { get; init; }

        readonly Dictionary<Socket, Player> _players = new(3);
        int _playersCount = 0;
        readonly Semaphore sem = new(1, 1);

        public char[]? Word { get; set; }
        public string? Riddle { get; set; }
        public char[]? GuessedLetters { get; set; }
        public bool IsPrivate { get; init; }
        public bool IsInProcess { get; set; }

        Socket? _currentPlayer;
        readonly TCPServer _server;
        int _currentPlayerIndex = 0;

        public bool IsFull => _playersCount >= 3;

        public Session(string sessionId, TCPServer server)
        {
            SessionId = sessionId;
            _server = server;
        }

        async Task GenerateRiddle()
        {
            Word = new char[3];
            Riddle = "";
        }

        public async Task StopGame()
        {
            if (IsInProcess)
            {
                foreach (var p in _players.Keys)
                {
                    if (p.Connected)
                    {
                        await Package.SendContentToSocket(p, await Serialiser.SerialiseToBytesAsync(new Message { Content = "Game over" }), Command.SendMessage, QueryType.Request).ConfigureAwait(false);
                        await p.DisconnectAsync(false).ConfigureAwait(false);
                    }
                }
            }
        }

        public async Task<bool> AddPlayer(Player playerInfo, Socket player)
        {
            try
            {
                sem.WaitOne();
                if (IsFull)
                {
                    throw new Exception();
                }

                _players[player] = playerInfo;
                Interlocked.Increment(ref _playersCount);

                if (IsFull)
                {
                    IsInProcess = true;
                    await GenerateRiddle();
                    await NotifyPlayers(await Serialiser.SerialiseToBytesAsync(new SessionInfo
                    {
                        Riddle = this.Riddle,
                        SessionId = this.SessionId,
                        Word = this.Word,
                        IsGuessed = false,
                        IsWin = false,
                        CurrentPlayer = _players.First().Value
                    }));
                    await NotifyServerAboutStartingGame();
                }
                sem.Release();
                return true;
            }
            catch
            {
                return false;
            }
        }

        async Task NotifyPlayers(byte[] content)
        {
            foreach (var p in _players.Keys)
            {
                if(p.Connected)
                {
                    await Package.SendResponseToUser(p, content);
                } 
            }
        }

        async Task NotifyPlayers(byte[] content, Socket exceptPlayer)
        {
            foreach (var p in _players.Keys)
            {
                if (p.Connected && !p.Equals(exceptPlayer))
                {
                    await Package.SendResponseToUser(p, content);
                }
            }
        }

        Player NextPlayer()
        {
            _currentPlayerIndex++;
            if(_currentPlayerIndex >= _playersCount)
            {
                _currentPlayerIndex = 0;
            }

            int i = 0;
            foreach(var p in _players)
            {
                if(i == _currentPlayerIndex)
                {
                    _currentPlayer = p.Key;
                    return p.Value;
                }
                i++;
            }

            return default!;
        }

        public async Task NameTheLetter(Socket player, char letter)
        {
            var info = new SessionInfo();
            
            if (IsExistedPlayer(player) && player.Equals(_currentPlayer))
            {
                for(var i = 0; i < Word!.Length; i++)
                {
                    if (Word[i].Equals(letter))
                    {
                        GuessedLetters![i] = letter;
                        info.IsGuessed = true;
                        info.Word = GuessedLetters;
                    }
                }

                info.IsWin = Word.SequenceEqual(GuessedLetters!);

                info.CurrentPlayer = NextPlayer();

                foreach(var p in _players.Keys)
                {
                    await Package.SendResponseToUser(p, await Serialiser.SerialiseToBytesAsync(info)).ConfigureAwait(false);
                }

                if(info.IsWin)
                {
                    await StopGame();
                }
            }
            else
            {
                throw new Exception();
            }
        }

        public async Task NameTheWord(Socket player, char[] word)
        {
            var info = new SessionInfo();

            if (IsExistedPlayer(player))
            {
                info.IsWin = Word!.SequenceEqual(word);

                info.CurrentPlayer = NextPlayer();

                foreach (var p in _players.Keys)
                {
                    await Package.SendResponseToUser(p, await Serialiser.SerialiseToBytesAsync(info)).ConfigureAwait(false);
                }

                if (info.IsWin)
                {
                    await StopGame();
                }
            }
            else
            {
                throw new Exception();
            }
        }

        bool IsExistedPlayer(Socket player)
        {
            return _players.ContainsKey(player);
        }

        public async Task SendMessageToPlayers(Message message, Socket sender)
        {
            foreach (var p in _players.Keys)
            {
                if(!p.Equals(sender)) await Package.SendResponseToUser(p, await Serialiser.SerialiseToBytesAsync(message), true).ConfigureAwait(false);
            }
        }

        async Task NotifyServerAboutStartingGame()
        {
            await _server.MoveSessionToProcessingSessions(SessionId);
        }
    }
}
