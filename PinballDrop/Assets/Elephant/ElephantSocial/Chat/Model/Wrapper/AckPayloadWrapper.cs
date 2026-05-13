using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ElephantSocial.Chat.Model
{
    public class AckPayloadWrapper
    {
        [JsonProperty("payload_type")] 
        public string PayloadType { get; set; }
        
        [JsonProperty("payload")] 
        public JObject Payload { get; set; }
        
        [JsonProperty("source_event_id")] 
        public string SourceEventId { get; set; }
    }
}