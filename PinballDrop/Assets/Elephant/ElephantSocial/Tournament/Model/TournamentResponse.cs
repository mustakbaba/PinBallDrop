using System;
using Newtonsoft.Json;

namespace ElephantSocial.Tournament.Model
{
    [Serializable]
    public class TournamentResponse
    {
        public int score;
        public int status;
        [JsonProperty("social_id")] public string socialId;
        [JsonProperty("player_name")]public string playerName;
        [JsonProperty("profile_picture")] public string profilePicture;
        [JsonProperty("team_id")] public string teamId;
        [JsonProperty("team_name")] public string teamName;
    }
}