using System;
using System.Collections.Generic;
using ElephantSocial.Tournament.Model;
using Newtonsoft.Json;
using UnityEngine.Serialization;

namespace ElephantSocial.Tournament
{
    [Serializable]
    public class MyTournamentResultsResponse
    {
        [JsonProperty("tournaments")] public List<TournamentData> myTournamentResults = new();
    }
}