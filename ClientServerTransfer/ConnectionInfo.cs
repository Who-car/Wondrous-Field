namespace ClientServerTransfer
{
    public class ConnectionInfo
    {
        public string? PlayerName { get; set; }
        public Guid PlayerId { get; set; }
        public string? SessionId { get; set; }
        public bool IsRandomJoin { get; set; }
        public bool IsSuccessfulJoin { get; set; }
    }
}
