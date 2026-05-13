namespace ElephantSocial.Chat.Model
{
    public class JoinRejectMessage : ChatMessage
    {
        public string SocialId { get; set; }
        public string PlayerName { get; set; }
        public string ProfilePicture { get; set; }
        public string TargetSocialId { get; set; }
        public string TargetPlayerName { get; set; }
        public string TargetProfilePicture { get; set; }
        
        public JoinRejectMessage()
        {
            Type = ChatMessageType.JOIN_REJECT;
        }
    }
}