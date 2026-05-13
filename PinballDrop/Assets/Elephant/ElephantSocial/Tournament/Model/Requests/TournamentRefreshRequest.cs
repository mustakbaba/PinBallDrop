using System;
using ElephantSDK;
using ElephantSocial.Model;
using Newtonsoft.Json;
using UnityEngine.Serialization;

namespace ElephantSocial.Tournament.Model
{
    [Serializable]
    public class TournamentRefreshRequest : SocialBaseData
    {
        [JsonProperty("tournament_id")] public int tournamentId; 
        [JsonProperty("schedule_id")] public int scheduleId;
        
        public TournamentRefreshRequest(int tournamentId, int scheduleId)
        {
            this.tournamentId = tournamentId;
            this.scheduleId = scheduleId;
        }
    }
}