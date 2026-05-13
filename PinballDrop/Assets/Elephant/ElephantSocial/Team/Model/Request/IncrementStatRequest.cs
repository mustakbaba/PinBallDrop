using ElephantSocial.Model;
using Newtonsoft.Json;

namespace ElephantSocial.Team.Model.Request
{
    public class IncrementStatRequest : SocialBaseData
    {
        [JsonProperty("incr_val")] 
        public int IncrementValue { get; set; }
        [JsonProperty("stat_meta_id")] 
        public string StatId { get; set; }
    }
}