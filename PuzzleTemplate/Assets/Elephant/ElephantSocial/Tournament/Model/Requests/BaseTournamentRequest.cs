using ElephantSDK;
using ElephantSocial.Model;
using Newtonsoft.Json;

namespace ElephantSocial.Tournament.Model
{
    public class BaseTournamentRequest : SocialBaseData
    {
        [JsonProperty("tournament_id")] public int tournamentId;
        [JsonProperty("schedule_id")] public int scheduleID;
        
        public BaseTournamentRequest(int tournamentId, int scheduleID)
        {
            this.tournamentId = tournamentId;
            this.scheduleID = scheduleID;
        }
    }
}