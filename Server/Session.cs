using System.Net.Sockets;

namespace Server
{
    public class Session
    {
        public string SessionId { get; init; }

        readonly Dictionary<string, Socket> _players = new(3);
        int _playersCount = 0;

        public char[] Word { get; init; }
        public string Riddle { get; init; }
        public char[] GuessedLetters { get; init; }

        public Socket currentPlayer;

        public bool SessionIsFull => _playersCount >= 3;

        public void AddPlayer(string name, Socket player)
        {
            if(_playersCount >= 3)
            {
                throw new Exception();
            }

            _players[name] = player;
            _playersCount++;
        }
    }
}
