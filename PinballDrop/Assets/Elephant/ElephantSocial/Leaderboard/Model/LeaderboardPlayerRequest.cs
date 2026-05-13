using System;
using ElephantSDK;
using ElephantSocial.Model;
using Newtonsoft.Json;

namespace ElephantSocial.Leaderboard.Model
{
    [Serializable]
    public class LeaderboardPlayerRequest : BaseLeaderboardRequest
    {
        public int score;
        public string operation;

        public LeaderboardPlayerRequest(int leaderboardId, BoardPlayer player, string operation) 
            : base(leaderboardId)
        {
            score = player.score;
            this.operation = operation;
        }
    }
}