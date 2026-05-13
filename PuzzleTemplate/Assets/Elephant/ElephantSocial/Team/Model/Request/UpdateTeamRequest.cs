using ElephantSocial.Model;
using Newtonsoft.Json;

namespace ElephantSocial.Team.Model.Request
{
    public class UpdateTeamRequest : SocialBaseData
    {
        [JsonProperty("badge")] public int Badge;
        [JsonProperty("capacity")] public int Capacity;
        [JsonProperty("description")] public string Description;
        [JsonProperty("name")] public string Name;
        [JsonProperty("required_level")] public int RequiredLevel;
        [JsonProperty("team_type")] public int TeamType;
    }
}