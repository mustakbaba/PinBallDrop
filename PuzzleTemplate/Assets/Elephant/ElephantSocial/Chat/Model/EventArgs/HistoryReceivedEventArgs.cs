using System;

namespace ElephantSocial.Chat.Model
{
    public class HistoryReceivedEventArgs : EventArgs
    {
        public string ChannelId { get; set; }
        public HistoryDeliveredPayload History { get; set; }
    }
}