using System;
using Newtonsoft.Json;
using UnityEngine;

namespace ElephantSDK
{
    [Serializable]
    public partial class Ilrd
    {
        public double? revenue;

        [JsonProperty("eCPM")]
        public double? ecpm;

        public string adUnitName;
        public int? consecutiveFailCycles;
        public int? maxConsecutiveFailCycles;

        public static string ConvertToJson(object anyObject)
        {
            try
            {
                return JsonConvert.SerializeObject(anyObject);
            }
            catch (Exception e)
            {
                Debug.Log(e);
            }

            return "";
        }
    }
}