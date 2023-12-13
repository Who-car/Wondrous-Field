namespace ClientServerTransfer
{
    public class SessionInfo
    {
        public string? PlayerName { get; set; }
        public string? SessionId { get; set; }
        public char Letter { get; set; }
        public char[]? Word { get; set; }
        public string? Riddle { get; set; }
        public bool IsGuessed { get; set; }
        public bool IsWin { get; set; }
        public string? CurrentPlayer { get; set; }
        public string[] Players { get; set; } = new string[3];
    }
}
