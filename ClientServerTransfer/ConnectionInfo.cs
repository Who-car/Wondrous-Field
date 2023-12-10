namespace ClientServerTransfer
{
    public class ConnectionInfo
    {
        public string? PlayerName { get; set; }
        public string? SessionId { get; set; }
        public bool IsRandomJoin { get; set; }
        public bool IsSuccessfullJoin { get; set; }
    }
}
