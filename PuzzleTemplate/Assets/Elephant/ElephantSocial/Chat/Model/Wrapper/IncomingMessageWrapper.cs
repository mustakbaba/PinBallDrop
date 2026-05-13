using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ElephantSocial.Chat.Model
{
    public class IncomingMessageWrapper
    {
        [JsonProperty("id")] 
        public string Id { get; set; }
        
        [JsonProperty("type")] 
        public string Type { get; set; }
        
        [JsonProperty("channel")] 
        public string Channel { get; set; }
        
        [JsonProperty("payload")] 
        public JObject Payload { get; set; }
    }
}