using ElephantSocial.Model;
using Newtonsoft.Json;

namespace ElephantSocial.Team.Model.Request
{
    public class GetTeamRequest : SocialBaseData
    {
        [JsonProperty("team_id")] public string TeamId;
    }
}