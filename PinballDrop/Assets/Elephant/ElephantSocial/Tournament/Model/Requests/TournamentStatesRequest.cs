using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace ElephantSocial.Tournament.Model
{
    [Serializable]
    public class TournamentStatesRequest
    {
        [JsonProperty("tournament_schedule_ids")]
        public List<TournamentAndScheduleId> tournamentScheduleIds;
    }
}