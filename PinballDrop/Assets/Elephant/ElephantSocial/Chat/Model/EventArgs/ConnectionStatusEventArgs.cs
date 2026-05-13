using System;

namespace ElephantSocial.Chat.Model
{
    public class ConnectionStatusEventArgs : EventArgs
    {
        public bool IsConnected { get; set; }
        public string Reason { get; set; }
    }
}