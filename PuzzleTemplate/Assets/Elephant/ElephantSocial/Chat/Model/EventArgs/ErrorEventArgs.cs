using System;

namespace ElephantSocial.Chat.Model
{
    public class ErrorEventArgs : EventArgs
    {
        public string Title { get; set; }
        public string Message { get; set; }
    }
}