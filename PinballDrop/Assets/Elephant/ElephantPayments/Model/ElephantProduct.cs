using System;
using Newtonsoft.Json;

namespace ElephantSDK
{
    [Serializable]
    public class ElephantProduct
    {
        [JsonProperty("product_id")]
        public string productId;
        [JsonProperty("price")]
        public double price;
    }
}