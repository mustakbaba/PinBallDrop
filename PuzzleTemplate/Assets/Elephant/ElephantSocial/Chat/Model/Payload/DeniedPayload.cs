using Newtonsoft.Json;

namespace ElephantSocial.Chat.Model
{
    public class DeniedPayload
    {
        [JsonProperty("reason")] 
        public string Reason { get; set; }
        
        [JsonProperty("source_event_id")] 
        public string SourceEventId { get; set; }
    }
}