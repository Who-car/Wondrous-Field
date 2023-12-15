using System.Net.Sockets;
using PackageHelper;
using ClientServerTransfer;
using System.Diagnostics.Metrics;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

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
        public readonly int[] Scores = new int[8] { 500, 100, 300, 600, -100, 200, 400, -200 };

        Socket? _currentPlayer;
        int _currentPlayerObtainedScore = 0;
        readonly TCPServer _server;
        int _currentPlayerIndex = 0;

        public bool IsFull => _playersCount >= 3;

        public Session(string sessionId, TCPServer server, bool isPrivate = false)
        {
            SessionId = sessionId;
            _server = server;
            IsPrivate = isPrivate;
        }

        async Task GenerateRiddle()
        {
            var json = await File.ReadAllTextAsync("riddles.json");

            using var document = JsonDocument.Parse(json);
            var root = document.RootElement;

            if (root.ValueKind != JsonValueKind.Array)
                throw new InvalidOperationException("JSON must be an array.");

            var count = root.GetArrayLength();
            if (count == 0)
                throw new InvalidOperationException("JSON array is empty.");

            var random = new Random();
            var randomIndex = random.Next(count);

            var randomItem = root[randomIndex];
            Riddle = randomItem.GetProperty("Riddle").GetString()!;
            Word = randomItem.GetProperty("Word").GetString()!.ToUpper().ToCharArray();
            GuessedLetters = new char[Word.Length];
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
                        Word = this.GuessedLetters,
                        IsGuessed = false,
                        IsWin = false,
                        CurrentPlayer = _players.First().Value
                    }));
                    await NotifyServerAboutStartingGame();
                    _currentPlayer = _players.First().Key;
                }
                sem.Release();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task NameTheLetter(Socket player, char letter)
        {            
            if (IsExistedPlayer(player) && player.Equals(_currentPlayer))
            {
                var info = new SessionInfo() { SessionId = this.SessionId };

                for (var i = 0; i < Word!.Length; i++)
                {
                    if (Word[i].Equals(char.ToUpper(letter)))
                    {
                        GuessedLetters![i] = char.ToUpper(letter);
                        info.IsGuessed = true;
                        info.Word = GuessedLetters;
                    }
                }

                if(!info.IsGuessed)
                {
                    _players[player].Points -= _currentPlayerObtainedScore;
                }

                info.IsWin = Word.SequenceEqual(GuessedLetters!);
                info.Word = GuessedLetters;
                info.CurrentPlayer = NextPlayer();

                await NotifyPlayers(await Serialiser.SerialiseToBytesAsync(info));

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

        public async Task NameTheWord(Socket player, char[] word)
        {
            if (IsExistedPlayer(player))
            {
                var info = new SessionInfo { SessionId = this.SessionId };

                info.IsWin = Word!.SequenceEqual(word.ToString()!.ToUpper().ToArray());
                if (!info.IsWin) _players[player].Points -= _currentPlayerObtainedScore;
                info.CurrentPlayer = NextPlayer();

                await NotifyPlayers(await Serialiser.SerialiseToBytesAsync(info));

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

        public async Task GetScore(Socket player)
        {
            var randomIndex = new Random().Next(0, 8);
            _players[player].Points += Scores[randomIndex];
            _currentPlayerObtainedScore = Scores[randomIndex];
            
            var info = new SessionInfo { CurrentPlayer = _players[player] };
            
            foreach (var p in _players.Keys)
            {
                await Package.SendResponseToUser(p, await Serialiser.SerialiseToBytesAsync(info));
            }
        }

        public async Task StopGame()
        {
            if (IsInProcess)
            {
                await _server.DeleteSession(SessionId);
            }
        }

        async Task NotifyPlayers(byte[] content)
        {
            foreach (var p in _players.Keys)
            {
                if (!p.Connected)
                {
                    await RemovePlayer(p);
                    continue;
                }
                await Package.SendResponseToUser(p, content);
            }
        }

        async Task NotifyPlayers(byte[] content, Socket exceptPlayer)
        {
            foreach (var p in _players.Keys)
            {
                if (!p.Connected)
                {
                    await RemovePlayer(p);
                    continue;
                }    
                if (!p.Equals(exceptPlayer))
                {
                    await Package.SendResponseToUser(p, content);
                }
            }
        }

        public async Task SendMessageToPlayers(Message message, Socket sender)
        {
            foreach (var p in _players.Keys)
            {
                if(!p.Equals(sender)) await Package.SendResponseToUser(p, await Serialiser.SerialiseToBytesAsync(message), true);
            }
        }

        async Task NotifyServerAboutStartingGame()
        {
            await _server.MoveSessionToProcessingSessions(SessionId);
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

        bool IsExistedPlayer(Socket player)
        {
            return _players.ContainsKey(player);
        }

        public async Task RemovePlayer(Socket player)
        {
            //TODO:removing
            if(IsExistedPlayer(player))
            {
                _players.Remove(player);
                var ind = GetPlayerIndex(player);
                if(true)
                {

                }    

                var info = new SessionInfo { SessionId = this.SessionId, CurrentPlayer = _players[_currentPlayer!] };

                await NotifyPlayers(await Serialiser.SerialiseToBytesAsync(info));
            }
        }

        int GetPlayerIndex(Socket player)
        {
            var i = 0;
            foreach(var p in _players)
            {
                if (player.Equals(p)) return i;
                i++;
            }

            return i;
        }
    }
}
