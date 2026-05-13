using System;
using Newtonsoft.Json;

namespace ElephantSocial.Tournament.Model
{
    [Serializable]
    public class TournamentAddScoreRequest : BaseTournamentRequest
    {
        [JsonProperty("score")] 
        public int score;
        [JsonProperty("online")] 
        public bool Online;

        public TournamentAddScoreRequest(int tournamentId, int scheduleID, int scoreToAdd) 
            : base(tournamentId, scheduleID)
        {
            score = scoreToAdd;
            Online = true;
        }
    }
}