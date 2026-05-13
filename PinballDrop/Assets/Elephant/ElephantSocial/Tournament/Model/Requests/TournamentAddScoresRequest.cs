using System;
using System.Collections.Generic;
using ElephantSDK;
using Newtonsoft.Json;

namespace ElephantSocial.Tournament.Model
{
    [Serializable]
    public class TournamentAddScoresRequest : BaseTournamentRequest
    {
        [JsonProperty("player_scores")]
        public List<PlayerScore> PlayerScores { get; }

        public TournamentAddScoresRequest(int tournamentId, int scheduleID, List<PlayerScore> scores) 
            : base(tournamentId, scheduleID)
        {
            PlayerScores = scores;
        }
    }
    
    [Serializable]
    public class PlayerScore
    {
        [JsonProperty("score")]
        public int Score { get; set; }

        [JsonProperty("date")]
        public long Date { get; set; }

        [JsonProperty("online")]
        public bool Online { get; set; }

        public PlayerScore() { }

        public PlayerScore(int score, long date, bool online)
        {
            Score = score;
            Date = date;
            Online = online;
        }
    }
}