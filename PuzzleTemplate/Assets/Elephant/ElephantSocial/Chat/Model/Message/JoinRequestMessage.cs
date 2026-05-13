namespace ElephantSocial.Chat.Model
{
    public class JoinRequestMessage : ChatMessage
    {
        public string RequestingSocialId { get; set; }
        public string RequestingPlayerName { get; set; }
        public string RequestingProfilePicture { get; set; }

        public JoinRequestMessage()
        {
            Type = ChatMessageType.JOIN_REQUEST;
        }
    }
}