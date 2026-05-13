using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using ElephantSocial.Model;

namespace ElephantSocial.Tournament.Model
{
    [Serializable]
    public class TournamentAddMatchRequest : BaseTournamentRequest
    {
        [JsonProperty("score_updates")] public List<ScoreUpdate> ScoreUpdates { get; }

        public TournamentAddMatchRequest(int tournamentId, int scheduleID, List<ScoreUpdate> updates)
            : base(tournamentId, scheduleID)
        {
            ScoreUpdates = updates ?? new List<ScoreUpdate>();
        }
    }

    [Serializable]
    public class ScoreUpdate
    {
        [JsonProperty("social_id")] public string SocialId { get; set; }
        [JsonProperty("score")] public int Score { get; set; }

        public ScoreUpdate() {}

        public ScoreUpdate(string socialId, int score)
        {
            SocialId = socialId;
            Score = score;
        }
    }
}
