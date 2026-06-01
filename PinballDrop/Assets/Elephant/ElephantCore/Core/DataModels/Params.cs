using System.Collections;
using System.Collections.Specialized;
using UnityEngine;

namespace ElephantSDK
{
    public class Params
    {
        public OrderedDictionary stringVals = new OrderedDictionary();
        public OrderedDictionary intVals = new OrderedDictionary();
        public OrderedDictionary doubleVals = new OrderedDictionary();
        
        public string customData;

        private const int MaxParameterCount = 8;
        
        private Params()
        {
        }

        public static Params New()
        {
            return new Params();
        }

        public Params Set(string key, string value)
        {
            if (stringVals.Count >= MaxParameterCount && !stringVals.Contains(key))
            {
                Debug.LogError($"You cannot set more than {MaxParameterCount} string values for event parameters.");
                return this;
            }
            
            stringVals[key] = value;
            return this;
        }
        
        public Params Set(string key, int value)
        {
            if (intVals.Count >= MaxParameterCount && !intVals.Contains(key))
            {
                Debug.LogError($"You cannot set more than {MaxParameterCount} int values for event parameters.");
                return this;
            }
            
            intVals[key] = value;
            return this;
        }
        
        public Params Set(string key, double value)
        {
            if (doubleVals.Count >= MaxParameterCount && !doubleVals.Contains(key))
            {
                Debug.LogError($"You cannot set more than {MaxParameterCount} double values for event parameters.");
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

            foreach (DictionaryEntry entry in stringVals)
            {
                if (hasValues) result.Append(", ");
                result.Append($"{entry.Key}: {entry.Value}");
                hasValues = true;
            }

            foreach (DictionaryEntry entry in intVals)
            {
                if (hasValues) result.Append(", ");
                result.Append($"{entry.Key}: {entry.Value}");
                hasValues = true;
            }

            foreach (DictionaryEntry entry in doubleVals)
            {
                if (hasValues) result.Append(", ");
                result.Append($"{entry.Key}: {entry.Value}");
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