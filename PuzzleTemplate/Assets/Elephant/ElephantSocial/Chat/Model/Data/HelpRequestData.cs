using System.Collections.Generic;
using Newtonsoft.Json;

namespace ElephantSocial.Chat.Model
{
    public class HelpRequestData
    {
        [JsonProperty("requested")] 
        public int Requested { get; set; }
        
        [JsonProperty("received")] 
        public int Received { get; set; }
        
        [JsonProperty("senders")] 
        public List<string> Senders { get; set; }
    }
}