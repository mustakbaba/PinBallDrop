using System;
using ElephantSDK;
using Newtonsoft.Json;
using UnityEngine;

namespace ElephantSocial
{
    public class SocialDataStore : GenericResponseOps
    {
        private const string MainKey = "SocialDataStore";
        
        private string GenerateKey(string val)
        {
            return MainKey + val;
        }

        protected void Save<T>(string key, T data)
        {
            var jsonData = JsonConvert.SerializeObject(data);
            PlayerPrefs.SetString(GenerateKey(key), jsonData);
        }

        protected T Load<T>(string key)
        {
            var storedData = PlayerPrefs.GetString(GenerateKey(key), "");
            if (string.IsNullOrEmpty(storedData))
            {
                return default;
            }

            try
            {
                var data = JsonConvert.DeserializeObject<T>(storedData);
                return data;
            }
            catch (JsonException jsonException)
            {
                ElephantLog.LogError("TournamentDataStoreInternalJson", jsonException.Message);
                return default;
            }
            catch (Exception e)
            {
                ElephantLog.LogError("TournamentDataStoreInternal", e.Message);
                return default;
            }
        }
    }
}