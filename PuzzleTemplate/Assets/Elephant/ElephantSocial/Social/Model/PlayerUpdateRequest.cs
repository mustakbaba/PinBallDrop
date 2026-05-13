using System;
using ElephantSDK;
using Newtonsoft.Json;

namespace ElephantSocial.Model
{
    [Serializable]
    public class PlayerUpdateRequest : SocialBaseData
    {
        [JsonProperty("player_name")] public string playerName;
        [JsonProperty("profile_picture")] public string profilePicture;
        public string content;
        public int status;
        public int badge;
        
        public PlayerUpdateRequest(Player player)
        {
            playerName = player.playerName;
            profilePicture = player.profilePicture;
            status = player.status;
            content = player.content;
            badge = player.badge;
        }
    }
}