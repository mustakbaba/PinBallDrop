using System;

namespace ElephantSDK
{
    [Serializable]
    public class MDSPayload
    {
        public string key;
        public int value;
        public string operation;
    }
}