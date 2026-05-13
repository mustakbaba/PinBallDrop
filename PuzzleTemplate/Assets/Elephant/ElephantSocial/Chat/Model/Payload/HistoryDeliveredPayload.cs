using System.Collections.Generic;
using Newtonsoft.Json;

namespace ElephantSocial.Chat.Model
{
    public class HistoryDeliveredPayload
    {
        [JsonProperty("messages")]
        public List<MessageContentPayload> Messages { get; set; } = new List<MessageContentPayload>();
        
        [JsonProperty("has_more")] 
        public bool HasMore { get; set; }
    }
}