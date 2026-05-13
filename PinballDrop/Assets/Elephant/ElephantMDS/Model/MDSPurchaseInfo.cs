using System;

namespace ElephantSDK
{
    [Serializable]
    public class MDSPurchaseInfo
    {
        public string adjust_event_token;
        public string product_id;
        public string transaction_id;
        public double price;
        public string currency;
        public double local_price;
        public string local_currency;
    }
}