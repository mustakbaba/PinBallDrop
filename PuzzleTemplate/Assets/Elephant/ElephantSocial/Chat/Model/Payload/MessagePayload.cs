using Newtonsoft.Json;

namespace ElephantSocial.Chat.Model
{
    public class MessagePayload
    {
        [JsonProperty("text_message")] 
        public string TextMessage { get; set; }
        
        [JsonProperty("help_request")] 
        public HelpRequestData HelpRequest { get; set; }
        
        [JsonProperty("reward")] 
        public RewardData Reward { get; set; }
        
        [JsonProperty("join_request")] 
        public JoinRequestData JoinRequest { get; set; }
        
        [JsonProperty("join_accept")] 
        public JoinAcceptData JoinAccept { get; set; }
        
        [JsonProperty("join_reject")] 
        public JoinRejectData JoinReject { get; set; }
        
        [JsonProperty("kick")] 
        public KickData Kick { get; set; }
    }
}