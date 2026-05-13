namespace ElephantSocial.Chat.Model
{
    public class TextMessage : ChatMessage
    {
        public string Text { get; set; }

        public TextMessage()
        {
            Type = ChatMessageType.TEXT;
        }
    }
}