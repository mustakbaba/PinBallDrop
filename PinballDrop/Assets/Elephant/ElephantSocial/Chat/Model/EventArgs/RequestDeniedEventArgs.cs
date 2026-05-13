using System;

namespace ElephantSocial.Chat.Model
{
    public class RequestDeniedEventArgs : EventArgs
    {
        public string ChannelId { get; set; }
        public DeniedPayload DenialInfo { get; set; }
    }
}