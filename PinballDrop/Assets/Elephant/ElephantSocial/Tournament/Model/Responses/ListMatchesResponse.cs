using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace ElephantSocial.Tournament.Model
{
    [Serializable]
    public class TournamentListMatchesResponse
    {
        [JsonProperty("matches")] public List<TournamentMatchItem> Matches { get; set; }
    }

    [Serializable]
    public class TournamentMatchItem
    {
        [JsonProperty("game_id")] public int GameId { get; set; }
        [JsonProperty("server_id")] public int ServerId { get; set; }
        [JsonProperty("tournament_id")] public int TournamentId { get; set; }
        [JsonProperty("schedule_id")] public int ScheduleId { get; set; }
        [JsonProperty("metadata")] public string Metadata { get; set; }
        [JsonProperty("score_updates")] public List<TournamentScoreUpdateWithInfo> ScoreUpdates { get; set; }
        [JsonProperty("created_at")] public string CreatedAt { get; set; }
    }

    [Serializable]
    public class TournamentScoreUpdateWithInfo
    {
        [JsonProperty("score")] public int Score { get; set; }
        [JsonProperty("social_id")] public string SocialId { get; set; }
        [JsonProperty("player_info")] public TournamentPlayerInfo PlayerInfo { get; set; }
    }

    [Serializable]
    public class TournamentPlayerInfo
    {
        [JsonProperty("player_name")] public string PlayerName { get; set; }
        [JsonProperty("profile_picture")] public string ProfilePicture { get; set; }
        [JsonProperty("status")] public int Status { get; set; }
        [JsonProperty("badge")] public int Badge { get; set; }
        [JsonProperty("country")] public string Country { get; set; }
        [JsonProperty("level")] public int Level { get; set; }
    }
}
