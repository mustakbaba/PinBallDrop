using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace ElephantSocial.Tournament
{
    [Serializable]
    public class TournamentsResponse
    {
        [JsonProperty("server_time")]
        public long serverTime;
        [JsonProperty("tournaments")]
        public List<TournamentData> tournaments = new();
    }
}