using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace RollicGames.Advertisements
{
    [Serializable]
    public class NetworkVersionInfo
    {
        [JsonProperty("android")]
        public string Android;

        [JsonProperty("ios")]
        public string Ios;
    }

    [Serializable]
    public class NetworkVersionsData
    {
        [JsonProperty("networks")]
        public Dictionary<string, NetworkVersionInfo> Networks = new Dictionary<string, NetworkVersionInfo>();
    }
}