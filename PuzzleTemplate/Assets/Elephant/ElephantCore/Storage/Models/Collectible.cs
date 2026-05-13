using System;
using System.Collections.Generic;

namespace ElephantSDK
{
    [Serializable]
    public class Collectible
    {
        public int id;
        public string message;
        public List<KV> payload;
        public string button_name;
    }

    [Serializable]
    public class KV
    {
        public string key;
        public object value;
        public string operation;
    }
}