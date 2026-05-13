using System;

namespace ElephantSocial.Chat.Model
{
    public class MessageUpdatedEventArgs : EventArgs
    {
        public string ChannelId { get; set; }
        public MessageContentPayload Message { get; set; }
    }
}