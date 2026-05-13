using ElephantSocial.Model;
using Newtonsoft.Json;

namespace ElephantSocial.Team.Model.Request
{
    public class RejectJoinRequest : SocialBaseData
    {
        [JsonProperty("target_social_id")] public string TargetSocialId;
    }
}