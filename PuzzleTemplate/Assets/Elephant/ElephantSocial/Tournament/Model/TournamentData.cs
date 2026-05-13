using System;
using Newtonsoft.Json;

namespace ElephantSocial.Tournament
{
    [Serializable]
    public class TournamentData
    {
        [JsonProperty("id")] public int tournamentID;
        [JsonProperty("schedule_id")] public int scheduleID;
        [JsonProperty("start_date")] public long startDateUnix;
        [JsonProperty("end_date")] public long endDateUnix;
        [JsonProperty("state")] public TournamentState tournamentState;
    }
}