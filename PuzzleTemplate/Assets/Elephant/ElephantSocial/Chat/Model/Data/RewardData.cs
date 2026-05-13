using System.Collections.Generic;
using Newtonsoft.Json;

namespace ElephantSocial.Chat.Model
{
    public class RewardData
    {
        [JsonProperty("reward_id")]
        public string RewardId { get; set; }
        
        [JsonProperty("max_receiver")] 
        public int MaxReceiver { get; set; }
        
        [JsonProperty("receivers")] 
        public List<string> Receivers { get; set; }
    }
}