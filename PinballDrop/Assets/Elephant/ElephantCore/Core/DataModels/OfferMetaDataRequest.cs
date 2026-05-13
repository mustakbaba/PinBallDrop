using System;
using System.Collections.Generic;
using UnityEngine.Serialization;

namespace ElephantSDK
{
    [Serializable]
    public class OfferMetaDataRequest : BaseData
    {
        public Level level;
        public TotalStats total_stats;
        public List<Offer> offers;
        public List<InventoryItem> inventory_items;
        public CurrencyAmount currency_amount;
        public SessionStats session_stats;
        public string trigger_point;
        public List<FirstOfferTimestamp> first_offer_timestamps;
        public List<string> purchased_offers;
        public List<string> purchased_products;
        public string subscription_type;
        public List<OfferCounts> offer_counts;

        public static OfferMetaDataRequest FillOfferMetaDataRequest(OfferMetaData offerMetaData)
        {
            var level = new Level
            {
                current = offerMetaData.currentLevel,
                last_played = offerMetaData.lastPlayedLevel.levelNumber,
                state = offerMetaData.lastPlayedLevel.levelState,
                status = offerMetaData.lastXLevelsFailCount,
                last_id = offerMetaData.lastPlayedLevel.levelId,
                current_id = offerMetaData.currentLevelId
            };

            var totalStats = new TotalStats
            {
                interstitial_count = offerMetaData.totalInterstitialCount,
                rewarded_count = offerMetaData.totalRewardedCount,
                iap_count = offerMetaData.totalIAPCount,
                playtime = offerMetaData.totalPlaytime,
                level_start_count = offerMetaData.totalLevelStartCount,
                level_complete_count = offerMetaData.totalLevelCompleteCount,
                session_count = offerMetaData.totalSessionCount,
                iap_ltv = offerMetaData.totalIAPLTV,
                ad_ltv = offerMetaData.totalAdLTV,
                currency_transaction_amount = offerMetaData.totalCurrencyTransactionAmount,
                boss_level_started_count = offerMetaData.sessionBossLevelStartedCount,
                boss_level_completed_count = offerMetaData.sessionBossLevelCompletedCount,
                challenge_level_started_count = offerMetaData.sessionChallengeLevelStartedCount,
                challenge_level_completed_count = offerMetaData.sessionChallengeLevelCompletedCount,
            };

            var offers = offerMetaData.offers;
            foreach (var offer in offers)
            {
                offer.template = null;
                offer.template_fields = null;
            }
            var firstOfferTimestamps = offerMetaData.firstOfferTimestamps;
            var purchasedOffers = offerMetaData.purchasedOffers;
            var purchasedProducts = offerMetaData.purchasedProducts;
            var inventoryItems = offerMetaData.inventoryItems;
            var currencyAmount = offerMetaData.currencyAmount;
            var triggerPoint = offerMetaData.triggerPoint;
            var subscriptionType = offerMetaData.subscriptionType;
            var offerCounts = offerMetaData.offerCounts;
            var sessionStats = new SessionStats
            {
                interstitial_count = offerMetaData.sessionInterstitialCount,
                rewarded_count = offerMetaData.sessionRewardedCount,
                iap_count = offerMetaData.sessionIAPCount,
                playtime = offerMetaData.sessionPlaytime,
                level_start_count = offerMetaData.sessionLevelStartCount,
                level_complete_count = offerMetaData.sessionLevelCompleteCount,
                daily_session_count = offerMetaData.dailySessionCount,
                iap_ltv = offerMetaData.sessionIAPLTV,
                ad_ltv = offerMetaData.sessionAdLTV,
                currency_transaction_amount = offerMetaData.sessionCurrencyTransactionAmount,
                list_of_offers = offerMetaData.sessionListOfOffers,
                fail_count = offerMetaData.sessionFailCount,
                recurring_fail_count = offerMetaData.sessionRecurringFailCount,
                boss_level_started_count = offerMetaData.sessionBossLevelStartedCount,
                boss_level_completed_count = offerMetaData.sessionBossLevelCompletedCount,
                challenge_level_started_count = offerMetaData.sessionChallengeLevelStartedCount,
                challenge_level_completed_count = offerMetaData.sessionChallengeLevelCompletedCount,
            };
            foreach (var sessionOffer in sessionStats.list_of_offers)
            {
                sessionOffer.template = null;
                sessionOffer.template_fields = null;
            }

            return FillOfferMetaDataRequestInternal(level, totalStats, offers, purchasedOffers, purchasedProducts, firstOfferTimestamps, inventoryItems, currencyAmount,
                sessionStats, triggerPoint, subscriptionType, offerCounts);
        }

        private static OfferMetaDataRequest FillOfferMetaDataRequestInternal(Level level, TotalStats totalStats, List<Offer> offers, List<string> purchasedOffers, List<string> purchasedProducts, List<FirstOfferTimestamp> firstOfferTimestamps, List<InventoryItem> inventoryItems,
            CurrencyAmount currencyAmount, SessionStats sessionStats, string triggerPoint, string subscriptionType, List<OfferCounts> offerCounts)
        {
            var offerMetaDataRequest = new OfferMetaDataRequest
            {
                level = level,
                total_stats = totalStats,
                offers = offers,
                first_offer_timestamps = firstOfferTimestamps,
                purchased_offers = purchasedOffers,
                purchased_products = purchasedProducts,
                inventory_items = inventoryItems,
                currency_amount = currencyAmount,
                session_stats = sessionStats,
                trigger_point = triggerPoint,
                subscription_type = subscriptionType,
                offer_counts = offerCounts
            };
            offerMetaDataRequest.FillBaseData(ElephantCore.Instance.GetCurrentSession().GetSessionID());
            return offerMetaDataRequest;
        }
    }

    [Serializable]
    public class Level
    {
        public int current;
        public int last_played;
        public string state;
        public string status;
        public string current_id;
        public string last_id;
    }

    [Serializable]
    public class TotalStats
    {
        public int interstitial_count;
        public int rewarded_count;
        public int iap_count;
        public long playtime;
        public int level_start_count;
        public int level_complete_count;
        public int session_count;
        public float iap_ltv;
        public float ad_ltv;
        public float currency_transaction_amount;
        public int boss_level_started_count;
        public int boss_level_completed_count;
        public int challenge_level_started_count;
        public int challenge_level_completed_count;
    }

    [Serializable]
    public class SessionStats
    {
        public int interstitial_count;
        public int rewarded_count;
        public int iap_count;
        public long playtime;
        public int level_start_count;
        public int level_complete_count;
        public int daily_session_count;
        public float iap_ltv;
        public float ad_ltv;
        public float currency_transaction_amount;
        public List<Offer> list_of_offers;
        public int fail_count;
        public int recurring_fail_count;
        public int boss_level_started_count;
        public int boss_level_completed_count;
        public int challenge_level_started_count;
        public int challenge_level_completed_count;
    }
}