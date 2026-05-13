using System;
using Newtonsoft.Json;

namespace ElephantSocial.Tournament.Model
{
    [Serializable]
    public class TournamentBulkAddScoresResponse
    {
        [JsonProperty("added_score")] public int addedScore;
    }
}