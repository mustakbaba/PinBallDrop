using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;

namespace ElephantSDK
{
    public class MetaDataUtils
    {
        private static MetaDataUtils _instance;
        private Queue<Offer> offerQueue = new Queue<Offer>();
        private Queue<Offer> sessionOfferQueue = new Queue<Offer>();
        private const int MAX_OFFER_COUNT = 10;
        private const int MAX_BUFFER_LENGTH = 10;
        private Dictionary<string, FirstOfferTimestamp> firstOfferTimestamps = new Dictionary<string, FirstOfferTimestamp>();
        private List<string> _purchasedOffers;
        private Dictionary<string, OfferCounts> offerCounts = new Dictionary<string, OfferCounts>();

        public static MetaDataUtils GetInstance()
        {
            return _instance ?? (_instance = new MetaDataUtils());
        }

        private MetaDataUtils()
        {
            FlushSession();
            InitOffers();
            IncrementByOne(MetaDataKeys.KeyTotalSessionCount);
            IncrementDailySessionCount();
        }

        public void AddOffer(Offer newOffer)
        {
            if (offerQueue.Count >= MAX_OFFER_COUNT)
            {
                offerQueue.Dequeue();
            }
            offerQueue.Enqueue(newOffer);

            if (sessionOfferQueue.Count >= MAX_OFFER_COUNT)
            {
                sessionOfferQueue.Dequeue();
            }
            sessionOfferQueue.Enqueue(newOffer);

            string json = JsonConvert.SerializeObject(new SerializableQueue<Offer>(offerQueue));
            SetToPrefs(MetaDataKeys.KeyOfferList, json);
            json = JsonConvert.SerializeObject(new SerializableQueue<Offer>(sessionOfferQueue));
            SetToPrefs(MetaDataKeys.KeySessionOfferList, json);
            
            IncreaseOfferCount(newOffer.offer_name);
        }

        private void IncreaseOfferCount(string offerName)
        {
            if (!offerCounts.ContainsKey(offerName))
            {
                offerCounts[offerName] = new OfferCounts
                {
                    key = offerName,
                    count = 1
                };
            }
            else
            {
                offerCounts[offerName].count += 1;
            }
            string json = JsonConvert.SerializeObject(new SerializableList<OfferCounts>(offerCounts.Values.ToList()));
            SetToPrefs(MetaDataKeys.KeyOfferCounts, json);
        }

        public void AddFirstOffer(Offer offer, string triggerPoint)
        {
            string key = $"{offer.offer_name}_{triggerPoint}";
            
            if (!firstOfferTimestamps.ContainsKey(key))
            {
                firstOfferTimestamps[key] = new FirstOfferTimestamp
                {
                    key = key,
                    timestamp = offer.timestamp
                };
            }
            string json = JsonConvert.SerializeObject(new SerializableList<FirstOfferTimestamp>(firstOfferTimestamps.Values.ToList()));
            SetToPrefs(MetaDataKeys.KeyFirstOfferTimestamps, json);
        }

        public void AddPurchasedOffer(string offerName)
        {
            if (_purchasedOffers == null) return;
            
            _purchasedOffers.Add(offerName);
            var purchasedOfferJsonString = JsonConvert.SerializeObject(new SerializableList<string>(_purchasedOffers));
            SetToPrefs(MetaDataKeys.KeyPurchasedOffers, purchasedOfferJsonString);
        }

        private void InitOffers()
        {
            try
            {
                // purchased offers init
                var purchasedOffersString = GetFromPrefs<string>(MetaDataKeys.KeyPurchasedOffers, null);
                if (!string.IsNullOrEmpty(purchasedOffersString))
                {
                    var purchasedOffersSerializableList = JsonConvert.DeserializeObject<SerializableList<string>>(purchasedOffersString);
                    _purchasedOffers = purchasedOffersSerializableList.List;
                }
                else
                {
                    _purchasedOffers = new List<string>();
                }
                
                // Offer list
                string json = GetFromPrefs(MetaDataKeys.KeyOfferList, "");
                if (!string.IsNullOrEmpty(json))
                {
                    SerializableQueue<Offer> serializableQueue = JsonConvert.DeserializeObject<SerializableQueue<Offer>>(json);
                    offerQueue = new Queue<Offer>(serializableQueue.queue);
                }
                
                // first offer timestamps
                json = GetFromPrefs(MetaDataKeys.KeyFirstOfferTimestamps, "");
                if (!string.IsNullOrEmpty(json))
                {
                    SerializableList<FirstOfferTimestamp> serializableList = JsonConvert.DeserializeObject<SerializableList<FirstOfferTimestamp>>(json);
                    firstOfferTimestamps = serializableList.ToDictionary(item => item.key);
                }
                
                // offer counts
                json = GetFromPrefs(MetaDataKeys.KeyOfferCounts, "");
                if (!string.IsNullOrEmpty(json))
                {
                    SerializableList<OfferCounts> serializableList = JsonConvert.DeserializeObject<SerializableList<OfferCounts>>(json);
                    offerCounts = serializableList.ToDictionary(item => item.key);
                }
            }
            catch (Exception e)
            {
                ElephantLog.Log("OFFER", "LoadOffers exception: " + e.Message);
            }
        }

        public void IncrementByOne(string key)
        {
            int currentValue = GetFromPrefs<int>(key, 0);
            SetToPrefs(key, currentValue + 1);
        }

        public void IncrementDailySessionCount()
        {
            var today = DateTime.Now.ToString("yyyyMMdd");
            var lastRecordedDate = GetFromPrefs<string>(MetaDataKeys.KeyLastRecordedDate, "");

            if (string.IsNullOrEmpty(lastRecordedDate) || lastRecordedDate != today)
            {
                SetToPrefs(MetaDataKeys.KeyDailySessionCount, 1);
                SetToPrefs(MetaDataKeys.KeyLastRecordedDate, today);
            }
            else
            {
                int currentCount = GetFromPrefs<int>(MetaDataKeys.KeyDailySessionCount, 0);
                SetToPrefs(MetaDataKeys.KeyDailySessionCount, currentCount + 1);
            }
        }

        public void UpdateLastXLevelsFailCount(char lastState)
        {
            var lastXLevelsFailCount = GetFromPrefs<string>(MetaDataKeys.KeyLastXLevelsFailCount, "") + lastState;
            if (lastXLevelsFailCount.Length > MAX_BUFFER_LENGTH)
            {
                lastXLevelsFailCount = lastXLevelsFailCount.Substring(lastXLevelsFailCount.Length - MAX_BUFFER_LENGTH);
            }
            SetToPrefs(MetaDataKeys.KeyLastXLevelsFailCount, lastXLevelsFailCount);
        }

        public void FlushSession()
        {
            ElephantLog.Log("OFFER", "Flush Session");
            
            float sessionStartLtv = PlayerPrefs.GetFloat(MetaDataKeys.KeyTotalAdLtv, 0f);
            SetToPrefs(MetaDataKeys.KeySessionStartLtv, sessionStartLtv);
            
            PlayerPrefs.DeleteKey(MetaDataKeys.KeySessionInterstitialWatchCounter);
            PlayerPrefs.DeleteKey(MetaDataKeys.KeySessionRewardedAdWatchCounter);
            PlayerPrefs.DeleteKey(MetaDataKeys.KeySessionLevelCompleteCount);
            PlayerPrefs.DeleteKey(MetaDataKeys.KeySessionLevelFailCount);
            PlayerPrefs.DeleteKey(MetaDataKeys.KeySessionLevelRecurringFailCount);
            PlayerPrefs.DeleteKey(MetaDataKeys.KeySessionLevelStartCount);
            PlayerPrefs.DeleteKey(MetaDataKeys.KeySessionStartLtv);
            PlayerPrefs.DeleteKey(MetaDataKeys.KeySessionOfferList);
            PlayerPrefs.Save();
        }

        public int GetCurrentLevel()
        {
            return GetFromPrefs<int>(MetaDataKeys.KeyCurrentLevel, 0);
        }

        public int GetLastLevel()
        {
            return GetFromPrefs<int>(MetaDataKeys.KeyLastLevel, 0);
        }

        public string GetLastLevelState()
        {
            return GetFromPrefs<string>(MetaDataKeys.KeyLastLevelState, "");
        }

        public int GetTotalInterstitialCount()
        {
            return GetFromPrefs<int>(MetaDataKeys.KeyInterstitialWatchCounter, 0);
        }

        public int GetTotalRewardedCount()
        {
            return GetFromPrefs<int>(MetaDataKeys.KeyRewardedAdWatchCounter, 0);
        }

        public int GetLevelCompleteCount()
        {
            return GetFromPrefs<int>(MetaDataKeys.KeyLevelCompleteCount, 0);
        }

        public int GetLevelStartCount()
        {
            return GetFromPrefs<int>(MetaDataKeys.KeyLevelStartCount, 0);
        }

        public int GetTotalSessionCount()
        {
            return GetFromPrefs<int>(MetaDataKeys.KeyTotalSessionCount, 0);
        }

        public float GetTotalAdLtv()
        {
            return GetFromPrefs<float>(MetaDataKeys.KeyTotalAdLtv, 0f);
        }

        public string GetLastXLevelsFailCount()
        {
            return GetFromPrefs<string>(MetaDataKeys.KeyLastXLevelsFailCount, "");
        }

        public int GetSessionInterstitialCount()
        {
            return GetFromPrefs<int>(MetaDataKeys.KeySessionInterstitialWatchCounter, 0);
        }

        public int GetSessionRewardedCount()
        {
            return GetFromPrefs<int>(MetaDataKeys.KeySessionRewardedAdWatchCounter, 0);
        }

        public int GetSessionLevelCompleteCount()
        {
            return GetFromPrefs<int>(MetaDataKeys.KeySessionLevelCompleteCount, 0);
        }

        public int GetSessionLevelStartCount()
        {
            return GetFromPrefs<int>(MetaDataKeys.KeySessionLevelStartCount, 0);
        }

        public int GetSessionLevelRecurringFailCount()
        {
            return GetFromPrefs<int>(MetaDataKeys.KeySessionLevelRecurringFailCount, 0);
        }

        public float GetSessionAdLtv()
        {
            float totalAdLtv = GetFromPrefs<float>(MetaDataKeys.KeyTotalAdLtv, 0f);
            float sessionStartLtv = GetFromPrefs<float>(MetaDataKeys.KeySessionStartLtv, 0f);
            return totalAdLtv - sessionStartLtv;
        }

        public int GetSessionLevelFailCount()
        {
            return GetFromPrefs<int>(MetaDataKeys.KeySessionLevelFailCount, 0);
        }

        public string GetLastRecordedDate()
        {
            return GetFromPrefs<string>(MetaDataKeys.KeyLastRecordedDate, "");
        }

        public int GetDailySessionCount()
        {
            return GetFromPrefs<int>(MetaDataKeys.KeyDailySessionCount, 0);
        }

        public string GetLastLevelId()
        {
            return GetFromPrefs<string>(MetaDataKeys.KeyLastLevelId, "");
        }
        
        public string GetCurrentLevelId()
        {
            return GetFromPrefs<string>(MetaDataKeys.KeyCurrentLevelId, "");
        }

        public List<Offer> GetOfferList()
        {
            return offerQueue.ToList();
        }

        public List<Offer> GetSessionOfferList()
        {
            return sessionOfferQueue.ToList();
        }

        public List<FirstOfferTimestamp> GetFirstOfferTimestamps()
        {
            return new List<FirstOfferTimestamp>(firstOfferTimestamps.Values);
        }
        
        public List<string> GetPurchasedOffers()
        {
            return _purchasedOffers;
        }
        
        public List<OfferCounts> GetOfferCounts()
        {
            return new List<OfferCounts>(offerCounts.Values);
        }

        private T GetFromPrefs<T>(string key, T defaultValue)
        {
            if (typeof(T) == typeof(int))
            {
                return (T)(object)PlayerPrefs.GetInt(key, (int)(object)defaultValue);
            }
            else if (typeof(T) == typeof(float))
            {
                return (T)(object)PlayerPrefs.GetFloat(key, (float)(object)defaultValue);
            }
            else if (typeof(T) == typeof(string))
            {
                return (T)(object)PlayerPrefs.GetString(key, (string)(object)defaultValue);
            }
            else
            {
                throw new InvalidOperationException($"Type {typeof(T)} not supported.");
            }
        }

        public void SetToPrefs<T>(string key, T value)
        {
            try
            {
                ElephantLog.Log("OFFER", "Set" + typeof(T).ToString()  + "with key: " + key + " and value: " + value);
                if (typeof(T) == typeof(int))
                {
                    PlayerPrefs.SetInt(key, (int)(object)value);
                }
                else if (typeof(T) == typeof(float))
                {
                    PlayerPrefs.SetFloat(key, (float)(object)value);
                }
                else if (typeof(T) == typeof(string))
                {
                    PlayerPrefs.SetString(key, (string)(object)value);
                }
                else if (typeof(T) == typeof(long))
                {
                    PlayerPrefs.SetString(key, ((long)(object)value).ToString());
                }
                else
                {
                    throw new InvalidOperationException($"Type {typeof(T)} not supported.");
                }
                PlayerPrefs.Save();
            }
            catch (Exception e)
            {
                ElephantLog.Log("OFFER", "Set exception: " + e.Message);
            }
        }
    }

    [System.Serializable]
    public class SerializableQueue<T>
    {
        public List<T> queue;

        public SerializableQueue(Queue<T> q)
        {
            queue = new List<T>(q);
        }
    }
    
    [Serializable]
    public class SerializableList<T>
    {
        [SerializeField]
        private List<T> list;

        public SerializableList(List<T> list)
        {
            this.list = list;
        }

        public List<T> List => list;

        public Dictionary<string, T> ToDictionary(Func<T, string> keySelector)
        {
            return list.ToDictionary(keySelector);
        }
    }
}