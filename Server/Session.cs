using System.Net.Sockets;
using PackageHelper;
using ClientServerTransfer;

namespace Server
{
    public class Session
    {
        public string SessionId { get; init; }

        readonly Dictionary<Socket, string> _players = new(3);
        int _playersCount = 0;

        public char[] Word { get; init; }
        public string Riddle { get; init; }
        public char[] GuessedLetters { get; init; }

        public Socket currentPlayer;

        public bool SessionIsFull => _playersCount >= 3;

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
                    await Package.SendResponseToUser(p, await Serialiser.SerialiseToBytes(new SessionInfo { Riddle = this.Riddle, SessionId = this.SessionId, Word = this.Word, IsGuessed = false, IsWin = false}));
                }
            }
        }

        public async Task NameTheLetter(Socket player, char letter)
        {
            //TODO: game
        }

        public async Task NameTheWord(Socket player, char[] word)
        {

        }
    }
}
