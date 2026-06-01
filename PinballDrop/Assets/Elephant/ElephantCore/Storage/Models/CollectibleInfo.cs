using System;
using System.Collections.Generic;

namespace ElephantSDK
{
    [Serializable]
    public class CollectibleInfo
    {
        public int Id;
        public string Message;
        public string ButtonName;
        public Dictionary<string, object> Payload;
    }
}
