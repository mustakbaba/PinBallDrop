using System.Collections.Generic;

namespace ElephantSocial.Chat.Model
{
    public class HelpMessage : ChatMessage
    {
        public int Max { get; set; }
        public int Received { get; set; }
        public List<string> Senders { get; set; } = new List<string>();
        public bool IsHelpedByMe { get; private set; } = false;

        public HelpMessage(int requestedAmount)
        {
            Type = ChatMessageType.HELP;
            Max = requestedAmount;
        }

        public void Help()
        {
            IsHelpedByMe = true;
        }
    }
}