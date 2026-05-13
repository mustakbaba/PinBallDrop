using System;
using ElephantSDK;
using Newtonsoft.Json;

namespace ElephantSocial.Model
{
    [Serializable]
    public class SocialBaseData : BaseData
    {
        [JsonProperty("request_id")] public long requestId;

        public SocialBaseData()
        {
            FillBaseData(ElephantCore.Instance.GetCurrentSession().GetSessionID());
            requestId = Utils.Timestamp();
        }
    }
}