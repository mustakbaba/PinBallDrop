using System;
using ElephantSocial.Team.Model;
using Newtonsoft.Json;

namespace ElephantSocial.Inbox.Model
{
    [Serializable]
    public class InboxItem
    {
        [JsonProperty("id")] 
        public int Id;
        
        [JsonProperty("owner_social_id")] 
        public string OwnerSocialId;
        
        [JsonProperty("sender_social_id")] 
        public string SenderSocialId;

        [JsonProperty("sender_player_info")] 
        public PlayerInfo SenderPlayerInfo;
        
        [JsonProperty("metadata")] 
        public string Metadata;
        
        [JsonProperty("created_at")] 
        public string CreatedAt;
    }
}