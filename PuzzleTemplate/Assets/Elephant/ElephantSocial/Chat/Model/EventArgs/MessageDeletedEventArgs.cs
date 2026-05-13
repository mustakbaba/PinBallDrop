using System;

namespace ElephantSocial.Chat.Model
{
    public class MessageDeletedEventArgs : EventArgs
    {
        public string ChannelId { get; set; }
        public DeletedMessagePayload DeletionInfo { get; set; }
    }
}