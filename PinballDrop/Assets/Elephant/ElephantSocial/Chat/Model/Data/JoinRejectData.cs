using Newtonsoft.Json;

namespace ElephantSocial.Chat.Model
{
    public class JoinRejectData
    {
        [JsonProperty("social_id")] 
        public string SocialId { get; set; }
        
        [JsonProperty("player_name")] 
        public string PlayerName { get; set; }
        
        [JsonProperty("profile_picture")] 
        public string ProfilePicture { get; set; }
        
        [JsonProperty("target_social_id")] 
        public string TargetSocialId { get; set; }
        
        [JsonProperty("target_player_name")] 
        public string TargetPlayerName { get; set; }
        
        [JsonProperty("target_profile_picture")] 
        public string TargetProfilePicture { get; set; }    }
}