using System;
using Newtonsoft.Json;

namespace ElephantSDK
{
    [Serializable]
    public class PaymentCheckoutResponse
    {
        [JsonProperty("escrow_code")]
        public string escrowCode;
        [JsonProperty("expiry")]
        public long expiry;
    }
}