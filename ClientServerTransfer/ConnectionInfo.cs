namespace ClientServerTransfer
{
    public class ConnectionInfo
    {
        public Player PlayerInfo { get; set; }
        public string? SessionId { get; set; }
        public bool IsRandomJoin { get; set; }
        public bool IsSuccessfulJoin { get; set; }
    }
}
