using System;

namespace ElephantSocial.Chat.Model
{
    public class MessageCreatedEventArgs : EventArgs
    {
        public string ChannelId { get; set; }
        public MessageContentPayload Message { get; set; }
    }
}