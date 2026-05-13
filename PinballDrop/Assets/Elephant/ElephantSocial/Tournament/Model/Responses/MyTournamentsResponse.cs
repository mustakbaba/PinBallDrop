using System;
using System.Collections.Generic;
using ElephantSocial.Tournament.Model;
using Newtonsoft.Json;

namespace ElephantSocial.Tournament
{
    [Serializable]
    public class MyTournamentsResponse
    {
        [JsonProperty("tournaments")]
        public List<TournamentAndScheduleId> myTournamentIds = new();
    }
}