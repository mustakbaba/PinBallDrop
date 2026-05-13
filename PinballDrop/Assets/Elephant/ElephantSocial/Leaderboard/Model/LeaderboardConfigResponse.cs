using System;
using Newtonsoft.Json;

namespace ElephantSocial.Leaderboard.Model
{
    [Serializable]
    public class LeaderboardConfigResponse
    {
        [JsonProperty("global_leaderboard_id")] public int globalLeaderboardId;
    }
}