using System;

namespace ElephantSocial.Chat.Model
{
    public class MessageAcknowledgedEventArgs : EventArgs
    {
        public string ChannelId { get; set; }
        public AckPayloadWrapper AckInfo { get; set; }
    }
}