using System;
using System.Collections.Generic;
using ElephantSocial.Model;
using Newtonsoft.Json;

namespace ElephantSocial.Tournament.Model
{
    [Serializable]
    public class TournamentJoinResponse
    {
        [JsonProperty("board_players")] public List<BoardPlayer> boardPlayers;
    }
}