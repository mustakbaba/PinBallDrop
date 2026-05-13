using ElephantSocial.Model;
using Newtonsoft.Json;

namespace ElephantSocial.Team.Model.Request
{
    public class SuggestTeamsRequest : SocialBaseData
    {
        [JsonProperty("level")]
        public long Level;
    }
}