using System;
using Newtonsoft.Json;

namespace ElephantSDK
{
    [Serializable]
    public class ElephantPayment
    {
        [JsonProperty("purchased_product")]
        public ElephantProduct purchasedProduct;
        [JsonProperty("transaction_id")]
        public string transactionId;
    }
}