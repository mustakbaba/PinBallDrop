using ElephantSocial.Model;
using Newtonsoft.Json;

namespace ElephantSocial.Leaderboard.Model
{
    public class BaseLeaderboardRequest : SocialBaseData
    {
        [JsonProperty("leaderboard_id")] public int leaderboardId;

        public BaseLeaderboardRequest(int leaderboardId)
        {
            this.leaderboardId = leaderboardId;
        }
    }
}