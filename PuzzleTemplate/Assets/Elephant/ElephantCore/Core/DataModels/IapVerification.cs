using System;

namespace ElephantSDK
{
    [Serializable]
    public class IapVerification
    {
        public bool verified;
        public float usd_price;
        public string transaction_id;
        public string original_transaction_id;
        public long service_ts;
    }
}