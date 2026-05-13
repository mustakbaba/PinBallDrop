using System;
using System.Collections.Generic;

namespace ElephantSDK
{
    [Serializable]
    public class MDSCollectible
    {
        public long id;
        public string elephant_id;
        public string message;
        public List<MDSPayload> payload;
        public string button_name;
        public bool claimed;
        public MDSPurchaseInfo mds;
    }
}