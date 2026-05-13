using System;
using ElephantSocial.Team.Model.Response;
using Newtonsoft.Json;

namespace ElephantSocial.Model
{
    [Serializable]
    public class Player
    {
        [JsonProperty("player_name")]public string playerName;
        [JsonProperty] public string country;
        [JsonProperty("social_id")] public string socialId;
        [JsonProperty("profile_picture")] public string profilePicture;
        public string content;
        public int status;
        public int badge;
        [JsonProperty("server_id")] public int serverId;
        [JsonProperty("team")] public TeamResponse team;
        
        public void FillBaseData(Player player)
        {
            playerName = player.playerName;
            profilePicture = player.profilePicture ?? "";
            status = player.status;
            socialId = player.socialId;
            country = player.country;
            content = player.content;
            serverId = player.serverId;
            badge = player.badge;
            team = player.team;
        }

        public Player Clone()
        {
            var player = new Player();
            player.FillBaseData(this);
            return player;
        }
    }
}