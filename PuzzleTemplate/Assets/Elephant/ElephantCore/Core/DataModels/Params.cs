using System.Collections.Generic;
using UnityEngine;

namespace ElephantSDK
{
    public class Params
    {
        public Dictionary<string, string> stringVals = new Dictionary<string, string>();
        public Dictionary<string, int> intVals = new Dictionary<string, int>();
        public Dictionary<string, double> doubleVals = new Dictionary<string, double>();
        public string customData;
        
        private Params()
        {
        }

        public static Params New()
        {
            return new Params();
        }


        public Params Set(string key, string value)
        {
            if (stringVals.Count >= 4)
            {
                Debug.LogError("You cannot set more than 4 string values for event parameters.");
                return this;
            }
            
            stringVals[key] = value;
            return this;
        }
        
        public Params Set(string key, int value)
        {
            if (intVals.Count >= 4)
            {
                Debug.LogError("You cannot set more than 4 string values for event parameters.");
                return this;
            }
            
            intVals[key] = value;
            return this;
        }
        
        
        public Params Set(string key, double value)
        {
            if (doubleVals.Count >= 4)
            {
                Debug.LogError("You cannot set more than 4 string values for event parameters.");
                return this;
            }
            
            doubleVals[key] = value;
            return this;
        }

        public Params CustomString(string data)
        {
            this.customData = data;
            return this;
        }
        
        public override string ToString()
        {
            var result = new System.Text.StringBuilder();
            var hasValues = false;

            foreach (var pair in stringVals)
            {
                if (hasValues) result.Append(", ");
                result.Append($"{pair.Key}: {pair.Value}");
                hasValues = true;
            }

            foreach (var pair in intVals)
            {
                if (hasValues) result.Append(", ");
                result.Append($"{pair.Key}: {pair.Value}");
                hasValues = true;
            }

            foreach (var pair in doubleVals)
            {
                if (hasValues) result.Append(", ");
                result.Append($"{pair.Key}: {pair.Value}");
                hasValues = true;
            }

            if (!string.IsNullOrEmpty(customData))
            {
                if (hasValues) result.Append(", ");
                result.Append($"customData: {customData}");
            }

            return result.Length > 0 ? result.ToString() : "no parameters";
        }
        
    }
}