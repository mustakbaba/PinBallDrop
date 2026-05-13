using System;
using Newtonsoft.Json;

namespace ElephantSocial.Team.Model.Response
{
    [Serializable]
    public class TeamResponse
    {
        [JsonProperty("badge")] 
        public int Badge;
        
        [JsonProperty("capacity")] 
        public int Capacity;
        
        [JsonProperty("description")] 
        public string Description;
        
        [JsonProperty("name")] 
        public string Name;
        
        [JsonProperty("required_level")] 
        public int RequiredLevel;
        
        [JsonProperty("server_id")] 
        public int ServerId;
        
        [JsonProperty("size")] 
        public int Size;
        
        [JsonProperty("stars")] 
        public int Stars;
        
        [JsonProperty("team_id")] 
        public string TeamId;
        
        [JsonProperty("team_members")] 
        public TeamMemberResponse[] TeamMembers;
        
        [JsonProperty("team_type")] 
        public int TeamType;
        
        [JsonProperty("weekly_helps")] 
        public int WeeklyHelps;
        
        [JsonProperty("online_count")] 
        public int OnlineCount;

        [JsonProperty("country")] 
        public string Country;

        [JsonProperty("team_stat")] 
        public TeamStat TeamStat;
    }
}