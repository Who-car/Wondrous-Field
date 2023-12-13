using System.Net.Sockets;
using PackageHelper;
using ClientServerTransfer;
using System.Diagnostics.Metrics;
using System.Text;

namespace Server
{
    public class Session
    {
        public string SessionId { get; init; }

        readonly Dictionary<Socket, string> _players = new(3);
        int _playersCount = 0;

        public char[]? Word { get; init; }
        public string? Riddle { get; init; }
        public char[]? GuessedLetters { get; init; }

        public Socket? currentPlayer;

        public bool IsFull => _playersCount >= 3;

        public Session(string sessionId)
        {
            SessionId = sessionId;
        }

        public async Task AddPlayer(string name, Socket player)
        {
            if(_playersCount >= 3)
            {
                throw new Exception();
            }

            _players[player] = name;
            _playersCount++;

            if(_playersCount >= 3)
            {
                foreach (var p in _players.Keys)
                {
                    await Package.SendResponseToUser(p, await Serialiser.SerialiseToBytesAsync(new SessionInfo { Riddle = this.Riddle, SessionId = this.SessionId, Word = this.Word, IsGuessed = false, IsWin = false}));
                }
            }
        }

        public async Task NameTheLetter(Socket player, char letter)
        {
            var info = new SessionInfo();
            
            if (IsExistedPlayer(player))
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

                foreach(var p in _players.Keys)
                {
                    await Package.SendResponseToUser(p, await Serialiser.SerialiseToBytesAsync(info));
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

                foreach (var p in _players.Keys)
                {
                    await Package.SendResponseToUser(p, await Serialiser.SerialiseToBytesAsync(info));
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

        public async Task SendMessageToPlayers(string message)
        {
            foreach (var p in _players.Keys)
            {
                await Package.SendResponseToUser(p, await Serialiser.SerialiseToBytesAsync(message));
            }
        }
    }
}
