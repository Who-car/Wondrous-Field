﻿namespace ClientServerTransfer
{
    public class SessionInfo
    { 
        public string SessionId { get; set; }
        public char Letter { get; set; }
        public char[]? Word { get; set; }
        public string? Riddle { get; set; }
        public bool IsGuessed { get; set; }
        public bool IsWin { get; set; } 
        public Player CurrentPlayer { get; set; }
    }
}
