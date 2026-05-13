namespace ElephantSocial.Chat.Model
{
    public class KickMessage : ChatMessage
    {
        public string SocialId { get; set; }
        public string PlayerName { get; set; }
        public string ProfilePicture { get; set; }
        public string TargetSocialId { get; set; }
        public string TargetPlayerName { get; set; }
        public string TargetProfilePicture { get; set; }
        
        public KickMessage()
        {
            Type = ChatMessageType.KICK;
        }
    }
}