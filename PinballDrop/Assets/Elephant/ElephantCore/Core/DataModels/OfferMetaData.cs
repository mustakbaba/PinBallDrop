using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace ElephantSDK
{
    [Serializable]
    public class OfferMetaData
    {
        public int currentLevel;
        public string currentLevelId;
        public LastPlayedLevel lastPlayedLevel;
        public int totalInterstitialCount;
        public int totalRewardedCount;
        public int totalIAPCount;
        public long totalPlaytime;
        public int totalLevelCompleteCount;
        public int totalLevelStartCount;
        public int totalSessionCount;
        public float totalIAPLTV;
        public float totalAdLTV;
        public int totalBossLevelStartedCount;
        public int totalBossLevelCompletedCount;
        public int totalChallengeLevelStartedCount;
        public int totalChallengeLevelCompletedCount;
        public CurrencyAmount currencyAmount;
        public float totalCurrencyTransactionAmount;
        public string lastXLevelsFailCount;
        public List<Offer> offers;
        public List<InventoryItem> inventoryItems;
        public int sessionInterstitialCount;
        public int sessionRewardedCount;
        public int sessionIAPCount;
        public int sessionLevelCompleteCount;
        public int dailySessionCount;
        public long sessionPlaytime;
        public float sessionIAPLTV;
        public float sessionAdLTV;
        public int sessionBossLevelCompletedCount;
        public int sessionBossLevelStartedCount;
        public int sessionChallengeLevelCompletedCount;
        public int sessionChallengeLevelStartedCount;
        public float sessionCurrencyTransactionAmount;
        public List<Offer> sessionListOfOffers;
        public int sessionFailCount;
        public int sessionRecurringFailCount;
        public int sessionLevelStartCount;
        public string triggerPoint;
        public string subscriptionType;
        public List<FirstOfferTimestamp> firstOfferTimestamps;
        public List<string> purchasedOffers;
        public List<string> purchasedProducts;
        public List<OfferCounts> offerCounts;

        private OfferMetaData(
            int totalIAPCount = 0,
            float totalIAPLTV = 0.0f,
            CurrencyAmount currencyAmount = null, // Assuming this has a default constructor
            float totalCurrencyTransactionAmount = 0.0f,
            List<InventoryItem> inventoryItems = null,
            int sessionIAPCount = 0,
            float sessionIAPLTV = 0.0f,
            float sessionCurrencyTransactionAmount = 0.0f,
            string triggerPoint = "",
            string subscriptionType = "",
            List<string> purchasedProducts = null,
            int totalBossLevelStartedCount = 0,
            int totalBossLevelCompletedCount = 0,
            int totalChallengeLevelStartedCount = 0,
            int totalChallengeLevelCompletedCount = 0,
            int sessionBossLevelStartedCount = 0,
            int sessionBossLevelCompletedCount = 0,
            int sessionChallengeLevelStartedCount= 0,
            int sessionChallengeLevelCompletedCount= 0
            )
        {
            var metaDataUtils = MetaDataUtils.GetInstance();
            currentLevel = metaDataUtils.GetCurrentLevel();
            currentLevelId = metaDataUtils.GetLastLevelId();
            lastPlayedLevel = new LastPlayedLevel
            {
                levelNumber = metaDataUtils.GetLastLevel(),
                levelState = metaDataUtils.GetLastLevelState(),
                levelId = metaDataUtils.GetLastLevelId()
            };
            totalInterstitialCount = metaDataUtils.GetTotalInterstitialCount();
            totalRewardedCount = metaDataUtils.GetTotalRewardedCount();
            this.totalIAPCount = totalIAPCount;
            totalLevelCompleteCount = metaDataUtils.GetLevelCompleteCount();
            totalLevelStartCount = metaDataUtils.GetLevelStartCount();
            totalSessionCount = metaDataUtils.GetTotalSessionCount();
            this.totalIAPLTV = totalIAPLTV;
            totalAdLTV = metaDataUtils.GetTotalAdLtv();
            this.currencyAmount = currencyAmount ?? new CurrencyAmount();
            this.totalCurrencyTransactionAmount = totalCurrencyTransactionAmount;
            lastXLevelsFailCount = metaDataUtils.GetLastXLevelsFailCount();
            offers = metaDataUtils.GetOfferList();
            this.inventoryItems = inventoryItems ?? new List<InventoryItem>();
            sessionInterstitialCount = metaDataUtils.GetSessionInterstitialCount();
            sessionRewardedCount = metaDataUtils.GetSessionRewardedCount();
            this.sessionIAPCount = sessionIAPCount;
            sessionLevelCompleteCount = metaDataUtils.GetSessionLevelCompleteCount();
            dailySessionCount = metaDataUtils.GetDailySessionCount();
            this.sessionIAPLTV = sessionIAPLTV;
            sessionAdLTV = metaDataUtils.GetSessionAdLtv();
            this.sessionCurrencyTransactionAmount = sessionCurrencyTransactionAmount;
            sessionListOfOffers = metaDataUtils.GetSessionOfferList();
            sessionFailCount = metaDataUtils.GetSessionLevelFailCount();
            sessionRecurringFailCount = metaDataUtils.GetSessionLevelRecurringFailCount();
            sessionLevelStartCount = metaDataUtils.GetSessionLevelStartCount();
            this.triggerPoint = triggerPoint;
            this.subscriptionType = subscriptionType;
            this.totalBossLevelStartedCount = totalBossLevelStartedCount;
            this.totalBossLevelCompletedCount = totalBossLevelCompletedCount;
            this.totalChallengeLevelStartedCount = totalChallengeLevelStartedCount;
            this.totalChallengeLevelCompletedCount = totalChallengeLevelCompletedCount;
            this.sessionBossLevelStartedCount = sessionBossLevelStartedCount;
            this.sessionBossLevelCompletedCount = sessionBossLevelCompletedCount;
            this.sessionChallengeLevelStartedCount = sessionChallengeLevelStartedCount;
            this.sessionChallengeLevelCompletedCount = sessionChallengeLevelCompletedCount;
            firstOfferTimestamps = metaDataUtils.GetFirstOfferTimestamps();
            purchasedOffers = metaDataUtils.GetPurchasedOffers();
            this.purchasedProducts = purchasedProducts;
            offerCounts = metaDataUtils.GetOfferCounts();
        }


        public class Builder
        {
            private int _totalIAPCount;
            private float _totalIAPLTV;
            private CurrencyAmount _currencyAmount;
            private float _totalCurrencyTransactionAmount;
            private List<InventoryItem> _inventoryItems;
            private int _sessionIAPCount;
            private float _sessionIAPLTV;
            private string _subscriptionType;
            private List<string> _purchasedProducts;
            private float _sessionCurrencyTransactionAmount;
            private int _totalBossLevelStartedCount;
            private int _totalBossLevelCompletedCount;
            private int _totalChallengeLevelStartedCount;
            private int _totalChallengeLevelCompletedCount;
            private int _sessionBossLevelStartedCount;
            private int _sessionChallengeLevelStartedCount;
            private int _sessionBossLevelCompletedCount;
            private int _sessionChallengeLevelCompletedCount;
            
            private string _triggerPoint;
            
            public Builder(string triggerPoint)
            {
                _triggerPoint = triggerPoint;
            }

            public Builder TotalIAPCount(int value)
            {
                _totalIAPCount = value;
                return this;
            }

            public Builder TotalIAPLTV(float value)
            {
                _totalIAPLTV = value;
                return this;
            }

            public Builder CurrencyAmount(CurrencyAmount value)
            {
                _currencyAmount = value;
                return this;
            }

            public Builder TotalCurrencyTransactionAmount(float value)
            {
                _totalCurrencyTransactionAmount = value;
                return this;
            }
            
            public Builder AddInventoryItems(List<InventoryItem> items)
            {
                _inventoryItems = items;
                return this;
            }

            public Builder SessionIAPCount(int value)
            {
                _sessionIAPCount = value;
                return this;
            }

            public Builder SessionIAPLTV(float value)
            {
                _sessionIAPLTV = value;
                return this;
            }
            
            public Builder SubscriptionType(string value)
            {
                _subscriptionType = value;
                return this;
            }
            
            public Builder AddPurchasedProduct(List<string> value)
            {
                _purchasedProducts = value;
                return this;
            }

            public Builder SessionCurrencyTransactionAmount(float value)
            {
                _sessionCurrencyTransactionAmount = value;
                return this;
            }

            public Builder TotalBossLevelStartedCount(int value)
            {
                _totalBossLevelStartedCount = value;
                return this;
            }
            
            public Builder SessionBossLevelStartedCount(int value)
            {
                _sessionBossLevelStartedCount = value;
                return this;
            }

            public Builder TotalChallengeLevelStartedCount(int value)
            {
                _totalChallengeLevelStartedCount = value;
                return this;
            }
            
            public Builder SessionChallengeLevelStartedCount(int value)
            {
                _sessionChallengeLevelStartedCount = value;
                return this;
            }
            
            public Builder TotalBossLevelCompletedCount(int value)
            {
                _totalBossLevelCompletedCount = value;
                return this;
            }
            
            public Builder SessionBossLevelCompletedCount(int value)
            {
                _sessionBossLevelCompletedCount = value;
                return this;
            }

            public Builder TotalChallengeLevelCompletedCount(int value)
            {
                _totalChallengeLevelCompletedCount = value;
                return this;
            }
            
            public Builder SessionChallengeLevelCompletedCount(int value)
            {
                _sessionChallengeLevelCompletedCount = value;
                return this;
            }

            public OfferMetaData Build()
            {
                var metaData = new OfferMetaData(
                    _totalIAPCount,
                    _totalIAPLTV,
                    _currencyAmount,
                    _totalCurrencyTransactionAmount,
                    _inventoryItems,
                    _sessionIAPCount,
                    _sessionIAPLTV,
                    _sessionCurrencyTransactionAmount,
                    _triggerPoint,
                    _subscriptionType,
                    _purchasedProducts,
                    _totalBossLevelStartedCount,
                    _totalBossLevelCompletedCount,
                    _totalChallengeLevelStartedCount,
                    _totalChallengeLevelCompletedCount,
                    _sessionBossLevelStartedCount,
                    _sessionBossLevelCompletedCount,
                    _sessionChallengeLevelStartedCount,
                    _sessionChallengeLevelCompletedCount
                );
                return metaData;
            }
        }
    }

    [Serializable]
    public class LastPlayedLevel
    {
        public string levelState = "";
        public int levelNumber = 0;
        public string levelId = "";
    }

    [Serializable]
    public class CurrencyAmount
    {
        public int health = 0;
        public int coin = 0;
        public int booster = 0;
    }

    [Serializable]
    public class InventoryItem
    {
        public string name = "";
        public int amount = 0;
        public int max_amount = 0;
    }
    
    [Serializable]
    public class FirstOfferTimestamp
    {
        public string key;
        public long timestamp;
    }
    
    [Serializable]
    public class OfferCounts
    {
        public string key;
        public int count;
    }
}