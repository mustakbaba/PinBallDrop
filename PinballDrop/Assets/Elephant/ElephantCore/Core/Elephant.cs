using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using RollicGames.Utils;
using UnityEngine;
#if UNITY_IOS && !UNITY_EDITOR
using UnityEngine.iOS;
#endif

namespace ElephantSDK
{
    public class Elephant
    {
        private static string LEVEL_STARTED = "level_started";
        private static string LEVEL_FAILED = "level_failed";
        private static string LEVEL_COMPLETED = "level_completed";

        private static MetaDataUtils _metaDataUtils;

        public static event Action OnLevelCompleted;
        public static event Action OnDismissOfferUI;
        private static IOfferListener _offerListener;
        private static bool isLiveOpsReady = false;

        public static event Action OnOfferClosed;
        public static event Action OnCollectibleClaimed;
        
        public static event Action<string> OnDeepLink;

        public static void Init(bool isOldUser = false, bool gdprSupported = false)
        {
            ElephantLog.Log("ELEPHANT", "Initializing SDK...");
            _metaDataUtils = MetaDataUtils.GetInstance();
            ElephantCore.Instance.Init();
        }

        public static void ShowFAQ()
        {
            ElephantCore.Instance.HelpShiftElephantAdapter?.ShowFAQs();
        }

        public static void ShowConversation()
        {
            ElephantCore.Instance.HelpShiftElephantAdapter?.ShowConversation();
        }
        
        public static void LoadStorage(IElephantStorage model)
        {
            ElephantLog.Log("STORAGE", "Loading storage data");
            ElephantStorageManager.GetInstance().LoadStorage(model);
        }

        public static void SaveStorage()
        {
            ElephantLog.Log("STORAGE", "Saving storage data");
            ElephantStorageManager.GetInstance().SaveStorage();
        }
        
        public static void TriggerCollectibleClaimed()
        {
            OnCollectibleClaimed?.Invoke();
        }
        
        public static void TriggerDeepLink(string url)
        {
            OnDeepLink?.Invoke(url);
        }
        
        public static IEnumerator ResetSoundWithDelay()
        {
            if (!ElephantCore.Instance.isSoundFixEnabled)
                yield break;

            yield return new WaitForSeconds(3);
            AudioSettings.Reset(AudioSettings.GetConfiguration());
        }
        
        public static void OpenURL(String url)
        {
#if UNITY_IOS
            ElephantIOS.openURL(url);
#else
            Application.OpenURL(url);
#endif
        }

        public static void DismissOfferUI()
        {
            if (OnDismissOfferUI != null)
                OnDismissOfferUI();
        }

        public static void SetOfferListener(IOfferListener offerListener)
        {
            _offerListener = offerListener;
            if(isLiveOpsReady)
                _offerListener?.OnLiveOpsReady();
        }
        
        public static void TriggerOfferPurchaseRequested(PurchaseOption purchaseOption)
        {
            _offerListener?.OnLiveOpsOfferPurchaseRequested(purchaseOption);
        }
        
        public static void TriggerOfferDismissed(List<PurchaseOption> purchaseOptions)
        {
            _offerListener?.OnLiveOpsOfferDismissed(purchaseOptions);
        }
        
        public static void TriggerOfferShown(List<PurchaseOption> purchaseOptions)
        {
            _offerListener?.OnLiveOpsOfferShown(purchaseOptions);
        }

        public static void TriggerLiveOpsReady(bool isOfferAssetsReady, bool isOfferProductsReady)
        {
#if UNITY_EDITOR
            isLiveOpsReady = isOfferAssetsReady;
#else
            isLiveOpsReady = isOfferAssetsReady && isOfferProductsReady;
#endif
            if(isLiveOpsReady)
                _offerListener?.OnLiveOpsReady();
        }

        public static void ShowComplianceDialog()
        {
            ElephantCore.Instance.PinRequest();
        }

        public static void ShowSupportView(string subject, string body)
        {
            subject = Utils.ReplaceEscapeCharsForUrl(subject);
            body = body + "\n\n" +
                   "Elephant ID: " + ElephantCore.Instance.userId + 
                   "\nIDFV: " + ElephantCore.Instance.idfv +
                   "\nGame ID: " + ElephantCore.Instance.gameID;
            body = Utils.ReplaceEscapeCharsForUrl(body);
            Application.OpenURL("mailto:" + "support@rollicgames.com" + "?subject=" + subject + "&body=" + body);
        }

        public static void IsIapBanned(Action<bool, string> callback)
        {
            if (ElephantCore.Instance == null)
            {
                Debug.LogWarning("Elephant SDK isn't working correctly, make sure you put Elephant prefab into your first scene..");
                return;
            }

            if (callback == null)
            {
                Debug.LogError("IsIapBanned callback cannot be null!");
                return;
            }

            ElephantCore.Instance.IsIapBanned(callback);
        }

        public static void VerifyPurchase(IapVerifyRequest request, Action<bool> callback)
        {
            ElephantLog.LogCustomKey("last_iap_product", request.product_id);
            if (ElephantCore.Instance == null)
            {
                ElephantLog.LogError("<ELEPHANT>", "Elephant SDK isn't working correctly, make sure you put Elephant prefab into your first scene..");
                return;
            }

            if (callback == null)
            {
                throw new NullReferenceException("ValidatePurchase callback cannot be null!");
            }

            ElephantLog.Log("IAP", $"Verifying purchase: {request.product_id}");
            ElephantCore.Instance.VerifyPurchase(request, callback);
        }

        public static void ShowAlertDialog(string title, string message)
        {
#if UNITY_IOS
            ElephantIOS.showAlertDialog(title, message);
#elif UNITY_ANDROID
            ElephantAndroid.showAlertDialog(title, message);
#else
            Debug.LogError(message);
#endif
        }

        public static void IsOfferEligibleToShow(OfferMetaData offerMetaData, Action<OfferData> callback)
        {
            if (ElephantCore.Instance == null)
            {
                ElephantLog.LogError("<ELEPHANT>", "Elephant SDK isn't working correctly, make sure you put Elephant prefab into your first scene..");
                return;
            }

            if (callback == null)
            {
                throw new NullReferenceException("IsOfferEligibleToShow callback cannot be null!");
            }

            if (!RemoteConfig.GetInstance().GetBool("live_ops_tool_enabled", false))
            {
                ElephantLog.LogError("<OFFER>", "Live-Ops Tool Disabled");
                return;
            }
            
            if (ElephantCore.Instance.LiveOpsElephantAdapter == null)
            {
                ElephantLog.LogError("<ELEPHANT>", "Elephant LiveOps is not implemented");
                return;
            }

            ElephantLog.Log("OFFER", $"Checking offer eligibility: {offerMetaData?.triggerPoint}");
            ElephantCore.Instance.LiveOpsElephantAdapter?.OfferGenerateRequest(offerMetaData, callback);
        }

        public static void LevelStarted(int level, string originalLevelId, Params parameters = null)
        {
            ElephantLog.LogCustomKey("last_completed_level", level.ToString());
            ElephantLog.LogCustomKey("last_completed_level_id", originalLevelId);
            ElephantLog.LogCustomKey("total_level_completes", _metaDataUtils.GetLevelCompleteCount().ToString());

            MonitoringUtils.GetInstance().SetCurrentLevel(level, originalLevelId);
            _metaDataUtils.SetToPrefs(MetaDataKeys.KeyCurrentLevel, level);
            _metaDataUtils.IncrementByOne(MetaDataKeys.KeyLevelStartCount);
            _metaDataUtils.IncrementByOne(MetaDataKeys.KeySessionLevelStartCount);
            _metaDataUtils.SetToPrefs(MetaDataKeys.KeyCurrentLevelId, originalLevelId);

            CustomEvent(LEVEL_STARTED, level, originalLevelId: originalLevelId, param: parameters);
        }

        public static void LevelCompleted(int level, string originalLevelId, Params parameters = null)
        {
            ElephantLog.LogCustomKey("last_completed_level", level.ToString());
            ElephantLog.LogCustomKey("last_completed_level_id", originalLevelId);
            ElephantLog.LogCustomKey("total_level_completes", _metaDataUtils.GetLevelCompleteCount().ToString());

            var currentLevel = MonitoringUtils.GetInstance().GetCurrentLevel();
            var currentTime = Utils.Timestamp();
            var levelTime = currentTime - currentLevel.level_time;

            _metaDataUtils.SetToPrefs(MetaDataKeys.KeyLastLevel, level);
            _metaDataUtils.SetToPrefs(MetaDataKeys.KeyLastLevelState, "SUCCESS");
            _metaDataUtils.SetToPrefs(MetaDataKeys.KeyLastLevelId, originalLevelId);
            _metaDataUtils.IncrementByOne(MetaDataKeys.KeyLevelCompleteCount);
            _metaDataUtils.IncrementByOne(MetaDataKeys.KeySessionLevelCompleteCount);
            _metaDataUtils.UpdateLastXLevelsFailCount('W');

            CustomEvent(LEVEL_COMPLETED, level, originalLevelId, levelTime, parameters);

            if (OnLevelCompleted == null) return;
            var evnt = OnLevelCompleted;
            evnt?.Invoke();

            var rateUsLevel = RemoteConfig.GetInstance().GetInt("gamekit_rate_us_show_on_level_completed", -1);
            if (rateUsLevel != -1 && rateUsLevel == level)
            {
                RequestRateUs(level);
            }
        }

        public static void LevelFailed(int level, string originalLevelId, Params parameters = null)
        {
            ElephantLog.LogCustomKey("last_completed_level", level.ToString());
            ElephantLog.LogCustomKey("last_completed_level_id", originalLevelId);
            ElephantLog.LogCustomKey("total_level_completes", _metaDataUtils.GetLevelCompleteCount().ToString());
            
            var currentLevel = MonitoringUtils.GetInstance().GetCurrentLevel();
            var currentTime = Utils.Timestamp();
            var levelTime = currentTime - currentLevel.level_time;

            _metaDataUtils.IncrementByOne(MetaDataKeys.KeySessionLevelFailCount);
            if (_metaDataUtils.GetSessionLevelRecurringFailCount() == 0 || level == _metaDataUtils.GetLastLevel())
            {
                _metaDataUtils.IncrementByOne(MetaDataKeys.KeySessionLevelRecurringFailCount);
            }

            _metaDataUtils.SetToPrefs(MetaDataKeys.KeyLastLevel, level);
            _metaDataUtils.SetToPrefs(MetaDataKeys.KeyLastLevelState, "FAILED");
            _metaDataUtils.UpdateLastXLevelsFailCount('F');
            _metaDataUtils.SetToPrefs(MetaDataKeys.KeyLastLevelId, originalLevelId);

            CustomEvent(LEVEL_FAILED, level, originalLevelId: originalLevelId, levelTime: levelTime, param: parameters);
        }

        public static void Event(string type, int level, Params parameters = null, string originalLevelId = "")
        {
            CustomEvent(type, level, originalLevelId: originalLevelId, param: parameters);
        }

        public static void AdEventV2(string type, string json)
        {
            ElephantLog.Log("AD", $"Event: {type} JSON: {json}");

            if (!ElephantCore.Instance.GetOpenResponse().ad_config.ad_callback_logs) return;

            var param = Params.New();
            param.CustomString(json);

            AdEvent("AdEvent_" + type, -1, param: param);
        }

        public static void RewardedEvent(string eventType, ElephantLevel level, string type, string source, string item, string adUuid, int result = -1, string mediationInfo = "")
        {
            var parameters = Params.New()
                .Set("ad_placement_category", type)
                .Set("ad_placement_source", source)
                .Set("ad_placement_item", item)
                .Set("mediation_info", mediationInfo + "|" + adUuid)
                .Set("result", result);

            CustomEvent(eventType, level.level, originalLevelId: level.original_level, param: parameters);
        }

        public static void InterstitialEvent(string eventType, ElephantLevel level, string source, string adUuid, int result = -1, string mediationInfo = "")
        {
            var parameters = Params.New()
                .Set("ad_placement_source", source)
                .Set("mediation_info", mediationInfo + "|" + adUuid)
                .Set("result", result);

            CustomEvent(eventType, level.level, originalLevelId: level.original_level, param: parameters);
        }
        
        private static void RequestRateUs(int level)
        {
#if UNITY_IOS && !UNITY_EDITOR
            Event("RateUs_ShowCalled", level);
            var popupShown = Device.RequestStoreReview();
            Event(popupShown ? "RateUs_Shown" : "RateUs_NotShown", level);
#else
            ElephantLog.Log("RateUs","RateUs only works on IOS");
#endif
        }

        public static void Transaction(string type, int level, long amount, long finalAmount, string source)
        {
            if (ElephantCore.Instance == null)
            {
                Debug.LogWarning("Elephant SDK isn't working correctly, make sure you put Elephant prefab into your first scene..");
                return;
            }

            var t = TransactionData.CreateTransactionData();
            t.type = type;
            t.level = level;
            t.amount = amount;
            t.final_amount = finalAmount;
            t.source = source;

            var req = new ElephantRequest(ElephantConstants.TRANSACTION_EP, t);
            ElephantCore.Instance.AddToQueue(req);
            ElephantCore.Instance.ZyngaPublishingElephantAdapter?.LogGameEconomyEvent(source, type, amount,
                finalAmount - amount, finalAmount);
        }

        public static void OfferShownEvent(string userSegment, string iapItemName, string offerName, string triggerPoint)
        {
            _metaDataUtils = MetaDataUtils.GetInstance();
            var liveOpsElephantAdapter = ElephantCore.Instance.LiveOpsElephantAdapter;
            if (liveOpsElephantAdapter == null)
            {
                ElephantLog.LogError("LIVEOPS", "LiveOpsElephantAdapter is NULL");
                return;
            }
            
            var currentOfferResponse = liveOpsElephantAdapter.GetCurrentOfferResponse();
            _metaDataUtils.AddOffer(currentOfferResponse);
            _metaDataUtils.AddFirstOffer(currentOfferResponse, triggerPoint);
            var parameters = SetOfferParameters(userSegment, iapItemName, offerName, triggerPoint);
            CustomEvent("elephant_offer_shown", MonitoringUtils.GetInstance().GetCurrentLevel().level, originalLevelId: MonitoringUtils.GetInstance().GetCurrentLevel().original_level, param: parameters);
        }

        public static void OfferAcceptedEvent(string userSegment, string iapItemName, string offerName, string triggerPoint)
        {
            var parameters = SetOfferParameters(userSegment, iapItemName, offerName, triggerPoint);
            _metaDataUtils.AddPurchasedOffer(offerName);
            CustomEvent("elephant_offer_purchased", MonitoringUtils.GetInstance().GetCurrentLevel().level, originalLevelId: MonitoringUtils.GetInstance().GetCurrentLevel().original_level, param: parameters);
        }

        public static void OfferCanceledEvent(string closeReason, string userSegment, string iapItemName, string offerName, string triggerPoint)
        {
            if (OnOfferClosed != null)
                OnOfferClosed();
            var parameters = SetOfferParameters(userSegment, iapItemName, offerName, triggerPoint, closeReason);
            CustomEvent("elephant_offer_canceled", MonitoringUtils.GetInstance().GetCurrentLevel().level, originalLevelId: MonitoringUtils.GetInstance().GetCurrentLevel().original_level, param: parameters);
        }

        public static void OfferClosedEvent(string userSegment, string iapItemName, string offerName, string triggerPoint)
        {
            var parameters = SetOfferParameters(userSegment, iapItemName, offerName, triggerPoint);
            CustomEvent("elephant_offer_closed", MonitoringUtils.GetInstance().GetCurrentLevel().level, originalLevelId: MonitoringUtils.GetInstance().GetCurrentLevel().original_level, param: parameters);
        }

        private static Params SetOfferParameters(string userSegment, string iapItemName, string offerName, string triggerPoint, string closeReason = null)
        {

            // we need to use keys as data point
            // i.e. key string 1 -> segmentinfo
            // closeReason has a special case. ask Mertkan and Nikan
            var offerParams = Params.New();
            offerParams.Set("userSegment", userSegment);
            offerParams.Set("iapItemName", iapItemName);
            offerParams.Set(offerName, triggerPoint);
            offerParams.Set("closeReason", closeReason);

            return offerParams;
        }

        private static void CustomEvent(string type, int level, string originalLevelId = "", long levelTime = 0, Params param = null)
        {
            ElephantLog.Log("EVENT", $"Event: {type}, Level: {level}, Parameters: {param}");

            if (ElephantCore.Instance == null)
            {
                Debug.LogWarning("Elephant SDK isn't working correctly, make sure you put Elephant prefab into your first scene..");
                return;
            }

            var ev = EventData.CreateEventData();
            ev.type = type;
            ev.level = level;
            ev.ltv = LtvManager.GetInstance().LifeTimeRevenue;
            ev.level_id = !string.IsNullOrEmpty(originalLevelId)
                ? originalLevelId 
                : MonitoringUtils.GetInstance().GetCurrentLevel().original_level;
            
            ev.level_time = levelTime;

            if (param != null)
            {
                MapParams(param, ev);
            }

            var req = new ElephantRequest(ElephantConstants.EVENT_EP, ev);
            ElephantCore.Instance.AddToQueue(req);
        }
        
        private static void AdEvent(string type, int level, string originalLevelId = "", long levelTime = 0, Params param = null)
        {
            if (ElephantCore.Instance == null)
            {
                Debug.LogWarning("Elephant SDK isn't working correctly, make sure you put Elephant prefab into your first scene..");
                return;
            }

            var ev = EventData.CreateEventData();
            ev.type = type;
            ev.level = level;
            ev.ltv = LtvManager.GetInstance().LifeTimeRevenue;
            ev.level_id = !string.IsNullOrEmpty(originalLevelId)
                ? originalLevelId 
                : MonitoringUtils.GetInstance().GetCurrentLevel().original_level;
            
            ev.level_time = levelTime;

            if (param != null)
            {
                MapParams(param, ev);
            }

            var req = new ElephantRequest(ElephantConstants.AD_EVENT_EP, ev);
            ElephantCore.Instance.AddToQueue(req);
        }

        public static void ShowSettingsView()
        {
#if UNITY_EDITOR
// No-op
#elif UNITY_IOS
                    ElephantIOS.showSettingsView("LOADING", "", ElephantCore.Instance.UsercentricsElephantAdapter.GetIsUcInitialized(), ElephantCore.Instance.userId);
#elif UNITY_ANDROID
                    ElephantAndroid.ShowSettingsViewOnUiThread("LOADING", "", ElephantCore.Instance.UsercentricsElephantAdapter.GetIsUcInitialized(), ElephantCore.Instance.userId);
#endif

            ElephantCore.Instance.GetSettingsContent(response =>
            {
                if (response.responseCode == 200)
                {
                    string responseString = JsonConvert.SerializeObject(response.data);

#if UNITY_EDITOR
// No-op
#elif UNITY_IOS
                    ElephantIOS.showSettingsView("CONTENT", responseString, ElephantCore.Instance.UsercentricsElephantAdapter.GetIsUcInitialized(), ElephantCore.Instance.userId);
#elif UNITY_ANDROID
                    ElephantAndroid.ShowSettingsViewOnUiThread("CONTENT", responseString, ElephantCore.Instance.UsercentricsElephantAdapter.GetIsUcInitialized(), ElephantCore.Instance.userId);
#endif
                }
                else
                {
#if UNITY_EDITOR
// No-op
#elif UNITY_IOS
                    ElephantIOS.showSettingsView("ERROR", "", ElephantCore.Instance.UsercentricsElephantAdapter.GetIsUcInitialized(), ElephantCore.Instance.userId);
#elif UNITY_ANDROID
                    ElephantAndroid.ShowSettingsViewOnUiThread("ERROR", "", ElephantCore.Instance.UsercentricsElephantAdapter.GetIsUcInitialized(), ElephantCore.Instance.userId);
#endif

                    Debug.Log("Settings Error: " + response.responseCode + " " + response.errorMessage);
                }
            }, s =>
            {
#if UNITY_EDITOR
// No-op
#elif UNITY_IOS
                    ElephantIOS.showSettingsView("ERROR", "", ElephantCore.Instance.UsercentricsElephantAdapter.GetIsUcInitialized(), ElephantCore.Instance.userId);
#elif UNITY_ANDROID
                    ElephantAndroid.ShowSettingsViewOnUiThread("ERROR", "", ElephantCore.Instance.UsercentricsElephantAdapter.GetIsUcInitialized(), ElephantCore.Instance.userId);
#endif

                Debug.Log("Settings Error: " + s);
            });
        }


        public static void ShowNetworkOfflineDialog()
        {
            if (!Utils.IsConnected())
            {
                ElephantLog.LogError("NETWORK", "No internet connection detected");
                ElephantLog.LogCustomKey("last_offline_session", ElephantCore.Instance.GetCurrentSession().GetSessionID().ToString());
                if (ElephantCore.Instance != null)
                {
                    Utils.SaveToFile(ElephantConstants.OFFLINE_FLAG, ElephantCore.Instance.GetCurrentSession().GetSessionID().ToString());
                }
#if UNITY_EDITOR
                ElephantLog.Log("Connection", "No internet connection.\nPlease check your internet settings.");
#elif UNITY_IOS
                ElephantIOS.showNetworkOfflinePopUpView("No internet connection.\nPlease check your internet settings.", "Try Again");
#elif UNITY_ANDROID
                ElephantAndroid.ShowNetworkOfflineDialog("No internet connection.\nPlease check your internet settings.", "Try Again");
#endif

                Utils.PauseGame();
            }
        }

        private static void MapParams(Params param, EventData ev)
        {
            ev.custom_data = param.customData;

            int c = 0;
            foreach (var k in param.stringVals.Keys)
            {
                var v = param.stringVals[k];
                if (c == 0)
                {
                    ev.key_string1 = k;
                    ev.value_string1 = v;
                }
                else if (c == 1)
                {
                    ev.key_string2 = k;
                    ev.value_string2 = v;
                }
                else if (c == 2)
                {
                    ev.key_string3 = k;
                    ev.value_string3 = v;
                }
                else if (c == 3)
                {
                    ev.key_string4 = k;
                    ev.value_string4 = v;
                }

                c++;
            }


            c = 0;
            foreach (var k in param.intVals.Keys)
            {
                var v = param.intVals[k];
                if (c == 0)
                {
                    ev.key_int1 = k;
                    ev.value_int1 = v;
                }
                else if (c == 1)
                {
                    ev.key_int2 = k;
                    ev.value_int2 = v;
                }
                else if (c == 2)
                {
                    ev.key_int3 = k;
                    ev.value_int3 = v;
                }
                else if (c == 3)
                {
                    ev.key_int4 = k;
                    ev.value_int4 = v;
                }

                c++;
            }

            c = 0;
            foreach (var k in param.doubleVals.Keys)
            {
                var v = param.doubleVals[k];
                if (c == 0)
                {
                    ev.key_double1 = k;
                    ev.value_double1 = v;
                }
                else if (c == 1)
                {
                    ev.key_double2 = k;
                    ev.value_double2 = v;
                }
                else if (c == 2)
                {
                    ev.key_double3 = k;
                    ev.value_double3 = v;
                }
                else if (c == 3)
                {
                    ev.key_double4 = k;
                    ev.value_double4 = v;
                }

                c++;
            }
        }

        public class IapSource
        {
            public const string SOURCE_SHOP = "shop";
            public const string SOURCE_OFFERWALL = "offerwall";
            public const string SOURCE_IN_GAME = "in_game";
            public const string SOURCE_LEVEL_END = "level_end";
        }
    }
}