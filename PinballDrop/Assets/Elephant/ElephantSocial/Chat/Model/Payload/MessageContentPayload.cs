using Newtonsoft.Json;

namespace ElephantSocial.Chat.Model
{
    public class MessageContentPayload
    {
        [JsonProperty("id")] 
        public string Id { get; set; }
        
        [JsonProperty("sender")] 
        public string SenderId { get; set; }
        
        [JsonProperty("sender_name")] 
        public string SenderName { get; set; }
        
        [JsonProperty("sender_profile_picture")] 
        public string SenderProfilePicture { get; set; }
        
        [JsonProperty("payload_type")] 
        public string PayloadType { get; set; }
        
        [JsonProperty("payload")] 
        public MessagePayload SpecificPayload { get; set; }
        
        [JsonProperty("created_at")] 
        public long CreatedAt { get; set; }
        
        [JsonProperty("updated_at")] 
        public long UpdatedAt { get; set; }
        
        [JsonProperty("version")] 
        public long Version { get; set; }
        
        [JsonProperty("deleted_at")] 
        public long? DeletedAt { get; set; }
    }
}