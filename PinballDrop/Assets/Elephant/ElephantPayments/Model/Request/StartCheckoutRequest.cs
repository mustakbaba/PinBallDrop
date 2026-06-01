using System;
using Newtonsoft.Json;

namespace ElephantSDK
{
    [Serializable]
    public class StartCheckoutRequest : BaseData
    {
        [JsonProperty("product_id")]
        public string productId;
        
        [JsonProperty("is_test")]
        public bool isTest;
        
        public static StartCheckoutRequest Create(string productId, bool isTest)
        {
            var request = new StartCheckoutRequest();
            request.FillBaseData(ElephantCore.Instance.GetCurrentSession().GetSessionID());
            request.productId = productId;
            request.isTest = isTest;
            return request;
        }
    }
}