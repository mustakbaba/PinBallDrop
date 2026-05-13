using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace ElephantSocial.Tournament.Model
{
    [Serializable]
    public class TournamentStatesResponse
    {
        [JsonProperty("tournament_states")] public List<StateInfo> tournamentStates;
    }

    [Serializable]
    public class StateInfo
    {
        [JsonProperty("tournament_id")] public int tournamentId;
        [JsonProperty("schedule_id")] public int scheduleId;
        [JsonProperty("start_date")] public long startDateUnix;
        [JsonProperty("end_date")] public long endDateUnix;
        [JsonProperty("state")] public TournamentState tournamentState;
    }
}