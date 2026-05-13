using System;
using Newtonsoft.Json;
using UnityEngine.Serialization;

namespace ElephantSocial.Tournament.Model
{
    [Serializable]
    public class TournamentInitResponse
    {
        [JsonProperty("tournaments")]public Tournament[] tournaments;
        [JsonProperty("player_tournaments")]public Tournament[] playerTournaments;
    }
}