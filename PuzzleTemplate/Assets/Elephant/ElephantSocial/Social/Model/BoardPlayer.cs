using System;
using Newtonsoft.Json;

namespace ElephantSocial.Model
{
    [Serializable]
    public class BoardPlayer
    {
        [JsonProperty("player_name")] public string playerName;
        [JsonProperty] public string country;
        [JsonProperty("social_id")] public string socialId;
        [JsonProperty("profile_picture")] public string profilePicture;
        public int status;
        public int badge;
        public int score;
        [JsonProperty("server_id")] public int serverId;
        
        public void FillBaseData(Player player)
        {
            playerName = player.playerName;
            profilePicture = player.profilePicture ?? "";
            status = player.status;
            socialId = player.socialId;
            country = player.country;
            badge = player.badge;
            serverId = player.serverId;
        }
        
        public void FillBaseData(BoardPlayer player)
        {
            playerName = player.playerName;
            profilePicture = player.profilePicture ?? "";
            status = player.status;
            socialId = player.socialId;
            country = player.country;
            badge = player.badge;
            serverId = player.serverId;
        }
        
        public BoardPlayer Clone()
        {
            var boardPlayer = new BoardPlayer();
            boardPlayer.FillBaseData(this);
            boardPlayer.score = score;
            return boardPlayer;
        }
    }
}