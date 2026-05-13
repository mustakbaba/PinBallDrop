using Newtonsoft.Json;

namespace ElephantSocial.Chat.Model
{
    public class DeletedMessagePayload
    {
        [JsonProperty("id")] 
        public string Id { get; set; }
    }
}