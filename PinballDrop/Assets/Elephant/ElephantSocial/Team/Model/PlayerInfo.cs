using System;
using Newtonsoft.Json;

namespace ElephantSocial.Team.Model
{
    [Serializable]
    public class PlayerInfo
    {
        [JsonProperty("player_name")] 
        public string PlayerName;
        [JsonProperty("profile_picture")] 
        public string ProfilePicture;
        [JsonProperty("status")] 
        public int Status;
        [JsonProperty("badge")] 
        public int Badge;
        [JsonProperty("country")] 
        public string Country;
        [JsonProperty("level")] 
        public string Level;
    }
}