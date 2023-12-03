using System.Net.Sockets;

namespace Server
{
    public class Distributor
    {
        Queue<Socket> _queue;

        public Distributor()
        {
            _queue = new();
        }

        public void AddWaitingPlayer(Socket socket)
        {
            _queue.Enqueue(socket);
            if (_queue.Count >= 3)
            {
                CreateSessionAndNotifyPlayers();
            }
        }

        public void CreateSessionAndNotifyPlayers()
        {
            if (_queue.Count >= 3)
            {
                for(int i = 0; i < 3; i++)
                {
                    var player = _queue.Dequeue();
                    //TODO: Add to session
                }
                //TODO: Session creating
            }
        }
    }
}
