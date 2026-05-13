using System.Collections.Generic;

namespace ElephantSocial.Chat.Model
{
    public class RewardMessage : ChatMessage
    {
        public string RewardId { get; set; }
        public int MaxReceiver { get; set; }
        public List<string> Receivers { get; set; } = new List<string>();

        public RewardMessage()
        {
            Type = ChatMessageType.REWARD;
        }
    }
}