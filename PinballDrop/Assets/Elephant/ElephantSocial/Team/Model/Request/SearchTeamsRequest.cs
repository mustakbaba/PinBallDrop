using ElephantSocial.Model;
using Newtonsoft.Json;

namespace ElephantSocial.Team.Model.Request
{
    public class SearchTeamsRequest : SocialBaseData
    {
        [JsonProperty("search")]
        public string SearchTerm;
    }
}