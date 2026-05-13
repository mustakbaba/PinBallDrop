using System;
using Newtonsoft.Json;

namespace ElephantSocial.Tournament.Model
{
    [Serializable]
    public class TournamentAddScoreResponse
    {
        [JsonProperty("added_score")] public int addedScore;
    }
}