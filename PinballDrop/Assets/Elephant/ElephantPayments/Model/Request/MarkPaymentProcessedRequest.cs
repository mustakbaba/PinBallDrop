using System;
using Newtonsoft.Json;

namespace ElephantSDK
{
    [Serializable]
    public class MarkPaymentProcessedRequest : BaseData
    {
        [JsonProperty("transaction_id")]
        public string transactionId;
        
        public static MarkPaymentProcessedRequest Create(string transactionId)
        {
            var request = new MarkPaymentProcessedRequest();
            request.FillBaseData(ElephantCore.Instance.GetCurrentSession().GetSessionID());
            request.transactionId = transactionId;
            return request;
        }
    }
}