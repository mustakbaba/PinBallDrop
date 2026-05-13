using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Timers;
using AmazonAds;
using UnityEngine;
using ElephantSDK;
using Facebook.Unity;
using Newtonsoft.Json;
using RollicGames.Model;
using RollicGames.Utils;
#if UNITY_IOS && !UNITY_EDITOR
using UnityEngine.iOS;
#endif

#if ELEPHANT_AUDIOMOBS
using Audiomob;
#endif

namespace RollicGames.Advertisements
{
    /**
     * DO NOT MODIFY THIS FILE!
     */
    public class RLAdvertisementManager : MonoBehaviour
    {
        public static event Action OnRollicAdsSdkInitializedEvent;
        public static event Action OnRollicAdsAdLoadedEvent;
        public static event Action<string> OnRollicAdsAdFailedEvent;
        public static event Action OnRollicAdsAdClickedEvent;
        public static event Action<string> OnRollicAdsAdExpandedEvent;
        public static event Action<string> OnRollicAdsAdCollapsedEvent;
        public static event Action<string> OnRollicAdsInterstitialLoadedEvent;
        public static event Action<IronSourceError> OnRollicAdsInterstitialFailedEvent;
        public static event Action OnRollicAdsInterstitialDismissedEvent;
        public static event Action<string> OnRollicAdsInterstitialExpiredEvent;
        public static event Action OnRollicAdsInterstitialShownEvent;
        public static event Action OnRollicAdsInterstitialAdShowFailedEvent;
        public static event Action OnRollicAdsInterstitialClickedEvent;
        public static event Action<string> OnRollicAdsRewardedVideoLoadedEvent;
        public static event Action<string, string> OnRollicAdsRewardedVideoFailedEvent;
        public static event Action OnRollicAdsRewardedVideoShownEvent;
        public static event Action OnRollicAdsRewardedVideoClickedEvent;
        public static event Action OnRollicAdsRewardedVideoFailedToPlayEvent;
        public static event Action<string> OnRollicAdsRewardedVideoReceivedRewardEvent;
        public static event Action OnRollicAdsRewardedVideoClosedEvent;
        public static event Action<string> OnRollicAdsRewardedVideoLeavingApplicationEvent;
        public static event Action OnFirebaseInitialized;


        private static RLAdvertisementManager instance = null;
        private bool _isRewardAvailable = false;
        public bool _isMediationInitialized;
        private long _mediationInitializeTime;
        private bool _hasInitMediationStarted = false;

        public Action<RLRewardedAdResult> rewardedAdResultCallback { get; set; }

        private string _appKey;
        private string _interstitialAdUnit;
        private string _interstitialHighAdUnit;
        private string _interstitialMidAdUnit;
        private string _interstitialNormalAdUnit;
        private string _rewardedVideoAdUnit;
        private string _rewardedVideoHighAdUnit;
        private string _rewardedVideoMidAdUnit;
        private string _rewardedVideoNormalAdUnit;
        private string _bannerAdUnit;
        private string _bannerBackgroundColor = "";
        private bool _isAdaptiveBannerEnabled;
        private bool _isBidFloorEnabled;
        private bool _isBidFloorIntEnabled;
        private bool _isBidFloorRwEnabled;
        private bool _isBidFloorTestFlowEnabled;

        private bool _isInterstitialReady = false;
        private bool _isBannerAutoShowEnabled = true;

        private string _amazonAppId;
        private string _amazonBannerSlotId;
        private string _amazonInterstitialVideoSlotId;
        private string _amazonRewardedVideoSlotId;

        private int bannerRequestTimerIndex = 0;
        private int interstitialRequestTimerIndex = 0;
        private int rewardedRequestTimerIndex = 0;
        private List<int> timers;
        private List<string> defaultTimerList = new List<string> { "2", "4", "8", "16" };
        private RevenueForwardingUtils _revenueForwardingUtils;
        private LtvManager _ltvManager;

        private RollicRewardedAd _rollicRewardedAd;
        private RollicInterstitialAd _rollicInterstitialAd;

        private bool isFirstVideoInterstitialRequest = true;
        private bool isFirstRewardedVideoRequest = true;

        private Dictionary<string, bool> _interstitialAdUnitIdStatus = new Dictionary<string, bool>();
        private Dictionary<string, bool> _rewardedAdUnitIdStatus = new Dictionary<string, bool>();

        private static ConcurrentQueue<Action> _actions = new ConcurrentQueue<Action>();

        public int interstitialCycleId = 0;
        public int rewardedAdCycleId = 0;
        public int bannerAdCycleId = 0;

        private IAdjustElephantAdapter _adjustAdapter;
        private IFacebookElephantAdapter _facebookAdapter;
        private IZyngaPublishingElephantAdapter _zyngaPublishingAdapter;
        private IFirebaseElephantAdapter _firebaseAdapter;

        
        //Protoype fields
        private bool _isAdFreeDay;
        
        // Inter Auto fields
        private bool _autoShowEnabled;
        private int _interstitialDisplayInterval;
        private float _lastTimeAdDisplayed;
        private int _addedValue;
        private float _timeSinceLastTimeAdDisplayed;
        private int _firstLevelToDisplay;
        private int _firstInterDisplayTimeAfterStart;
        private int _levelFrequency;
        private int _lastLevelAdDisplayed;
        private string _interShowLogic;
        private int _firstInterstitialDelay;
        private Timer _intTimer;
        private int _intRemainingTime;
        private bool _isIntLocked;
        private const string InterstitialEventPrefix = "InterstitialEvent";
        public const string ShowLogicLevelBased = "level_based";
        public const string ShowLogicIncremental = "incremental";

        // Rate Us fields
        private bool _isRateUsEnabled;
        private int _firstRateUsLevelToDisplay;
        private int _firstRateUsDisplayAfterStart;
        private int _rateUsLevelFrequency;
        private int _lastLevelRateUsDisplayed;
        private string _rateUsShowLogic;
        private int _rateUsDisplayInterval;
        private float _lastTimeRateUsDisplayed;
        private int _firstRateUsDelay;
        private Timer _rateUsTimer;
        private int _rateUsRemainingTime;
        private bool _isRateUsLocked;
        private float _timeSinceLastRateUsDisplayed;
        private const string RateUsEventPrefix = "RateUsEvent";

        // Banner timer management
        private int _firstBannerDelay;
        private Timer _bannerTimer;
        private int _bannerRemainingTime;


#if ELEPHANT_AUDIOMOBS
        #region AudioMob

        public event Action<AdPlaybackStatus> AudioMobAdPlaybackStatusChanged;
        public event Action<string, AdFailureReason> AudioMobAdFailed;
        public static bool AudioMobPluginInitialized { get; set; }

        private bool _audioMobAdsActive = false;
        private bool _waitingInterstitialRequest = false;
        private RollicInterstitialAd.InterstitialAdSource _waitingInterstitialRequestSource;

        public bool audioMobActive = RemoteConfig.GetInstance().GetBool("audiomob_active", false);
        public bool audioMobAdsForceRequestAdsButtonActive =
 RemoteConfig.GetInstance().GetBool("audiomob_ads_force_request_ads_button_active", false);
        
        public bool audioMobUseTimerCooldown = RemoteConfig.GetInstance().GetBool("audiomob_use_timer_cooldown", false);
        
        public int audioMobStartLevel = RemoteConfig.GetInstance().GetInt("audiomob_start_level", 3);
        public int audioMobLevelCooldown = RemoteConfig.GetInstance().GetInt("audiomob_level_cooldown", 3);
        public int audioMobMaximumSkippableAdCount =
 RemoteConfig.GetInstance().GetInt("audiomob_maximum_skippable_ad_count", 0);

        public float audioMobSkippableAdTimerCooldown =
 RemoteConfig.GetInstance().GetFloat("audiomob_skippable_ad_timer_cooldown", 300f);
        
        public float audioMobAdPlayingVolume = RemoteConfig.GetInstance().GetFloat("audiomob_game_volume", -50f);
        public float audioMobAdsVolume = RemoteConfig.GetInstance().GetFloat("audiomob_ad_volume", -25f);
        
        public float audioMobAppearCooldownFromGameSceneStartRemoteConfig =
 RemoteConfig.GetInstance().GetFloat("audiomob_appear_cd_from_gamescene_start", 10f);
        public float audioMobAppearCooldownFromEveryLevelStartRemoteConfig =
 RemoteConfig.GetInstance().GetFloat("audiomob_appear_cd_from_every_level_start", 5f);
        
#if UNITY_EDITOR
        private bool _audioMobDebuggerActive = RemoteConfig.GetInstance().GetBool("audiomob_debugger_active", true);
#else
        private bool _audioMobDebuggerActive = RemoteConfig.GetInstance().GetBool("audiomob_debugger_active", false);
#endif
        private bool _audioMobShowTestAdsActive =
 RemoteConfig.GetInstance().GetBool("audiomob_show_test_ads_active", false);
        private bool _audioMobBlockInterstitialWhenAdsActive =
 RemoteConfig.GetInstance().GetBool("audiomob_block_interstitial_when_ads_active", true);
        private bool _audioMobHoldInterstitialRequestUntilAudioAdsCompleted =
 RemoteConfig.GetInstance().GetBool("audiomob_hold_interstitial_request_until_audio_ads_completed", true);



        private void CheckAudioMobsSystems()
        {
            if (audioMobActive && _audioMobHoldInterstitialRequestUntilAudioAdsCompleted)
            {
                if (_waitingInterstitialRequest && !_audioMobAdsActive)
                {
                    _waitingInterstitialRequest = false;
                    AudioMobLog("Holding Interstitial Request is over. Interstitial Requested");
                    showInterstitial(_waitingInterstitialRequestSource);
                }
            }
        }

        public void RequestAudioMobSkippableAds()
        {
            if (_isNoAdsPurchased)
            {
                return;
            }
            
            if (AudioMobPluginInitialized && AudiomobPlugin.Instance != null && AudiomobPlugin.Instance.IsAdAvailable(AudiomobPlugin.AdUnits.SkippableRectangle))
            {
                AudiomobPlugin.Instance.PlayAd(AudiomobPlugin.AdUnits.SkippableRectangle);
            }
        }

        private void OnAudioMobAdPlaybackStatusChanged(AdSequence adSequence, AdPlaybackStatus adPlaybackStatus)
        {
            switch (adPlaybackStatus)
            {
                case AdPlaybackStatus.Started:
                    _audioMobAdsActive = true;
                    break;
                default:
                    _audioMobAdsActive = false;
                    break;
            }
            
            AudioMobAdPlaybackStatusChanged?.Invoke(adPlaybackStatus);
            AudioMobLog($"OnAudioMobAdPlaybackStatusChanged => {adPlaybackStatus}");
        }

        private void OnAudioMobAdFailedEvent(string adUnitId, AdFailureReason adFailureReason)
        {
            AudioMobAdFailed?.Invoke(adUnitId, adFailureReason);
            AudioMobLog($"OnAudioMobAdFailedEvent => adUnitId:{adUnitId} - Reason: {adFailureReason}");
            _audioMobAdsActive = false;
        }

        public void AudioMobLog(string log)
        {
            if (_audioMobDebuggerActive)
            {
                Elephant.Event(log, -1);
#if UNITY_EDITOR
                Debug.LogWarning(log);
#endif
            }
        }

        private void OnAudiomobPaidEvent(IAudioAd adInfo)
        {
            if (_adjustAdapter == null)
            {
                AudioMobLog("AudiomobPaidEvent AdjustAdapter IS NULL");
                return;
            }

            var revenue = adInfo.Ecpm / 1000;
            if (revenue <= 0f)
            {
                ElephantLog.Log("AUDIOMOB", "Paid event revenue is zero");
                AudioMobLog("AudiomobPaidEvent Paid event revenue is zero");

                return;
            }

            AudioMobLog($"AudiomobPaidEvent Paid event revenue is {revenue}");

            var extraParams = new Dictionary<string, string>
            {
                { "ad_format", "Banner" }
            };

            _adjustAdapter.TrackAdRevenue(
                "applovin_max_sdk",
                revenue,
                "USD",
                "audiomob",
                adInfo.Id,
                "Banner",
                "Banner",
                adInfo.AdUnitId,
                extraParams
            );

            _revenueForwardingUtils.UpdateRevenue(revenue);
            _ltvManager?.UpdateRevenue(revenue);

            var impressionParameters = new Dictionary<string, object>
            {
                {"ad_platform", "applovin_max"},
                {"ad_source", "audiomob"},
                {"ad_unit_name", adInfo.Id},
                {"ad_format", "Banner"},
                {"value", revenue},
                {"currency", "USD"},
            };

            _firebaseAdapter?.LogEvent("ad_impression", impressionParameters);
            _firebaseAdapter?.LogEvent("custom_ad_impression", impressionParameters);

            var fbAdParams = new Dictionary<string, object>
            {
                [AppEventParameterName.Currency] = "USD"
            };
            _facebookAdapter?.LogAppEvent("AdImpression", revenue, fbAdParams);
        }


        #endregion

#endif

        private void SetAdapters()
        {
            if (ElephantCore.Instance == null) return;
            _adjustAdapter = ElephantCore.Instance.AdjustElephantAdapter;
            _facebookAdapter = ElephantCore.Instance.FacebookElephantAdapter;
            _zyngaPublishingAdapter = ElephantCore.Instance.ZyngaPublishingElephantAdapter;
            _firebaseAdapter = ElephantCore.Instance.FirebaseElephantAdapter;
        }

        public static RLAdvertisementManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindObjectOfType<RLAdvertisementManager>();
                    if (instance == null)
                    {
                        GameObject obj = new GameObject();
                        obj.name = typeof(RLAdvertisementManager).Name;
                        instance = obj.AddComponent<RLAdvertisementManager>();
                    }
                }

                return instance;
            }
        }

        public static void ExecuteOnMainThread(Action action)
        {
            if (action == null) return;

            _actions.Enqueue(action);
        }

        void Awake()
        {
            Elephant.OnLevelCompleted += OnLevelCompleted;
            if (instance == null)
            {
                instance = this as RLAdvertisementManager;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }

            SetAdapters();
            InitFirebase();
#if !UNITY_EDITOR && UNITY_ANDROID
            RollicAdsAndroid.Init();
#endif
        }

        private void Update()
        {
#if ELEPHANT_AUDIOMOBS
            CheckAudioMobsSystems();
#endif

            while (_actions.TryDequeue(out var action))
            {
                action.Invoke();
            }
        }
        
        public void StartAdManager()
        {
            var adInstance = Instance;
        }

        public void init(bool isDebugEnabled = false)
        {
            if (_autoShowEnabled)
            {
                ElephantLog.LogError("RLAdvertisementManager", "Auto show is enabled, don't manual init.");
                return;
            }
            InitInternal("", isDebugEnabled);
        }

        internal void InitInternal(string appKey = "", bool isDebugEnabled = false)
        {
            _appKey = RollicApplovinIDs.AppKey;

            var adConfig = ElephantCore.Instance.GetOpenResponse().ad_config;
            _isBidFloorEnabled = adConfig.bidfloor_enabled;
            _isBidFloorIntEnabled = adConfig.bidfloor_int_enabled;
            _isBidFloorRwEnabled = adConfig.bidfloor_rw_enabled;
            _isBidFloorTestFlowEnabled = adConfig.bidfloor_test_flow_enabled;

            ValidateBidFloorTestIds();

#if UNITY_IOS
            _bannerAdUnit = RemoteConfig.GetInstance()
                .Get("rl_custom_banner_ad_unit_id_ios", RollicApplovinIDs.BannerAdUnitIos);
            _interstitialAdUnit = RemoteConfig.GetInstance().Get("rl_custom_interstitial_ad_unit_id_ios",
                RollicApplovinIDs.InterstitialAdUnitIos);
            _rewardedVideoAdUnit = RemoteConfig.GetInstance()
                .Get("rl_custom_rewarded_ad_unit_id_ios", RollicApplovinIDs.RewardedAdUnitIos);

            if (_isBidFloorTestFlowEnabled)
            {
                _interstitialHighAdUnit = RollicApplovinIDs.TestInterstitialHighAdUnitIos;
                _interstitialMidAdUnit = RollicApplovinIDs.TestInterstitialMidAdUnitIos;
                _interstitialNormalAdUnit = RollicApplovinIDs.TestInterstitialNormalAdUnitIos;
                _rewardedVideoHighAdUnit = RollicApplovinIDs.TestRewardedHighAdUnitIos;
                _rewardedVideoMidAdUnit = RollicApplovinIDs.TestRewardedMidAdUnitIos;
                _rewardedVideoNormalAdUnit = RollicApplovinIDs.TestRewardedNormalAdUnitIos;
            }
            else
            {
                _interstitialHighAdUnit = RollicApplovinIDs.InterstitialHighAdUnitIos;
                _interstitialMidAdUnit = RollicApplovinIDs.InterstitialMidAdUnitIos;
                _interstitialNormalAdUnit = RollicApplovinIDs.InterstitialNormalAdUnitIos;
                _rewardedVideoHighAdUnit = RollicApplovinIDs.RewardedHighAdUnitIos;
                _rewardedVideoMidAdUnit = RollicApplovinIDs.RewardedMidAdUnitIos;
                _rewardedVideoNormalAdUnit = RollicApplovinIDs.RewardedNormalAdUnitIos;
            }

            _amazonAppId = RollicApplovinIDs.AmazonAppIdIos;
            _amazonBannerSlotId = RollicApplovinIDs.AmazonBannerSlotIdIos;
            _amazonInterstitialVideoSlotId = RollicApplovinIDs.AmazonInterstitialVideoSlotIdIos;
            _amazonRewardedVideoSlotId = RollicApplovinIDs.AmazonRewardedVideoSlotIdIos;

            if (string.IsNullOrEmpty(_bannerAdUnit))
            {
                _bannerAdUnit = RollicApplovinIDs.BannerAdUnitIos;
            }

            if (string.IsNullOrEmpty(_interstitialAdUnit))
            {
                _interstitialAdUnit = RollicApplovinIDs.InterstitialAdUnitIos;
            }

            if (string.IsNullOrEmpty(_rewardedVideoAdUnit))
            {
                _rewardedVideoAdUnit = RollicApplovinIDs.RewardedAdUnitIos;
            }

#elif UNITY_ANDROID || UNITY_EDITOR
            _bannerAdUnit =
                RemoteConfig.GetInstance().Get("rl_custom_banner_ad_unit_id_android",
                    RollicApplovinIDs.BannerAdUnitAndroid);
            _interstitialAdUnit =
                RemoteConfig.GetInstance().Get("rl_custom_interstitial_ad_unit_id_android",
                    RollicApplovinIDs.InterstitialAdUnitAndroid);
            _rewardedVideoAdUnit =
                RemoteConfig.GetInstance().Get("rl_custom_rewarded_ad_unit_id_android",
                    RollicApplovinIDs.RewardedAdUnitAndroid);

            if (_isBidFloorTestFlowEnabled)
            {
                _interstitialHighAdUnit = RollicApplovinIDs.TestInterstitialHighAdUnitAndroid;
                _interstitialMidAdUnit = RollicApplovinIDs.TestInterstitialMidAdUnitAndroid;
                _interstitialNormalAdUnit = RollicApplovinIDs.TestInterstitialNormalAdUnitAndroid;
                _rewardedVideoHighAdUnit = RollicApplovinIDs.TestRewardedHighAdUnitAndroid;
                _rewardedVideoMidAdUnit = RollicApplovinIDs.TestRewardedMidAdUnitAndroid;
                _rewardedVideoNormalAdUnit = RollicApplovinIDs.TestRewardedNormalAdUnitAndroid;
            }
            else
            {
                _interstitialHighAdUnit = RollicApplovinIDs.InterstitialHighAdUnitAndroid;
                _interstitialMidAdUnit = RollicApplovinIDs.InterstitialMidAdUnitAndroid;
                _interstitialNormalAdUnit = RollicApplovinIDs.InterstitialNormalAdUnitAndroid;
                _rewardedVideoHighAdUnit = RollicApplovinIDs.RewardedHighAdUnitAndroid;
                _rewardedVideoMidAdUnit = RollicApplovinIDs.RewardedMidAdUnitAndroid;
                _rewardedVideoNormalAdUnit = RollicApplovinIDs.RewardedNormalAdUnitAndroid;
            }

            _amazonAppId = RollicApplovinIDs.AmazonAppIdAndroid;
            _amazonBannerSlotId = RollicApplovinIDs.AmazonBannerSlotIdAndroid;
            _amazonInterstitialVideoSlotId = RollicApplovinIDs.AmazonInterstitialVideoSlotIdAndroid;
            _amazonRewardedVideoSlotId = RollicApplovinIDs.AmazonRewardedVideoSlotIdAndroid;

            if (string.IsNullOrEmpty(_bannerAdUnit))
            {
                _bannerAdUnit = RollicApplovinIDs.BannerAdUnitAndroid;
            }

            if (string.IsNullOrEmpty(_interstitialAdUnit))
            {
                _interstitialAdUnit = RollicApplovinIDs.InterstitialAdUnitAndroid;
            }

            if (string.IsNullOrEmpty(_rewardedVideoAdUnit))
            {
                _rewardedVideoAdUnit = RollicApplovinIDs.RewardedAdUnitAndroid;
            }

#endif

            var timerStringList = adConfig.GetList("retry_periods", defaultTimerList);

            timers = timerStringList
                .Select(s => Int32.TryParse(s, out int n) ? n : 0)
                .ToList();

            var threshold = RemoteConfig.GetInstance().GetDouble("rev_fw_threshold", 1000);
            var frequencyPercentage = RemoteConfig.GetInstance().GetInt("rev_fw_frequency_percentage", 10);
            _revenueForwardingUtils = RevenueForwardingUtils.GetInstance(threshold, frequencyPercentage);
            _ltvManager = LtvManager.GetInstance();
        }

        private void OnLevelCompleted()
        {
            if (_autoShowEnabled && !_isAdFreeDay && string.Equals(_interShowLogic, ShowLogicLevelBased))
            {
                ShowInterstitialLevelBased();
            }

            if (_isRateUsEnabled && string.Equals(_rateUsShowLogic, ShowLogicLevelBased))
            {
                ShowRateUsLevelBased();
            }
        }

        private bool ValidateBidFloorTestIds()
        {
            var isValid = true;
            var missingIds = "";

            if (!_isBidFloorTestFlowEnabled) return isValid;
#if UNITY_IOS
            if (string.IsNullOrEmpty(RollicApplovinIDs.TestInterstitialHighAdUnitIos))
            {
                isValid = false;
                missingIds += "TestInterstitialHighAdUnitIos,";
            }

            if (string.IsNullOrEmpty(RollicApplovinIDs.TestInterstitialMidAdUnitIos))
            {
                isValid = false;
                missingIds += "TestInterstitialMidAdUnitIos,";
            }

            if (string.IsNullOrEmpty(RollicApplovinIDs.TestInterstitialNormalAdUnitIos))
            {
                isValid = false;
                missingIds += "TestInterstitialNormalAdUnitIos,";
            }

            if (string.IsNullOrEmpty(RollicApplovinIDs.TestRewardedHighAdUnitIos))
            {
                isValid = false;
                missingIds += "TestRewardedHighAdUnitIos,";
            }

            if (string.IsNullOrEmpty(RollicApplovinIDs.TestRewardedMidAdUnitIos))
            {
                isValid = false;
                missingIds += "TestRewardedMidAdUnitIos,";
            }

            if (string.IsNullOrEmpty(RollicApplovinIDs.TestRewardedNormalAdUnitIos))
            {
                isValid = false;
                missingIds += "TestRewardedNormalAdUnitIos,";
            }
#elif UNITY_ANDROID || UNITY_EDITOR
            if (string.IsNullOrEmpty(RollicApplovinIDs.TestInterstitialHighAdUnitAndroid))
            {
                isValid = false;
                missingIds += "TestInterstitialHighAdUnitAndroid,";
            }
            if (string.IsNullOrEmpty(RollicApplovinIDs.TestInterstitialMidAdUnitAndroid))
            {
                isValid = false;
                missingIds += "TestInterstitialMidAdUnitAndroid,";
            }
            if (string.IsNullOrEmpty(RollicApplovinIDs.TestInterstitialNormalAdUnitAndroid))
            {
                isValid = false;
                missingIds += "TestInterstitialNormalAdUnitAndroid,";
            }
            if (string.IsNullOrEmpty(RollicApplovinIDs.TestRewardedHighAdUnitAndroid))
            {
                isValid = false;
                missingIds += "TestRewardedHighAdUnitAndroid,";
            }
            if (string.IsNullOrEmpty(RollicApplovinIDs.TestRewardedMidAdUnitAndroid))
            {
                isValid = false;
                missingIds += "TestRewardedMidAdUnitAndroid,";
            }
            if (string.IsNullOrEmpty(RollicApplovinIDs.TestRewardedNormalAdUnitAndroid))
            {
                isValid = false;
                missingIds += "TestRewardedNormalAdUnitAndroid,";
            }
#endif

            if (!isValid)
            {
                if (missingIds.EndsWith(","))
                {
                    missingIds = missingIds.Substring(0, missingIds.Length - 1);
                }

                var eventMessage = "Bid floor test flow disabled due to missing IDs: " + missingIds;
                ElephantLog.Log("BIDFLOOR", eventMessage);

                var param = Params.New().Set("missing_ids", missingIds);
                Elephant.Event("bidfloor_test_flow_disabled", -1, param);

                _isBidFloorTestFlowEnabled = false;
            }
            else
            {
                Elephant.Event("bidfloor_test_flow_enabled", -1);
            }

            return isValid;
        }

        void Start()
        {
            _autoShowEnabled = RemoteConfig.GetInstance().GetBool("gamekit_ads_enabled", false);
            _isAdFreeDay = ElephantCore.Instance.CheckAdFreePeriod();
#if UNITY_ANDROID || UNITY_EDITOR
            InitMediation("consent_disabled");
#elif UNITY_IOS
            if (!ElephantCore.Instance.GetOpenResponse().internal_config.idfa_consent_enabled)
            {
                InitMediation("consent_disabled");
            } else {
                StartCoroutine(CheckIdfaStatus());
            }
            
            _isRateUsEnabled = RemoteConfig.GetInstance().GetBool("gamekit_rate_us_enabled", false);
            if (_isRateUsEnabled)
            {
                InitRateUs();
            }
#endif
        }

        private IEnumerator CheckIdfaStatus()
        {
            while (IdfaConsentResult.GetInstance().GetStatus() == IdfaConsentResult.Status.Waiting)
            {
                yield return null;
            }

            InitMediation(IdfaConsentResult.GetInstance().GetIdfaResultValue());
        }

        private void InitMediation(string message)
        {
            if (ElephantCore.Instance.GetOpenResponse().internal_config.ads_disabled)
            {
                ElephantLog.Log("MEDIATION", "Ads are disabled via config");
                return;
            }

            if (_hasInitMediationStarted)
            {
                ElephantLog.Log("MEDIATION", "Mediation initialization already started");
                return;
            }

            ElephantLog.Log("MEDIATION", $"Starting mediation initialization with consent: {message}");

            _hasInitMediationStarted = true;
#if UNITY_IOS && !UNITY_EDITOR
            if (message.Equals("consent_disabled"))
            {
                Elephant.Event("facebook_tracking_enabled", -1);
                RollicAdsIos.setTrackingEnabled(true);
            }
            else
            {
                if (message.Equals("Authorized"))
                {
                    Elephant.Event("facebook_tracking_enabled", -1);
                    RollicAdsIos.setTrackingEnabled(true);
                }
                else
                {
                    Elephant.Event("facebook_tracking_disabled", -1);
                    RollicAdsIos.setTrackingEnabled(false);
                }
            }
            
            if (message.Equals("Denied") || message.Equals("Restricted"))
            {
                OnOnGDPRStateChangeEvent(false);
            }
#endif


            this._isMediationInitialized = false;
            if (_isBidFloorEnabled)
            {
                if (IsIntBidFloorAdUnitIdsOk())
                {
                    MaxSdk.SetExtraParameter("disable_b2b_ad_unit_ids",
                        $"{_interstitialHighAdUnit},{_interstitialMidAdUnit},{_interstitialNormalAdUnit}");
                }

                if (IsRwBidFloorAdUnitIdsOk())
                {
                    MaxSdk.SetExtraParameter("disable_b2b_ad_unit_ids",
                        $"{_rewardedVideoHighAdUnit},{_rewardedVideoMidAdUnit},{_rewardedVideoNormalAdUnit}");
                }
            }

#if !UNITY_EDITOR
            if (!string.IsNullOrEmpty(_amazonAppId))
            {
                Amazon.Initialize(_amazonAppId);
                Amazon.SetAdNetworkInfo(new AdNetworkInfo(DTBAdNetwork.MAX));
                Amazon.SetMRAIDPolicy(Amazon.MRAIDPolicy.CUSTOM);
                Amazon.SetMRAIDSupportedVersions(new string[] { "1.0", "2.0", "3.0" });
            }
#endif

            MaxSdk.SetSdkKey(_appKey);
            MaxSdk.SetVerboseLogging(RemoteConfig.GetInstance().GetBool("gamekit_ads_verbose_logging_enabled", false));
            StartCoroutine(AdjustIdWhenReady());
            SetSegmentTargeting();
            MaxSdk.InitializeSdk();


            MaxSdkCallbacks.OnSdkInitializedEvent += OnSdkInitializedEvent;

            MaxSdkCallbacks.Interstitial.OnAdLoadedEvent += OnInterstitialLoadedEvent;
            MaxSdkCallbacks.Interstitial.OnAdLoadFailedEvent += OnInterstitialFailedEvent;
            MaxSdkCallbacks.Interstitial.OnAdDisplayedEvent += OnInterstitialShownEvent;
            MaxSdkCallbacks.Interstitial.OnAdClickedEvent += OnInterstitialClickedEvent;
            MaxSdkCallbacks.Interstitial.OnAdHiddenEvent += OnInterstitialDismissedEvent;
            MaxSdkCallbacks.Interstitial.OnAdDisplayFailedEvent += OnInterstitialFailedToShowEvent;

            MaxSdkCallbacks.Rewarded.OnAdLoadedEvent += OnRewardedVideoLoadedEvent;
            MaxSdkCallbacks.Rewarded.OnAdLoadFailedEvent += OnRewardedVideoFailedEvent;
            MaxSdkCallbacks.Rewarded.OnAdDisplayedEvent += OnRewardedVideoShownEvent;
            MaxSdkCallbacks.Rewarded.OnAdClickedEvent += OnRewardedVideoClickedEvent;
            MaxSdkCallbacks.Rewarded.OnAdHiddenEvent += OnRewardedVideoClosedEvent;
            MaxSdkCallbacks.Rewarded.OnAdDisplayFailedEvent += OnRewardedVideoFailedToPlayEvent;
            MaxSdkCallbacks.Rewarded.OnAdReceivedRewardEvent += OnRewardedVideoReceivedRewardEvent;

            MaxSdkCallbacks.Banner.OnAdLoadedEvent += OnAdLoadedEvent;
            MaxSdkCallbacks.Banner.OnAdClickedEvent += OnAdClickedEvent;
            MaxSdkCallbacks.Banner.OnAdLoadFailedEvent += OnAdFailedEvent;
            MaxSdkCallbacks.Banner.OnAdCollapsedEvent += OnAdCollapsedEvent;
            MaxSdkCallbacks.Banner.OnAdExpandedEvent += OnAdExpandedEvent;

            MaxSdkCallbacks.Interstitial.OnAdRevenuePaidEvent +=
                (adUnitId, adInfo) => OnImpressionTrackedEvent(adUnitId, adInfo, RLAdFormat.Interstitial);
            MaxSdkCallbacks.Rewarded.OnAdRevenuePaidEvent +=
                (adUnitId, adInfo) => OnImpressionTrackedEvent(adUnitId, adInfo, RLAdFormat.RewardedAd);
            MaxSdkCallbacks.Banner.OnAdRevenuePaidEvent +=
                (adUnitId, adInfo) => OnImpressionTrackedEvent(adUnitId, adInfo, RLAdFormat.Banner);

            ElephantComplianceManager.GetInstance(ElephantCore.Instance.GetOpenResponse()).OnCCPAStateChangeEvent +=
                OnOnCCPAStateChangeEvent;
            ElephantComplianceManager.GetInstance(ElephantCore.Instance.GetOpenResponse()).OnGDPRStateChangeEvent +=
                OnOnGDPRStateChangeEvent;
        }

        private IEnumerator AdjustIdWhenReady()
        {
            if (_adjustAdapter == null) yield break;

            yield return new WaitUntil(() => !string.IsNullOrEmpty(ElephantCore.Instance.adjustId));
            MaxSdk.SetUserId(ElephantCore.Instance.userId + "|" + ElephantCore.Instance.adjustId);
        }

        private void SetSegmentTargeting()
        {
            try
            {
                var segmentConfig = ElephantCore.Instance.GetOpenResponse().segment_config;
                if (segmentConfig?.segments == null || segmentConfig.segments.Count == 0)
                {
                    ElephantLog.Log("MEDIATION", "Invalid segment configuration");
                    return;
                }

                var ltvManager = LtvManager.GetInstance();
                if (ltvManager == null) return;

                var metaDataUtils = MetaDataUtils.GetInstance();
                if (metaDataUtils == null) return;

                var context = new Dictionary<string, object>
                {
                    { "isBuyer", ltvManager.IsBuyer },
                    { "level", metaDataUtils.GetCurrentLevel() },
                    { "totalInterstitialCount", metaDataUtils.GetTotalInterstitialCount() },
                    { "totalRewardedCount", metaDataUtils.GetTotalRewardedCount() },
                    { "timespend", GetTimeSpentInMinutes() },
                    { "ltv", ltvManager.LifeTimeRevenue }
                };

                var expressionParser = new ExpressionParser();
                var collectionBuilder = MaxSegmentCollection.Builder();
                var anySegmentsAdded = false;

                foreach (var segmentGroup in segmentConfig.segments)
                {
                    if (segmentGroup.categories == null || segmentGroup.categories.Count == 0)
                        continue;

                    var matchedCategoryIds = new List<int>();

                    foreach (var category in segmentGroup.categories)
                    {
                        if (!string.IsNullOrEmpty(category.condition) &&
                            !expressionParser.EvaluateExpression(context, category.condition)) continue;
                        matchedCategoryIds.Add(category.categoryId);
                        ElephantLog.Log("MEDIATION",
                            $"User matched category '{category.name}' (ID: {category.categoryId}) in segment '{segmentGroup.name}'");
                    }

                    if (matchedCategoryIds.Count <= 0) continue;
                    collectionBuilder.AddSegment(new MaxSegment(segmentGroup.segmentId, matchedCategoryIds));
                    ElephantLog.Log("MEDIATION",
                        $"Added segment '{segmentGroup.name}' (ID: {segmentGroup.segmentId}) with categories: {string.Join(", ", matchedCategoryIds)}");
                    anySegmentsAdded = true;
                }

                if (anySegmentsAdded)
                {
                    MaxSdk.SetSegmentCollection(collectionBuilder.Build());
                    ElephantLog.Log("MEDIATION", "Successfully set segment collection");
                }
                else
                {
                    ElephantLog.Log("MEDIATION", "No matching categories found for user in any segment group");
                }
            }
            catch (Exception e)
            {
                ElephantLog.LogError("MEDIATION", $"Error in segment targeting: {e.Message}");
            }
        }

        private long GetTimeSpentInMinutes()
        {
            try
            {
                var totalTimeSpend = ElephantSDK.Utils.ReadFromFile(ElephantConstants.TimeSpend);
                return string.IsNullOrEmpty(totalTimeSpend) ? 0 : Convert.ToInt64(totalTimeSpend) / 1000 / 60;
            }
            catch
            {
                return 0;
            }
        }

        #region AdEvents

        #region InitEvents

        private void OnSdkInitializedEvent(MaxSdkBase.SdkConfiguration sdkConfiguration)
        {
            // TODO Refactor targeting method
            // MaxSdk.TargetingData.Keywords = new [] {
            //     "campaign_name:" + ElephantCore.Instance.GetOpenResponse().campaign_name,
            //     "segment:" + ElephantCore.Instance.GetOpenResponse().segment_name,
            //     "custom_keyword:" + RemoteConfig.GetInstance().Get("custom_keyword_for_targeting", "")
            // };

#if ELEPHANT_APP_OPEN_INTERSTITIAL
            if (appOpenAdActive && !_isNoAdsPurchased)
            {
                MaxSdkCallbacks.AppOpen.OnAdHiddenEvent += OnAppOpenDismissedEvent;
                if (IsAppOpenAdsAvailable(true))
                {
                    if (appOpenUseInterstitialWhenRequestAppOpen)
                    {
                        StartCoroutine(requestAppOpenInterstitialWithDelay());
                    }
                    else
                    {
                        if (MaxSdk.IsAppOpenAdReady(appOpenAdUnitId))
                        {
                            MaxSdk.ShowAppOpenAd(appOpenAdUnitId);
                        }
                        else
                        {
                            MaxSdk.LoadAppOpenAd(appOpenAdUnitId);
                        }

                        MaxSdkCallbacks.AppOpen.OnAdLoadedEvent += ShowFirstOpenAd;
                    }
                }

                if (!appOpenUseInterstitialWhenRequestAppOpen)
                {
                    MaxSdkCallbacks.AppOpen.OnAdRevenuePaidEvent += (adUnitId, adInfo) =>
                        OnImpressionTrackedEvent(adUnitId, adInfo, RLAdFormat.Interstitial);
                }
            }

#endif

#if ELEPHANT_AUDIOMOBS
            if (audioMobActive && !_isNoAdsPurchased)
            {
                if (AudiomobPlugin.Instance != null && !AudioMobPluginInitialized)
                {
                    AudiomobPlugin.Init(isSuccess =>
                    {
                        if (isSuccess)
                        {
                            AudioMobPluginInitialized = true;
                            
                            if (_audioMobShowTestAdsActive)
                            {
                                AudiomobPlugin.Instance.ForceTestAds = true;
                            }

                            AudiomobPlugin.Instance.SendConsentStrings =
 ElephantCore.Instance.consentStatus == "Authorized";
                            AudiomobPlugin.Instance.OnAdPlaybackStatusChanged += OnAudioMobAdPlaybackStatusChanged;
                            AudiomobPlugin.Instance.OnAdFailed += OnAudioMobAdFailedEvent;
                        }
                        else
                        {
                            AudioMobLog("Failed to Initialize AudioMobPlugin");
                        }
                    });

                    AudiomobPlugin.Instance.OnAdPaid += OnAudiomobPaidEvent;
                }
            }

#endif


            if (_isBidFloorEnabled)
            {
                if (IsIntBidFloorAdUnitIdsOk())
                {
                    MaxSdk.SetInterstitialExtraParameter(_interstitialHighAdUnit, "disable_auto_retries",
                        "true");
                    MaxSdk.SetInterstitialExtraParameter(_interstitialMidAdUnit, "disable_auto_retries",
                        "true");
                    MaxSdk.SetInterstitialExtraParameter(_interstitialNormalAdUnit, "disable_auto_retries",
                        "true");

                    try
                    {
                        _interstitialAdUnitIdStatus.Add(_interstitialHighAdUnit, false);
                        _interstitialAdUnitIdStatus.Add(_interstitialMidAdUnit, false);
                        _interstitialAdUnitIdStatus.Add(_interstitialNormalAdUnit, false);
                    }
                    catch (Exception e)
                    {
                        ElephantLog.LogError("RLADS-EVENTS", "Error adding interstitial ad unit IDs: " + e.Message);
                    }
                }

                if (IsRwBidFloorAdUnitIdsOk())
                {
                    MaxSdk.SetRewardedAdExtraParameter(_rewardedVideoHighAdUnit, "disable_auto_retries",
                        "true");
                    MaxSdk.SetRewardedAdExtraParameter(_rewardedVideoMidAdUnit, "disable_auto_retries",
                        "true");
                    MaxSdk.SetRewardedAdExtraParameter(_rewardedVideoNormalAdUnit, "disable_auto_retries",
                        "true");

                    try
                    {
                        _rewardedAdUnitIdStatus.Add(_rewardedVideoHighAdUnit, false);
                        _rewardedAdUnitIdStatus.Add(_rewardedVideoMidAdUnit, false);
                        _rewardedAdUnitIdStatus.Add(_rewardedVideoNormalAdUnit, false);
                    }
                    catch (Exception e)
                    {
                        ElephantLog.LogError("RLADS-EVENTS", "Error adding rewarded ad unit IDs: " + e.Message);
                    }
                }
            }

            StartCoroutine(LoadAdsAfterInitialization());
            this._isMediationInitialized = true;
            this._mediationInitializeTime = ElephantSDK.Utils.Timestamp();

            StartCoroutine(SendSdkInitEvents());

            if (RemoteConfig.GetInstance().GetBool("max_mediation_debugger_enabled", false))
            {
                MaxSdk.ShowMediationDebugger();
            }
        }

        IEnumerator LoadAdsAfterInitialization()
        {
            yield return new WaitForSecondsRealtime(1.0f);
            if (_isBidFloorEnabled)
            {
                ElephantLog.Log("Bidfloor test: ", "First Request - high");
                RequestInterstitial(_isBidFloorIntEnabled
                    ? _interstitialHighAdUnit
                    : _interstitialAdUnit);

                yield return new WaitForSecondsRealtime(1.0f);
                RequestRewardedAd(_isBidFloorRwEnabled
                    ? _rewardedVideoHighAdUnit
                    : _rewardedVideoAdUnit);
            }
            else
            {
                RequestInterstitial(_interstitialAdUnit);
                yield return new WaitForSecondsRealtime(1.0f);
                RequestRewardedAd(_rewardedVideoAdUnit);
            }

            if (_autoShowEnabled && !_isAdFreeDay)
            {
                if (RemoteConfig.GetInstance().GetBool("gamekit_banner_enabled", false))
                {
                    InitBannerTimer();
                } 
            
                if (RemoteConfig.GetInstance().GetBool("gamekit_interstitial_enabled", true))
                {
                    InitAutoShowInterstitials();
                }
            }

            SetBannerBackground("#ffffff");
        }

        private IEnumerator SendSdkInitEvents()
        {
            var adIdTimeout = RemoteConfig.GetInstance().GetFloat("adjust_adid_timeout", 1f);
            if (ElephantCore.Instance != null && _adjustAdapter != null)
            {
                var adjustId = "";
                _adjustAdapter.GetAdid(adId =>
                {
                    adjustId = adId;
                    ElephantCore.Instance.adjustId = adId;
                });

                var timeWaited = 0f;
                while (_adjustAdapter != null && string.IsNullOrEmpty(adjustId) && timeWaited < adIdTimeout)
                {
                    yield return null;
                    timeWaited += Time.unscaledDeltaTime;
                }

                if (timeWaited >= adIdTimeout)
                {
                    ElephantLog.LogError("Adjust", "ADID fetch timed out after " + adIdTimeout + " seconds");
                }
            }

            Elephant.Event("OnSdkInitializedEvent", -1, null);
            var evnt = OnRollicAdsSdkInitializedEvent;
            evnt?.Invoke();
        }

        #endregion

        #region InterstitialEvents

        private void OnInterstitialLoadedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
#if ELEPHANT_APP_OPEN_INTERSTITIAL
            if (adUnitId == appOpenInterstitialAdUnitId)
            {
                return;
            }
#endif

            ElephantLog.Log("RLADS-EVENTS", "OnInterstitialLoadedEvent: " + adInfo);

            if (!string.Equals(adUnitId, _interstitialAdUnit))
            {
                ElephantLog.Log("Bidfloor test: ", "OnInterstitialLoadedEvent ad unit id : " + adUnitId);
                _interstitialAdUnitIdStatus[adUnitId] = true;
                if (string.Equals(adUnitId, _interstitialHighAdUnit))
                {
                    ElephantLog.Log("Bidfloor test: ", "OnInterstitialLoadedEvent high.. request mid");
                    RequestInterstitial(_interstitialMidAdUnit);
                }
                else if (string.Equals(adUnitId, _interstitialMidAdUnit))
                {
                    ElephantLog.Log("Bidfloor test: ", "OnInterstitialLoadedEvent mid.. request norm");
                    RequestInterstitial(_interstitialNormalAdUnit);
                }
            }
            else
            {
                _isInterstitialReady = true;
            }

            interstitialRequestTimerIndex = 0;

            var ilrd = Ilrd.CreateIlrd(adInfo, RLAdFormat.Interstitial, interstitialCycleId);
            Elephant.AdEventV2("OnInterstitialLoadedEvent", Ilrd.ConvertToJson(ilrd));

            _zyngaPublishingAdapter?.LogAdLoadedEvent(
                _rollicInterstitialAd.adUuid,
                adInfo.CreativeIdentifier,
                adInfo.LatencyMillis);
            _zyngaPublishingAdapter?.LogAdLoadedDetailsEvent(
                adInfo.AdUnitIdentifier,
                _rollicInterstitialAd.adUuid,
                adInfo.NetworkName,
                ZPAdFormat.Interstitial,
                adInfo.Revenue);

            _rollicInterstitialAd = RollicInterstitialAd.VideoAdReady(_rollicInterstitialAd, ilrd);

            var evnt = OnRollicAdsInterstitialLoadedEvent;
            evnt?.Invoke(adInfo.NetworkName);

            StartCoroutine(Elephant.ResetSoundWithDelay());
        }

        private void OnInterstitialFailedEvent(string adUnitId, MaxSdkBase.ErrorInfo errorInfo)
        {
#if ELEPHANT_APP_OPEN_INTERSTITIAL
            if (adUnitId == appOpenInterstitialAdUnitId)
            {
                return;
            }
#endif

            ElephantLog.Log("RLADS-EVENTS", "OnInterstitialFailedEvent: " + errorInfo);

            if (!string.Equals(adUnitId, _interstitialAdUnit))
            {
                ElephantLog.Log("Bidfloor test: ", "OnInterstitialFailedEvent ad unit id : " + adUnitId);
                _interstitialAdUnitIdStatus[adUnitId] = false;
                StartCoroutine(RequestInterstitialAgain(adUnitId));
            }
            else
            {
                _isInterstitialReady = false;
                StartCoroutine(RequestInterstitialAgain());
            }

            _rollicInterstitialAd = RollicInterstitialAd.VideoFailedToLoad(_rollicInterstitialAd);

            var ilrd = Ilrd.CreateError(errorInfo, adUnitId, RLAdFormat.Interstitial, interstitialCycleId);
            var errorJson = Ilrd.ConvertToJson(ilrd);
            Elephant.AdEventV2("OnInterstitialFailedEvent", errorJson);

            _zyngaPublishingAdapter?.LogAdLoadFailedEvent(
                _rollicInterstitialAd.adUuid,
                errorInfo.LatencyMillis,
                errorInfo.Code.ToString(),
                errorInfo.Message);

            IronSourceError error = new IronSourceError(errorInfo.MediatedNetworkErrorCode, errorInfo.Message);
            var evnt = OnRollicAdsInterstitialFailedEvent;
            evnt?.Invoke(error);
        }

        private void OnInterstitialFailedToShowEvent(string adUnitId, MaxSdkBase.ErrorInfo errorInfo,
            MaxSdkBase.AdInfo adInfo)
        {
#if ELEPHANT_APP_OPEN_INTERSTITIAL
            if (adUnitId == appOpenInterstitialAdUnitId)
            {
                return;
            }
#endif

            ElephantLog.Log("RLADS-EVENTS", "OnInterstitialFailedToShowEvent: " + errorInfo);

            if (!string.Equals(adUnitId, _interstitialAdUnit))
            {
                ElephantLog.Log("Bidfloor test: ", "OnInterstitialFailedToShowEvent ad unit id : " + adUnitId);
                _interstitialAdUnitIdStatus[adUnitId] = false;
                if (string.Equals(adUnitId, _interstitialHighAdUnit))
                {
                    RequestInterstitial(_interstitialMidAdUnit);
                }
                else
                {
                    RequestInterstitial(_interstitialHighAdUnit);
                }
            }
            else
            {
                _isInterstitialReady = false;
                RequestInterstitial(_interstitialAdUnit);
            }

            RollicInterstitialAd.VideoFailedToPlay(_rollicInterstitialAd);

            var ilrd = Ilrd.CreateError(errorInfo, adUnitId, RLAdFormat.Interstitial, interstitialCycleId);
            var errorJson = Ilrd.ConvertToJson(ilrd);
            Elephant.AdEventV2("OnInterstitialFailedToShowEvent", Ilrd.ConvertToJson(errorJson));

            _zyngaPublishingAdapter?.LogAdFailedEvent(
                _rollicInterstitialAd.adUuid,
                adInfo.CreativeIdentifier,
                adInfo.LatencyMillis,
                errorInfo.Code.ToString(),
                errorInfo.Message);

            var evnt = OnRollicAdsInterstitialAdShowFailedEvent;
            evnt?.Invoke();
        }

        private void OnInterstitialShownEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
#if ELEPHANT_APP_OPEN_INTERSTITIAL
            if (adUnitId == appOpenInterstitialAdUnitId)
            {
                return;
            }
#endif

            ElephantLog.Log("RLADS-EVENTS", "OnInterstitialShownEvent: " + adInfo);

            if (!string.Equals(adUnitId, _interstitialAdUnit))
            {
                ElephantLog.Log("Bidfloor test: ", "OnInterstitialShownEvent ad unit id : " + adUnitId);
                _interstitialAdUnitIdStatus[adUnitId] = false;
            }
            else
            {
                _isInterstitialReady = false;
            }

            _rollicInterstitialAd = RollicInterstitialAd.VideoShown(_rollicInterstitialAd);

            var ilrd = Ilrd.CreateIlrd(adInfo, RLAdFormat.Interstitial, interstitialCycleId);
            Elephant.AdEventV2("OnInterstitialShownEvent", Ilrd.ConvertToJson(ilrd));

            if (_autoShowEnabled)
            {
                _lastTimeAdDisplayed = Time.realtimeSinceStartup;
                _lastLevelAdDisplayed = MonitoringUtils.GetInstance().GetCurrentLevel().level;
                _addedValue = 0;
            }

            var evnt = OnRollicAdsInterstitialShownEvent;
            evnt?.Invoke();
        }

        private void OnInterstitialDismissedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
#if ELEPHANT_APP_OPEN_INTERSTITIAL
            if (adUnitId == appOpenInterstitialAdUnitId)
            {
                return;
            }
#endif

            ElephantLog.Log("RLADS-EVENTS", "OnInterstitialDismissedEvent: " + adInfo);

            if (!string.Equals(adUnitId, _interstitialAdUnit))
            {
                ElephantLog.Log("Bidfloor test: ", "OnInterstitialDismissedEvent ad unit id : " + adUnitId);
                _interstitialAdUnitIdStatus[adUnitId] = false;
                RequestInterstitial(_interstitialHighAdUnit);
            }
            else
            {
                _isInterstitialReady = false;
                RequestInterstitial(_interstitialAdUnit);
            }

            RollicInterstitialAd.VideoClosed(_rollicInterstitialAd);

            var ilrd = Ilrd.CreateIlrd(adInfo, RLAdFormat.Interstitial, interstitialCycleId);
            Elephant.AdEventV2("OnInterstitialDismissedEvent", Ilrd.ConvertToJson(ilrd));

            var evnt = OnRollicAdsInterstitialDismissedEvent;
            evnt?.Invoke();

            RollicEventUtils.GetInstance().SendInterstitialEvents();

            StartCoroutine(Elephant.ResetSoundWithDelay());
        }

        private void OnInterstitialClickedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
#if ELEPHANT_APP_OPEN_INTERSTITIAL
            if (adUnitId == appOpenInterstitialAdUnitId)
            {
                return;
            }
#endif

            var ilrd = Ilrd.CreateIlrd(adInfo, RLAdFormat.Interstitial, interstitialCycleId);
            Elephant.AdEventV2("OnInterstitialClickedEvent", Ilrd.ConvertToJson(ilrd));

            _zyngaPublishingAdapter?.LogAdClickEvent(
                _rollicInterstitialAd.adUuid,
                adInfo.CreativeIdentifier,
                adInfo.LatencyMillis);

            var evnt = OnRollicAdsInterstitialClickedEvent;
            evnt?.Invoke();
        }

        #endregion

        #region RewardedEvents

        private void OnRewardedVideoLoadedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            ElephantLog.Log("RLADS-EVENTS", "OnRewardedVideoLoadedEvent: " + adInfo);
            if (!string.Equals(adUnitId, _rewardedVideoAdUnit))
            {
                ElephantLog.Log("Bidfloor test: ", "OnRewardedVideoLoadedEvent ad unit id : " + adUnitId);
                _rewardedAdUnitIdStatus[adUnitId] = true;
                if (string.Equals(adUnitId, _rewardedVideoHighAdUnit))
                {
                    ElephantLog.Log("Bidfloor test: ", "OnRewardedVideoLoadedEvent high.. request mid");
                    RequestRewardedAd(_rewardedVideoMidAdUnit);
                }
                else if (string.Equals(adUnitId, _rewardedVideoMidAdUnit))
                {
                    ElephantLog.Log("Bidfloor test: ", "OnRewardedVideoLoadedEvent mid.. request norm");
                    RequestRewardedAd(_rewardedVideoNormalAdUnit);
                }
            }

            rewardedRequestTimerIndex = 0;

            var ilrd = Ilrd.CreateIlrd(adInfo, RLAdFormat.RewardedAd, rewardedAdCycleId);
            Elephant.AdEventV2("OnRewardedVideoLoadedEvent", Ilrd.ConvertToJson(ilrd));

            _zyngaPublishingAdapter?.LogAdLoadedEvent(
                _rollicRewardedAd.adUuid,
                adInfo.CreativeIdentifier,
                adInfo.LatencyMillis);
            _zyngaPublishingAdapter?.LogAdLoadedDetailsEvent(
                adInfo.AdUnitIdentifier,
                _rollicRewardedAd.adUuid,
                adInfo.NetworkName,
                ZPAdFormat.Rewarded,
                adInfo.Revenue);

            var evnt = OnRollicAdsRewardedVideoLoadedEvent;
            evnt?.Invoke(adInfo.NetworkName);

            StartCoroutine(Elephant.ResetSoundWithDelay());
        }

        private void OnRewardedVideoFailedEvent(string adUnitId, MaxSdkBase.ErrorInfo errorInfo)
        {
            ElephantLog.Log("RLADS-EVENTS", "OnRewardedVideoFailedEvent: " + errorInfo);
            if (!string.Equals(adUnitId, _rewardedVideoAdUnit))
            {
                ElephantLog.Log("Bidfloor test: ", "OnRewardedVideoFailedEvent ad unit id : " + adUnitId);
                _rewardedAdUnitIdStatus[adUnitId] = false;
                StartCoroutine(RequestRewardedAgain(adUnitId));
            }
            else
            {
                StartCoroutine(RequestRewardedAgain());
            }

            var ilrd = Ilrd.CreateError(errorInfo, adUnitId, RLAdFormat.RewardedAd, rewardedAdCycleId);
            var errorJson = Ilrd.ConvertToJson(ilrd);
            Elephant.AdEventV2("OnRewardedVideoFailedEvent", Ilrd.ConvertToJson(errorJson));

            _zyngaPublishingAdapter?.LogAdLoadFailedEvent(
                _rollicRewardedAd.adUuid,
                errorInfo.LatencyMillis,
                errorInfo.Code.ToString(),
                errorInfo.Message);

            RollicRewardedAd.VideoFailedToPlay(_rollicRewardedAd);
            _rollicRewardedAd = RollicRewardedAd.RefreshAd();

            var evnt = OnRollicAdsRewardedVideoFailedEvent;
            evnt?.Invoke(adUnitId, errorInfo.Message);
        }

        private void OnRewardedVideoShownEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            ElephantLog.Log("RLADS-EVENTS", "OnRewardedVideoShownEvent: " + adInfo);
            if (!string.Equals(adUnitId, _rewardedVideoAdUnit))
            {
                ElephantLog.Log("Bidfloor test: ", "OnRewardedVideoShownEvent ad unit id : " + adUnitId);
                _rewardedAdUnitIdStatus[adUnitId] = false;
            }
            else
            {
                _isRewardAvailable = false;
            }

            var ilrd = Ilrd.CreateIlrd(adInfo, RLAdFormat.RewardedAd, rewardedAdCycleId);
            Elephant.AdEventV2("OnRewardedVideoShownEvent", Ilrd.ConvertToJson(ilrd));

            _rollicRewardedAd = RollicRewardedAd.VideoShown(_rollicRewardedAd, ilrd);

            var evnt = OnRollicAdsRewardedVideoShownEvent;
            evnt?.Invoke();
        }

        private void OnRewardedVideoClickedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            var ilrd = Ilrd.CreateIlrd(adInfo, RLAdFormat.RewardedAd, rewardedAdCycleId);
            Elephant.AdEventV2("OnRewardedVideoClickedEvent", Ilrd.ConvertToJson(ilrd));

            _zyngaPublishingAdapter?.LogAdClickEvent(
                _rollicRewardedAd.adUuid,
                adInfo.CreativeIdentifier,
                adInfo.LatencyMillis);

            var evnt = OnRollicAdsRewardedVideoClickedEvent;
            evnt?.Invoke();
        }

        private void OnRewardedVideoClosedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            ElephantLog.Log("RLADS-EVENTS", "OnRewardedVideoClosedEvent: " + adInfo);

            if (!string.Equals(adUnitId, _rewardedVideoAdUnit))
            {
                ElephantLog.Log("Bidfloor test: ", "OnRewardedVideoClosedEvent ad unit id : " + adUnitId);
                _rewardedAdUnitIdStatus[adUnitId] = false;
                RequestRewardedAd(_rewardedVideoHighAdUnit);
            }
            else
            {
                RequestRewardedAd(adUnitId);
            }

            CheckReward();

            var ilrd = Ilrd.CreateIlrd(adInfo, RLAdFormat.RewardedAd, rewardedAdCycleId);
            Elephant.AdEventV2("OnRewardedVideoClosedEvent", Ilrd.ConvertToJson(ilrd));

            var evnt = OnRollicAdsRewardedVideoClosedEvent;
            evnt?.Invoke();

            StartCoroutine(Elephant.ResetSoundWithDelay());
        }

        private void OnRewardedVideoFailedToPlayEvent(string adUnitId, MaxSdkBase.ErrorInfo errorInfo,
            MaxSdkBase.AdInfo adInfo)
        {
            ElephantLog.Log("RLADS-EVENTS", "OnRewardedVideoFailedToPlayEvent: " + errorInfo);
            if (!string.Equals(adUnitId, _rewardedVideoAdUnit))
            {
                ElephantLog.Log("Bidfloor test: ", "OnRewardedVideoFailedToPlayEvent ad unit id : " + adUnitId);
                _rewardedAdUnitIdStatus[adUnitId] = false;
                if (string.Equals(adUnitId, _rewardedVideoHighAdUnit))
                {
                    RequestRewardedAd(_rewardedVideoMidAdUnit);
                }
                else
                {
                    RequestRewardedAd(_rewardedVideoHighAdUnit);
                }
            }
            else
            {
                RequestRewardedAd(adUnitId);
            }

            rewardedAdResultCallback?.Invoke(RLRewardedAdResult.Failed);

            var ilrd = Ilrd.CreateError(errorInfo, adUnitId, RLAdFormat.RewardedAd, rewardedAdCycleId);
            var errorJson = Ilrd.ConvertToJson(ilrd);
            Elephant.AdEventV2("OnRewardedVideoFailedToPlayEvent", Ilrd.ConvertToJson(errorJson));

            _zyngaPublishingAdapter?.LogAdFailedEvent(
                _rollicRewardedAd.adUuid,
                adInfo.CreativeIdentifier,
                adInfo.LatencyMillis,
                errorInfo.Code.ToString(),
                errorInfo.Message);

            var evnt = OnRollicAdsRewardedVideoFailedToPlayEvent;
            evnt?.Invoke();
        }

        private void OnRewardedVideoReceivedRewardEvent(string adUnitId, MaxSdkBase.Reward reward,
            MaxSdkBase.AdInfo adInfo)
        {
            ElephantLog.Log("RLADS-EVENTS", "OnRewardedVideoReceivedRewardEvent: " + adInfo);
            _isRewardAvailable = true;

            var ilrd = Ilrd.CreateIlrd(adInfo, RLAdFormat.RewardedAd, rewardedAdCycleId);
            Elephant.AdEventV2("OnRewardedVideoReceivedRewardEvent", Ilrd.ConvertToJson(ilrd));

            var evnt = OnRollicAdsRewardedVideoReceivedRewardEvent;
            evnt?.Invoke(adInfo.NetworkName);

            RollicEventUtils.GetInstance().SendRewardedEvents();
        }

        #endregion

        #region BannerEvents

        private void OnAdLoadedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            ElephantLog.Log("RLADS-EVENTS", "OnAdLoadedEvent: " + adInfo);
            bannerRequestTimerIndex = 0;

            var ilrd = Ilrd.CreateIlrd(adInfo, RLAdFormat.Banner, bannerAdCycleId);
            Elephant.AdEventV2("OnAdLoadedEvent", Ilrd.ConvertToJson(ilrd));

            _zyngaPublishingAdapter?.LogAdLoadedEvent(
                null,
                adInfo.CreativeIdentifier,
                adInfo.LatencyMillis);
            _zyngaPublishingAdapter?.LogAdLoadedDetailsEvent(
                adInfo.AdUnitIdentifier,
                null,
                adInfo.NetworkName,
                ZPAdFormat.Banner,
                adInfo.Revenue);

            var evnt = OnRollicAdsAdLoadedEvent;
            evnt?.Invoke();

            if (_isBannerAutoShowEnabled)
            {
                showBanner();
            }

            StartCoroutine(Elephant.ResetSoundWithDelay());
        }

        private void OnAdFailedEvent(string adUnitId, MaxSdkBase.ErrorInfo errorInfo)
        {
            ElephantLog.Log("RLADS-EVENTS", "OnAdFailedEvent: " + errorInfo);
            StartCoroutine(RequestBannerAgain());

            var ilrd = Ilrd.CreateError(errorInfo, adUnitId, RLAdFormat.Banner, bannerAdCycleId);
            var errorJson = Ilrd.ConvertToJson(ilrd);
            Elephant.AdEventV2("OnAdFailedEvent", Ilrd.ConvertToJson(errorJson));

            _zyngaPublishingAdapter?.LogAdLoadFailedEvent(
                null,
                errorInfo.LatencyMillis,
                errorInfo.Code.ToString(),
                errorInfo.Message);

            var evnt = OnRollicAdsAdFailedEvent;
            evnt?.Invoke(errorInfo.Message);
        }

        private void OnAdClickedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            var ilrd = Ilrd.CreateIlrd(adInfo, RLAdFormat.Banner, bannerAdCycleId);
            Elephant.AdEventV2("OnAdClickedEvent", Ilrd.ConvertToJson(ilrd));

            _zyngaPublishingAdapter?.LogAdClickEvent(
                null,
                adInfo.CreativeIdentifier,
                adInfo.LatencyMillis);

            var evnt = OnRollicAdsAdClickedEvent;
            evnt?.Invoke();
        }

        private void OnAdCollapsedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            var ilrd = Ilrd.CreateIlrd(adInfo, RLAdFormat.Banner, bannerAdCycleId);
            Elephant.AdEventV2("OnAdCollapsedEvent", Ilrd.ConvertToJson(ilrd));

            var evnt = OnRollicAdsAdCollapsedEvent;
            evnt?.Invoke(adInfo.NetworkName);
        }

        private void OnAdExpandedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            var ilrd = Ilrd.CreateIlrd(adInfo, RLAdFormat.Banner, bannerAdCycleId);
            Elephant.AdEventV2("OnAdExpandedEvent", Ilrd.ConvertToJson(ilrd));

            var evnt = OnRollicAdsAdExpandedEvent;
            evnt?.Invoke(adInfo.NetworkName);
        }

        #endregion

        #region ILRDEvents

        private void InitFirebase()
        {
            if (_firebaseAdapter == null) return;

            _firebaseAdapter.InitializeFirebase(() =>
            {
                ElephantLog.Log("FIREBASE", "Firebase initialized (via adapter)");

                OnFirebaseInitialized?.Invoke();

                _firebaseAdapter.SetAnalyticsCollectionEnabled(true);

                _firebaseAdapter.GetAnalyticsInstanceId(instanceId =>
                {
                    if (!string.IsNullOrEmpty(instanceId))
                    {
                        var param = Params.New().Set("firebaseId", instanceId);
                        Elephant.Event("elephant_firebase_user_id", -1, param);
                        ElephantLog.Log("FIREBASE", "Firebase Analytics Instance ID: " + instanceId);
                    }
                });

                if (ElephantCore.Instance == null) return;

                if (!string.IsNullOrEmpty(ElephantCore.Instance.userId))
                {
                    _firebaseAdapter.SetCrashlyticsUserId(ElephantCore.Instance.userId);
                }

                MaxVersionReporter.ReportVersionsToCrashlytics();

                var remoteConfig = RemoteConfig.GetInstance();
                if (remoteConfig != null && !string.IsNullOrEmpty(remoteConfig.GetTag()))
                {
                    ElephantLog.LogCustomKey("user_tag", remoteConfig.GetTag());
                }

                var usercentricsElephantAdapter = ElephantCore.Instance.UsercentricsElephantAdapter;
                if (usercentricsElephantAdapter == null)
                {
                    ElephantLog.LogError("USERCENTRICS-ELEPHANT", "UsercentricsElephantAdapter IS NULL");
                    return;
                }

                var isConsentModeV2Enabled = remoteConfig.GetBool("consent_mode_v2_enabled", false);
                if (usercentricsElephantAdapter.DidAnalyticsConsentSet() && isConsentModeV2Enabled)
                {
                    var analyticsConsentStatus = usercentricsElephantAdapter.GetAnalyticsConsentStatus();
                    _firebaseAdapter.SetAnalyticsConsent(analyticsConsentStatus);
                }

                if (usercentricsElephantAdapter.DidCrashlyticsConsentSet())
                {
                    _firebaseAdapter.SetCrashlyticsCollectionEnabled(usercentricsElephantAdapter
                        .GetCrashlyticsConsentStatus());
                }
            });
        }

        private void OnImpressionTrackedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo, string adFormat)
        {
#if UNITY_ANDROID
            ExecuteOnMainThread(() => { LogRevenue(adInfo, adFormat); });
#else
            LogRevenue(adInfo, adFormat);
#endif
        }

        private void LogRevenue(MaxSdkBase.AdInfo adInfo, string adFormat)
        {
            if (_adjustAdapter == null) return;

            var extraParams = new Dictionary<string, string>
            {
                { "ad_format", adFormat },
                { "network_placement", adInfo.NetworkPlacement },
                { "creative_id", adInfo.CreativeIdentifier }
            };

            if (string.Equals(adFormat, RLAdFormat.RewardedAd))
            {
                _rollicRewardedAd = RollicRewardedAd.Impression(_rollicRewardedAd);
                extraParams.Add("ad_uuid", _rollicRewardedAd.adUuid);
                extraParams.Add("ad_placement_category", _rollicRewardedAd._category);
                extraParams.Add("ad_placement_source", _rollicRewardedAd._source);
                extraParams.Add("ad_placement_item", _rollicRewardedAd._item);
            }
            else if (string.Equals(adFormat, RLAdFormat.Interstitial))
            {
                _rollicInterstitialAd = RollicInterstitialAd.Impression(_rollicInterstitialAd);
                extraParams.Add("ad_uuid", _rollicInterstitialAd.adUuid);
                extraParams.Add("ad_placement_source", _rollicInterstitialAd._source);
            }

            _adjustAdapter.TrackAdRevenue(
                _adjustAdapter.AppLovinMAXRevenueSource,
                adInfo.Revenue,
                "USD",
                adInfo.NetworkName,
                adInfo.AdUnitIdentifier,
                adInfo.Placement,
                adInfo.AdFormat,
                adInfo.AdUnitIdentifier,
                extraParams
            );

            _revenueForwardingUtils.UpdateRevenue(adInfo);
            _ltvManager?.UpdateRevenue((float)adInfo.Revenue);

            var impressionParameters = new Dictionary<string, object>
            {
                { "ad_platform", "applovin_max" },
                { "ad_source", adInfo.NetworkName },
                { "ad_unit_name", adInfo.AdUnitIdentifier },
                { "ad_format", adFormat },
                { "value", adInfo.Revenue },
                { "currency", "USD" },
            };

            _firebaseAdapter?.LogEvent("ad_impression", impressionParameters);
            _firebaseAdapter?.LogEvent("custom_ad_impression", impressionParameters);

            var fbAdParams = new Dictionary<string, object>
            {
                [AppEventParameterName.Currency] = "USD"
            };
            _facebookAdapter?.LogAppEvent("AdImpression", (float)adInfo.Revenue, fbAdParams);

            switch (adFormat)
            {
                case RLAdFormat.Banner:
                    _zyngaPublishingAdapter?.LogAdImpressionEvent(
                        adInfo.AdUnitIdentifier,
                        null,
                        ZPAdFormat.Banner,
                        adInfo.NetworkName,
                        adInfo.NetworkPlacement,
                        adInfo.Revenue,
                        adInfo.RevenuePrecision);
                    break;
                case RLAdFormat.Interstitial:
                    _zyngaPublishingAdapter?.LogAdImpressionEvent(
                        adInfo.AdUnitIdentifier,
                        _rollicInterstitialAd.adUuid,
                        ZPAdFormat.Interstitial,
                        adInfo.NetworkName,
                        adInfo.NetworkPlacement,
                        adInfo.Revenue,
                        adInfo.RevenuePrecision);
                    break;
                case RLAdFormat.RewardedAd:
                    _zyngaPublishingAdapter?.LogAdImpressionEvent(
                        adInfo.AdUnitIdentifier,
                        _rollicRewardedAd.adUuid,
                        ZPAdFormat.Rewarded,
                        adInfo.NetworkName,
                        adInfo.NetworkPlacement,
                        adInfo.Revenue,
                        adInfo.RevenuePrecision);
                    break;
            }
        }

        #endregion

#if ELEPHANT_APP_OPEN_INTERSTITIAL
        #region APP OPEN VARIABLES

        public static event Action<bool> NoAdsStatusUpdatedEvent; 
        
        private static bool _isNoAdsPurchased
        {
            get
            {
                if (instance.appOpenAdDebuggerActive)
                {
                    instance.AppOpenLog("Add Open NoAds Purchased: " +
                                        ((PlayerPrefs.GetInt("ElephantKeyNoAdsPurchased", 0) == 1)
                                            ? "true"
                                            : "false"));
                }

                return PlayerPrefs.GetInt("ElephantKeyNoAdsPurchased", 0) == 1;
            }
            set => PlayerPrefs.SetInt("ElephantKeyNoAdsPurchased", value ? 1 : 0);
        }

        private bool appOpenAdActive = RemoteConfig.GetInstance().GetBool("app_open_ads_enabled", false);

        private bool appOpenUseInterstitialWhenRequestAppOpen = RemoteConfig.GetInstance()
            .GetBool("app_open_use_interstitial_when_request_app_open", false);

#if UNITY_IOS
        private string appOpenAdUnitId = RemoteConfig.GetInstance().Get("app_open_ad_unit_id_ios");
        private string appOpenInterstitialAdUnitId =
 RemoteConfig.GetInstance().Get("app_open_interstitial_ad_unit_id_ios", "9b0b59ab84b8bebe");

#else // UNITY_ANDROID

        private string appOpenAdUnitId = RemoteConfig.GetInstance().Get("app_open_ad_unit_id_android");

        private string appOpenInterstitialAdUnitId = RemoteConfig.GetInstance()
            .Get("app_open_interstitial_ad_unit_id_android", "9b0b59ab84b8bebe");

#endif

#if UNITY_EDITOR
        private bool appOpenAdDebuggerActive =
            RemoteConfig.GetInstance().GetBool("app_open_ads_debugger_enabled", true);
#else
        private bool appOpenAdDebuggerActive =
 RemoteConfig.GetInstance().GetBool("app_open_ads_debugger_enabled", false);
#endif

        private int appOpenFirstOpenDelayHour = RemoteConfig.GetInstance().GetInt("app_open_delay_hour", 24);

        private float appOpenAdsMinAppPauseSeconds =
            RemoteConfig.GetInstance().GetFloat("app_open_ads_app_pause_seconds", 30f);

        private bool _appOpenInterstitialRequested;
        private bool _appOpenShowInterstitialOnLoad;
        private bool _appOpenIsReturnFromInterstitialAd;
        private float _pauseStartTime;

        private void ShowFirstOpenAd(string arg1, MaxSdkBase.AdInfo arg2)
        {
            MaxSdkCallbacks.AppOpen.OnAdLoadedEvent -= ShowFirstOpenAd;

            if (MaxSdk.IsAppOpenAdReady(appOpenAdUnitId))
            {
                MaxSdk.ShowAppOpenAd(appOpenAdUnitId);
            }
            else
            {
                AppOpenLog("App Open Ad Not Ready");
            }
        }

        private IEnumerator requestAppOpenInterstitialWithDelay()
        {
            yield return new WaitForSeconds(1f);
            requestAppOpenIntersitial(true);
        }

        private void requestAppOpenIntersitial(bool showOnLoad)
        {
            if (string.IsNullOrEmpty(appOpenInterstitialAdUnitId))
            {
                AppOpenLog($"App Open Interstitial Requested failed! appOpenInterstitialAdUnitId is empty");
                return;
            }

            if (!MaxSdk.IsInitialized())
            {
                AppOpenLog($"App Open Interstitial Requested failed! MaxSdk is not initialized.");
                return;
            }

            if (_appOpenShowInterstitialOnLoad)
            {
                _appOpenShowInterstitialOnLoad = false;
                showAppOpenInterstitial();
                return;
            }

            MaxSdkCallbacks.Interstitial.OnAdLoadedEvent += appOpenInterstitialAdLoadedEvent;
            AppOpenLog($"App Open Interstitial Requested");
            _appOpenInterstitialRequested = true;
            MaxSdk.LoadInterstitial(appOpenInterstitialAdUnitId);
            _appOpenShowInterstitialOnLoad = showOnLoad;
        }

        private void appOpenInterstitialAdLoadedEvent(string adUnitId, MaxSdkBase.AdInfo arg2)
        {
            if (!_appOpenInterstitialRequested || adUnitId != appOpenInterstitialAdUnitId)
                return;

            AppOpenLog($"App Open Interstitial loaded {adUnitId}");
            _appOpenInterstitialRequested = false;
            if (_appOpenShowInterstitialOnLoad)
            {
                _appOpenShowInterstitialOnLoad = false;
                showAppOpenInterstitial();
            }
        }

        private bool isAppOpenInterstitialReady()
        {
            return MaxSdk.IsInterstitialReady(appOpenInterstitialAdUnitId);
        }

        private void showAppOpenInterstitial()
        {
            if (isAppOpenInterstitialReady())
            {
                _appOpenIsReturnFromInterstitialAd = true;
                MaxSdkCallbacks.Interstitial.OnAdLoadedEvent -= appOpenInterstitialAdLoadedEvent;
                AppOpenLog($"App Open Interstitial show {appOpenInterstitialAdUnitId}");
                MaxSdk.ShowInterstitial(appOpenInterstitialAdUnitId);
            }
        }

        public static void SetNoAdsPurchased(bool noAdsPurchased)
        {
            _isNoAdsPurchased = noAdsPurchased;
            NoAdsStatusUpdatedEvent?.Invoke(noAdsPurchased);
        }

        private void AppOpenLog(string log)
        {
            if (appOpenAdDebuggerActive)
            {
                Elephant.Event(log, -1);
#if UNITY_EDITOR
                Debug.LogWarning(log);
#endif
            }
        }

        private void OnApplicationPause(bool pauseStatus)
        {
            if (instance == null && !instance._isMediationInitialized)
            {
                return;
            }

            if (!pauseStatus)
            {
                if (appOpenAdDebuggerActive)
                {
                    var startTime = _pauseStartTime;
                    var requiredTime = _pauseStartTime + appOpenAdsMinAppPauseSeconds;
                    var currentTime = Time.realtimeSinceStartup;
                    var isTimerReady = _pauseStartTime + appOpenAdsMinAppPauseSeconds < Time.realtimeSinceStartup;
                    AppOpenLog(
                        $"App Open Ads is not paused. Request Started! startTime= {startTime} - requiredTime= {requiredTime} - currentTime= {currentTime} - isTimerReady={isTimerReady}");
                }

                if (appOpenUseInterstitialWhenRequestAppOpen)
                {
                    if (!_appOpenIsReturnFromInterstitialAd)
                    {
                        ShowAdIfReady();
                    }
                    else
                    {
                        _appOpenIsReturnFromInterstitialAd = false;
                    }
                }
                else
                {
                    ShowAdIfReady();
                }
            }
            else
            {
                _pauseStartTime = Time.realtimeSinceStartup;
                if (appOpenAdDebuggerActive)
                {
                    AppOpenLog("App Open Ads is paused");
                }
            }
        }

        private void ShowAdIfReady()
        {
            if (!IsAppOpenAdsAvailable() || _isNoAdsPurchased)
            {
                return;
            }

            if (appOpenUseInterstitialWhenRequestAppOpen)
            {
                requestAppOpenIntersitial(true);
            }
            else
            {
                if (MaxSdk.IsAppOpenAdReady(appOpenAdUnitId))
                {
                    MaxSdk.ShowAppOpenAd(appOpenAdUnitId);
                }
                else
                {
                    MaxSdk.LoadAppOpenAd(appOpenAdUnitId);
                }
            }
        }

        private void OnAppOpenDismissedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            if (!IsAppOpenAdsAvailable() || _isNoAdsPurchased)
                return;

            MaxSdk.LoadAppOpenAd(appOpenAdUnitId);
        }

        private bool IsAppOpenAdsAvailable(bool skipSecondsCheck = false)
        {
            if (!appOpenAdActive || string.IsNullOrWhiteSpace(appOpenAdUnitId))
            {
                if (appOpenAdDebuggerActive)
                {
                    AppOpenLog("App Open Ads is not available: appOpenAdActive-" + appOpenAdUnitId);
                }

                return false;
            }

            var currentTime = ElephantSDK.Utils.Timestamp();
            var requiredTime = ElephantCore.Instance.installTime + appOpenFirstOpenDelayHour * 3600000;
            if (currentTime < requiredTime)
            {
                AppOpenLog("App Open Ads is not available: appOpenFirstOpenDelayHour");
                return false;
            }

            if (skipSecondsCheck)
            {
                return true;
            }

            if (!(_pauseStartTime + appOpenAdsMinAppPauseSeconds < Time.realtimeSinceStartup))
            {
                AppOpenLog("App Open Ads is not available: appOpenAdsMinAppPauseSeconds");
            }

            return _pauseStartTime + appOpenAdsMinAppPauseSeconds < Time.realtimeSinceStartup;
        }

        #endregion

#endif

        #endregion

        #region UserConsentEvents

        private void OnOnCCPAStateChangeEvent(bool accepted)
        {
            MaxSdk.SetDoNotSell(!accepted);
        }

        private void OnOnGDPRStateChangeEvent(bool accepted)
        {
            MaxSdk.SetHasUserConsent(accepted);
        }

        #endregion

        #region Rewarded

        private void RequestRewardedAd(string adUnitId)
        {
            ElephantLog.Log("RLADS-EVENTS", "RequestRewardedAd: " + adUnitId);
            if (string.Equals(adUnitId, _rewardedVideoHighAdUnit))
            {
                rewardedAdCycleId += 1;
            }

            var ilrd = Ilrd.CreateIlrd(adUnitId, RLAdFormat.RewardedAd, rewardedAdCycleId);
            Elephant.AdEventV2("OnRewardedRequested", Ilrd.ConvertToJson(ilrd));

            _zyngaPublishingAdapter?.LogAdLoadEvent(adUnitId);
            _rollicRewardedAd = RollicRewardedAd.RefreshAd();

#if UNITY_EDITOR
            if (false)
#else
            if (isFirstRewardedVideoRequest && !string.IsNullOrEmpty(_amazonRewardedVideoSlotId))
#endif
            {
                isFirstRewardedVideoRequest = false;
                var rewardedVideoAdRequest = new APSVideoAdRequest(320, 480, _amazonRewardedVideoSlotId);
                rewardedVideoAdRequest.onSuccess += (adResponse) =>
                {
                    ElephantLog.Log("RLADS-APS", "Rewarded success: " + adResponse);
                    MaxSdk.SetRewardedAdLocalExtraParameter(adUnitId, "amazon_ad_response", adResponse.GetResponse());
                    MaxSdk.LoadRewardedAd(adUnitId);
                };
                rewardedVideoAdRequest.onFailedWithError += (adError) =>
                {
                    ElephantLog.Log("RLADS-APS", "Rewarded failed: " + adError.GetMessage());
                    MaxSdk.SetRewardedAdLocalExtraParameter(adUnitId, "amazon_ad_error", adError.GetAdError());
                    MaxSdk.LoadRewardedAd(adUnitId);
                };

                rewardedVideoAdRequest.LoadAd();
            }
            else
            {
                MaxSdk.LoadRewardedAd(adUnitId);
            }
        }

        public bool isRewardedVideoAvailable()
        {
            if (!IsMediationReady()) return false;

            if (!_isBidFloorEnabled || !_isBidFloorRwEnabled) return MaxSdk.IsRewardedAdReady(_rewardedVideoAdUnit);

            if (_rewardedAdUnitIdStatus.TryGetValue(_rewardedVideoHighAdUnit, out var highReady) && highReady)
            {
                return MaxSdk.IsRewardedAdReady(_rewardedVideoHighAdUnit);
            }

            if (_rewardedAdUnitIdStatus.TryGetValue(_rewardedVideoMidAdUnit, out var midReady) && midReady)
            {
                return MaxSdk.IsRewardedAdReady(_rewardedVideoMidAdUnit);
            }

            return MaxSdk.IsRewardedAdReady(_rewardedVideoNormalAdUnit);
        }

        public void showRewardedVideo(RollicRewardedAd.RewardedAdCategory category,
            RollicRewardedAd.RewardedAdSource source, string item)
        {
            if (!IsMediationReady()) return;

            _rollicRewardedAd = RollicRewardedAd.ShowTapped(_rollicRewardedAd, category, source, item);

            if (_isBidFloorEnabled && _isBidFloorRwEnabled)
            {
                if (MaxSdk.IsRewardedAdReady(_rewardedVideoHighAdUnit))
                {
                    ElephantLog.Log("Bidfloor test: ", "showRewardedVideo, high");
                    MaxSdk.ShowRewardedAd(_rewardedVideoHighAdUnit);
                }
                else if (MaxSdk.IsRewardedAdReady(_rewardedVideoMidAdUnit))
                {
                    ElephantLog.Log("Bidfloor test: ", "showRewardedVideo, mid");
                    MaxSdk.ShowRewardedAd(_rewardedVideoMidAdUnit);
                }
                else
                {
                    ElephantLog.Log("Bidfloor test: ", "showRewardedVideo, normal");
                    MaxSdk.ShowRewardedAd(_rewardedVideoNormalAdUnit);
                }
            }
            else
            {
                if (MaxSdk.IsRewardedAdReady(_rewardedVideoAdUnit))
                {
                    MaxSdk.ShowRewardedAd(_rewardedVideoAdUnit);
                }
            }
        }

        #endregion

        #region Banner

        public void loadBanner(bool autoShow = true)
        {
            if (!IsMediationReady()) return;
            
            if(_isAdFreeDay && _autoShowEnabled) return;
            
            StartCoroutine(loadBannerAsync());

            _isBannerAutoShowEnabled = autoShow;
        }

        private IEnumerator loadBannerAsync()
        {
            while (!this._isMediationInitialized)
                yield return null;

            long now = ElephantSDK.Utils.Timestamp();

            if ((now - this._mediationInitializeTime) <= 2000)
            {
                yield return new WaitForSecondsRealtime(2.0f);
            }

            RequestBanner();

            if (!string.IsNullOrEmpty(_bannerBackgroundColor))
            {
                if (ColorUtility.TryParseHtmlString(_bannerBackgroundColor, out var color))
                {
                    MaxSdk.SetBannerBackgroundColor(_bannerAdUnit, color);
                }
            }
        }

        private void RequestBanner()
        {
            ElephantLog.Log("RLADS-EVENTS", "RequestBanner");
            bannerAdCycleId += 1;
            var ilrd = Ilrd.CreateIlrd(_bannerAdUnit, RLAdFormat.Banner, bannerAdCycleId);
            Elephant.AdEventV2("OnBannerRequested", Ilrd.ConvertToJson(ilrd));

            _zyngaPublishingAdapter?.LogAdLoadEvent(_bannerAdUnit);

            const int width = 320;
            const int height = 50;
#if UNITY_EDITOR
            if (false)
#else
            if (!string.IsNullOrEmpty(_amazonBannerSlotId))
#endif
            {
                var bannerAdRequest = new APSBannerAdRequest(width, height, _amazonBannerSlotId);
                bannerAdRequest.onFailedWithError += (adError) =>
                {
                    ElephantLog.Log("RLADS-APS", "Banner failed: " + adError.GetMessage());
                    MaxSdk.SetBannerLocalExtraParameter(_bannerAdUnit, "amazon_ad_error", adError.GetAdError());
                    CreateMaxBannerAd();
                };
                bannerAdRequest.onSuccess += (adResponse) =>
                {
                    ElephantLog.Log("RLADS-APS", "Banner success: " + adResponse.ToString());
                    MaxSdk.SetBannerLocalExtraParameter(_bannerAdUnit, "amazon_ad_response", adResponse.GetResponse());
                    CreateMaxBannerAd();
                };

                bannerAdRequest.LoadAd();
            }
            else
            {
                CreateMaxBannerAd();
            }
        }

        private void CreateMaxBannerAd()
        {
            MaxSdk.CreateBanner(_bannerAdUnit, MaxSdkBase.BannerPosition.BottomCenter);
            MaxSdk.SetBannerPlacement(_bannerAdUnit, "MY_BANNER_PLACEMENT");
            MaxSdk.SetBannerExtraParameter(_bannerAdUnit, "adaptive_banner", _isAdaptiveBannerEnabled.ToString());
        }

        public void showBanner()
        {
            MaxSdk.ShowBanner(_bannerAdUnit);
        }

        public void hideBanner()
        {
            MaxSdk.HideBanner(_bannerAdUnit);
        }

        public void destroyBanner()
        {
            MaxSdk.DestroyBanner(_bannerAdUnit);
        }

        public void SetBannerBackground(string backgroundColor)
        {
            _bannerBackgroundColor = backgroundColor;
        }

        public void SetAdaptiveBannerEnabled(bool isAdaptiveBannerEnabled)
        {
            _isAdaptiveBannerEnabled = isAdaptiveBannerEnabled;
        }

        public float GetBannerHeight()
        {
            if (!IsMediationReady())
            {
                return -1;
            }

#if UNITY_IOS
            return RollicAdsIos.getPixelValue(MaxSdkUtils.GetAdaptiveBannerHeight());
#elif UNITY_ANDROID
            return RollicAdsAndroid.ConvertDpToPixel(MaxSdkUtils.GetAdaptiveBannerHeight());
#else
            return MaxSdkUtils.GetAdaptiveBannerHeight();
#endif
        }

        #endregion

        #region Interstitial

        private void RequestInterstitial(string adUnitId)
        {
            if(_isAdFreeDay && _autoShowEnabled) return;
            ElephantLog.Log("RLADS-EVENTS", "RequestInterstitial: " + adUnitId);
            if (string.Equals(adUnitId, _interstitialHighAdUnit))
            {
                interstitialCycleId += 1;
            }

            var ilrd = Ilrd.CreateIlrd(adUnitId, RLAdFormat.Interstitial, interstitialCycleId);

            Elephant.AdEventV2("OnInterstitialRequested", Ilrd.ConvertToJson(ilrd));

            _zyngaPublishingAdapter?.LogAdLoadEvent(adUnitId);
            _rollicInterstitialAd = RollicInterstitialAd.RefreshAd();

#if UNITY_EDITOR
            if (false)
#else
            if (isFirstVideoInterstitialRequest && !string.IsNullOrEmpty(_amazonInterstitialVideoSlotId))
#endif
            {
                isFirstVideoInterstitialRequest = false;
                var interstitialVideoAdRequest = new APSVideoAdRequest(320, 480, _amazonInterstitialVideoSlotId);
                interstitialVideoAdRequest.onSuccess += (adResponse) =>
                {
                    ElephantLog.Log("RLADS-APS", "Inter success: " + adResponse);
                    MaxSdk.SetInterstitialLocalExtraParameter(adUnitId, "amazon_ad_response", adResponse.GetResponse());
                    MaxSdk.LoadInterstitial(adUnitId);
                };
                interstitialVideoAdRequest.onFailedWithError += (adError) =>
                {
                    ElephantLog.Log("RLADS-APS", "Inter failed: " + adError.GetMessage());
                    MaxSdk.SetInterstitialLocalExtraParameter(adUnitId, "amazon_ad_error", adError.GetAdError());
                    MaxSdk.LoadInterstitial(adUnitId);
                };

                interstitialVideoAdRequest.LoadAd();
            }
            else
            {
                MaxSdk.LoadInterstitial(adUnitId);
            }
        }

        public bool isInterstitialReady()
        {
            var isReady = false;
            var attemptedAdUnitId = "";

            if (_isBidFloorEnabled && _isBidFloorIntEnabled)
            {
                if (_interstitialAdUnitIdStatus.TryGetValue(_interstitialHighAdUnit, out var highReady) && highReady)
                {
                    attemptedAdUnitId = _interstitialHighAdUnit;
                    isReady = MaxSdk.IsInterstitialReady(_interstitialHighAdUnit);
                }
                else if (_interstitialAdUnitIdStatus.TryGetValue(_interstitialMidAdUnit, out var midReady) && midReady)
                {
                    attemptedAdUnitId = _interstitialMidAdUnit;
                    isReady = MaxSdk.IsInterstitialReady(_interstitialMidAdUnit);
                }
                else if (_interstitialAdUnitIdStatus.TryGetValue(_interstitialNormalAdUnit, out var normalReady) &&
                         normalReady)
                {
                    attemptedAdUnitId = _interstitialNormalAdUnit;
                    isReady = MaxSdk.IsInterstitialReady(_interstitialNormalAdUnit);
                }

                var data = new Ilrd
                {
                    adUnitId = attemptedAdUnitId,
                    isReadyToShow = isReady,
                    cycleId = interstitialCycleId
                };

                var jsonString = JsonConvert.SerializeObject(data);
                Elephant.AdEventV2("OnInterstitialUserShowCall", jsonString);
            }
            else
            {
                isReady = _isInterstitialReady;
                var data = new Ilrd
                {
                    adUnitId = _interstitialAdUnit,
                    isReadyToShow = isReady,
                    cycleId = interstitialCycleId
                };

                var jsonString = JsonConvert.SerializeObject(data);
                Elephant.AdEventV2("OnInterstitialUserShowCall", jsonString);
            }

            return isReady;
        }

        public void showInterstitial(RollicInterstitialAd.InterstitialAdSource source)
        {
#if ELEPHANT_AUDIOMOBS
            if (_audioMobBlockInterstitialWhenAdsActive && _audioMobAdsActive)
            {
                if (_audioMobHoldInterstitialRequestUntilAudioAdsCompleted)
                {
                    AudioMobLog($"AudioMob ads is active. Interstitial Request is transferred to hold state. Source: {source}");
                    _waitingInterstitialRequestSource = source;
                    _waitingInterstitialRequest = true;
                }
                
                return;
            }

#endif


            _rollicInterstitialAd = RollicInterstitialAd.ShowCalled(_rollicInterstitialAd, source);

            if (_isBidFloorEnabled && _isBidFloorIntEnabled)
            {
                if (_interstitialAdUnitIdStatus.TryGetValue(_interstitialHighAdUnit, out var highReady) && highReady)
                {
                    ElephantLog.Log("Bidfloor test: ", "showInterstitial, high");
                    MaxSdk.ShowInterstitial(_interstitialHighAdUnit);
                }
                else if (_interstitialAdUnitIdStatus.TryGetValue(_interstitialMidAdUnit, out var midReady) && midReady)
                {
                    ElephantLog.Log("Bidfloor test: ", "showInterstitial, mid");
                    MaxSdk.ShowInterstitial(_interstitialMidAdUnit);
                }
                else
                {
                    ElephantLog.Log("Bidfloor test: ", "showInterstitial, normal");
                    MaxSdk.ShowInterstitial(_interstitialNormalAdUnit);
                }
            }
            else
            {
                MaxSdk.ShowInterstitial(_interstitialAdUnit);
            }
        }

        #endregion

        IEnumerator RequestInterstitialAgain(string adUnitId = null)
        {
            if (timers == null) throw new Exception("RLAdvertisementManager has not been initialized!");

            yield return new WaitForSecondsRealtime(timers[interstitialRequestTimerIndex]);
            if (interstitialRequestTimerIndex < timers.Count - 1)
            {
                interstitialRequestTimerIndex++;
            }
            else
            {
                interstitialRequestTimerIndex = 0;
            }

            if (adUnitId == null)
            {
                RequestInterstitial(_interstitialAdUnit);
            }
            else
            {
                if (string.Equals(adUnitId, _interstitialHighAdUnit))
                {
                    RequestInterstitial(_interstitialMidAdUnit);
                }
                else if (string.Equals(adUnitId, _interstitialMidAdUnit))
                {
                    RequestInterstitial(_interstitialNormalAdUnit);
                }
                else
                {
                    RequestInterstitial(_interstitialHighAdUnit);
                }
            }
        }

        IEnumerator RequestRewardedAgain(string adUnitId = null)
        {
            if (timers == null) throw new Exception("RLAdvertisementManager has not been initialized!");

            yield return new WaitForSecondsRealtime(timers[rewardedRequestTimerIndex]);
            if (rewardedRequestTimerIndex < timers.Count - 1)
            {
                rewardedRequestTimerIndex++;
            }
            else
            {
                rewardedRequestTimerIndex = 0;
            }

            if (adUnitId == null)
            {
                RequestRewardedAd(_rewardedVideoAdUnit);
            }
            else
            {
                if (string.Equals(adUnitId, _rewardedVideoHighAdUnit))
                {
                    RequestRewardedAd(_rewardedVideoMidAdUnit);
                }
                else if (string.Equals(adUnitId, _rewardedVideoMidAdUnit))
                {
                    RequestRewardedAd(_rewardedVideoNormalAdUnit);
                }
                else
                {
                    RequestRewardedAd(_rewardedVideoHighAdUnit);
                }
            }
        }

        IEnumerator RequestBannerAgain()
        {
            if (timers == null) throw new Exception("RLAdvertisementManager has not been initialized!");

            yield return new WaitForSecondsRealtime(timers[bannerRequestTimerIndex]);
            if (bannerRequestTimerIndex < timers.Count - 1)
            {
                bannerRequestTimerIndex++;
            }
            else
            {
                bannerRequestTimerIndex = 0;
            }

            loadBanner();
        }

        void CheckReward()
        {
            if (_isRewardAvailable)
            {
                rewardedAdResultCallback?.Invoke(RLRewardedAdResult.Finished);
            }
            else
            {
                RollicRewardedAd.VideoSkipped(_rollicRewardedAd);
                _rollicRewardedAd = RollicRewardedAd.RefreshAd();
                rewardedAdResultCallback?.Invoke(RLRewardedAdResult.Skipped);
            }
        }

        private bool IsMediationReady()
        {
            if (_isMediationInitialized)
            {
                return true;
            }

            Debug.LogError(
                "RLAdvertisementManager is not initialized properly! Please make sure that you registered OnSdkInitializedEvent event");
            return false;
        }

        private bool IsIntBidFloorAdUnitIdsOk()
        {
            return !string.IsNullOrEmpty(_interstitialHighAdUnit)
                   || !string.IsNullOrEmpty(_interstitialMidAdUnit)
                   || !string.IsNullOrEmpty(_interstitialNormalAdUnit);
        }

        private bool IsRwBidFloorAdUnitIdsOk()
        {
            return !string.IsNullOrEmpty(_rewardedVideoHighAdUnit)
                   || !string.IsNullOrEmpty(_rewardedVideoMidAdUnit)
                   || !string.IsNullOrEmpty(_rewardedVideoNormalAdUnit);
        }

        #region Auto-Show Interstitials

        private void InitAutoShowInterstitials()
        {
            ElephantLog.Log("AUTO-SHOW", "Initializing Auto-Show Interstitials");

            _addedValue = 0;
            _interstitialDisplayInterval =
                RemoteConfig.GetInstance().GetInt("gamekit_ads_interstitial_display_interval", 30);
            _firstInterDisplayTimeAfterStart = RemoteConfig.GetInstance()
                .GetInt("gamekit_ads_interstitial_first_int_display_after_start", 90);
            _firstLevelToDisplay =
                RemoteConfig.GetInstance().GetInt("gamekit_ads_interstitial_first_level_to_display", 1);
            _levelFrequency = RemoteConfig.GetInstance().GetInt("gamekit_ads_interstitial_level_frequency", 2);
            _interShowLogic = RemoteConfig.GetInstance().Get("gamekit_ads_display_logic", "level_based");
            _firstInterstitialDelay = RemoteConfig.GetInstance().GetInt("gamekit_ads_first_interstitial_delay", 1200);

            _intTimer = new Timer(1000);
            _intRemainingTime = (int)((_firstInterstitialDelay - ElephantCore.Instance.timeSpend / 1000));
            if (_intRemainingTime > 0)
            {
                _intTimer.Elapsed += OnIntTimedEvent;
                _intTimer.Start();
                _isIntLocked = true;
            }
            else
            {
                _isIntLocked = false;
            }

            if (string.Equals(_interShowLogic, ShowLogicIncremental))
            {
                InvokeRepeating("ShowInterstitialIncremental", _firstInterDisplayTimeAfterStart,
                    _interstitialDisplayInterval);
            }
        }

        private void OnIntTimedEvent(object sender, ElapsedEventArgs e)
        {
            if (_intRemainingTime > 0)
            {
                _intRemainingTime--;
                _isIntLocked = true;
            }
            else
            {
                _intTimer.Stop();
                _isIntLocked = false;
            }
        }

        public bool IsTimerReady(float realTimeSinceStartup)
        {
            if (_isIntLocked) return false;
            if (_interstitialDisplayInterval == 0) return true;

            if (realTimeSinceStartup < _interstitialDisplayInterval && _lastTimeAdDisplayed == 0)
                return true;

            _timeSinceLastTimeAdDisplayed = realTimeSinceStartup - _lastTimeAdDisplayed;
            var timeToNextInterstitial = (int)(_interstitialDisplayInterval - _timeSinceLastTimeAdDisplayed);
            var timeToNextInterstitialAfterAddition = timeToNextInterstitial + _addedValue;

            if (timeToNextInterstitialAfterAddition > 0)
            {
                ElephantLog.Log("AUTO-SHOW", "TimerLock: Still in interval: " + timeToNextInterstitialAfterAddition);
                return false;
            }

            return true;
        }

        private bool IsLevelReady()
        {
            if (_isIntLocked) return false;
            if (_firstLevelToDisplay == -1 && _levelFrequency == -1) return true;

            var currentLevel = MonitoringUtils.GetInstance().GetCurrentLevel().level;

            if (_firstLevelToDisplay == -1 && _levelFrequency >= 0)
                return currentLevel - _lastLevelAdDisplayed >= _levelFrequency;

            if (_levelFrequency == -1 && _firstLevelToDisplay >= 0)
                return currentLevel > _firstLevelToDisplay;

            return currentLevel - _lastLevelAdDisplayed >= _levelFrequency && currentLevel > _firstLevelToDisplay;
        }

        private void ShowInterstitialLevelBased()
        {
            var isLevelReady = IsLevelReady();
            var isTimerReady = IsTimerReady(Time.realtimeSinceStartup);

            if (!isTimerReady || !isLevelReady)
            {
                ElephantLog.Log("AUTO-SHOW", "LOCKED on ShowInterstitial");

                var notShowCalledParams = Params.New();
                notShowCalledParams.Set("is_level_ready", isLevelReady.ToString());
                notShowCalledParams.Set("is_timer_ready", isTimerReady.ToString());
                notShowCalledParams.Set("time_since_last_time_ad_displayed", _timeSinceLastTimeAdDisplayed);
                notShowCalledParams.Set("is_interstitial_ready", _isInterstitialReady ? 1 : 0);
                notShowCalledParams.Set("added_time_value", _addedValue);
                notShowCalledParams.Set("last_displayed_ad_time", _lastTimeAdDisplayed);
                Elephant.Event(InterstitialEventPrefix + "_NotShowCalled",
                    MonitoringUtils.GetInstance().GetCurrentLevel().level, notShowCalledParams);
                return;
            }

            showInterstitial(RollicInterstitialAd.InterstitialAdSource.LevelComplete);

            var showCalledParams = Params.New();
            showCalledParams.Set("time_since_last_time_ad_displayed", _timeSinceLastTimeAdDisplayed);
            showCalledParams.Set("added_time_value", _addedValue);
            showCalledParams.Set("last_displayed_ad_time", _lastTimeAdDisplayed);
            showCalledParams.Set("is_interstitial_ready", _isInterstitialReady ? 1 : 0);
            Elephant.Event(InterstitialEventPrefix + "_ShowCalled",
                MonitoringUtils.GetInstance().GetCurrentLevel().level, showCalledParams);
        }

        private void ShowInterstitialIncremental()
        {
            var isTimerReady = IsTimerReady(Time.realtimeSinceStartup);

            if (!isTimerReady)
            {
                ElephantLog.Log("AUTO-SHOW", "LOCKED on ShowInterstitial");
                return;
            }

            showInterstitial(RollicInterstitialAd.InterstitialAdSource.Other);
        }

        #endregion

        #region Rate Us

        public void InitRateUs()
        {
            ElephantLog.Log("RATE-US", "Initializing RateUs functionality");

            if (!_isRateUsEnabled)
            {
                ElephantLog.Log("RATE-US", "Rate Us disabled via remote config");
                return;
            }

            _rateUsDisplayInterval = RemoteConfig.GetInstance().GetInt("gamekit_rate_us_display_interval", 100000);
            _firstRateUsDisplayAfterStart =
                RemoteConfig.GetInstance().GetInt("gamekit_rate_us_first_display_after_start", 600);
            _firstRateUsLevelToDisplay =
                RemoteConfig.GetInstance().GetInt("gamekit_rate_us_first_level_to_display", 10);
            _rateUsLevelFrequency = RemoteConfig.GetInstance().GetInt("gamekit_rate_us_level_frequency", 100000);
            _rateUsShowLogic = RemoteConfig.GetInstance().Get("gamekit_rate_us_display_logic", "level_based");
            _firstRateUsDelay = RemoteConfig.GetInstance().GetInt("gamekit_rate_us_first_delay", 600);

            _rateUsTimer = new Timer(1000);
            _rateUsRemainingTime = (int)((_firstRateUsDelay - ElephantCore.Instance.timeSpend / 1000));
            if (_rateUsRemainingTime > 0)
            {
                _rateUsTimer.Elapsed += OnRateUsTimedEvent;
                _rateUsTimer.Start();
                _isRateUsLocked = true;
            }
            else
            {
                _isRateUsLocked = false;
            }

            if (string.Equals(_rateUsShowLogic, ShowLogicIncremental))
            {
                InvokeRepeating("ShowRateUsIncremental", _firstRateUsDisplayAfterStart, _rateUsDisplayInterval);
            }
        }

        private void OnRateUsTimedEvent(object sender, ElapsedEventArgs e)
        {
            if (_rateUsRemainingTime > 0)
            {
                _rateUsRemainingTime--;
                _isRateUsLocked = true;
            }
            else
            {
                _rateUsTimer.Stop();
                _isRateUsLocked = false;
            }
        }

        public bool IsRateUsTimerReady(float realTimeSinceStartup)
        {
            if (_isRateUsLocked) return false;
            if (_rateUsDisplayInterval == 0) return true;

            if (realTimeSinceStartup < _rateUsDisplayInterval && _lastTimeRateUsDisplayed == 0)
                return true;

            _timeSinceLastRateUsDisplayed = realTimeSinceStartup - _lastTimeRateUsDisplayed;
            var timeToNextRateUs = (int)(_rateUsDisplayInterval - _timeSinceLastRateUsDisplayed);

            return timeToNextRateUs <= 0;
        }

        private bool IsRateUsLevelReady()
        {
            if (_isRateUsLocked) return false;
            if (_firstRateUsLevelToDisplay == -1 && _rateUsLevelFrequency == -1) return true;

            var currentLevel = MonitoringUtils.GetInstance().GetCurrentLevel().level;

            if (_firstRateUsLevelToDisplay == -1 && _rateUsLevelFrequency >= 0)
                return currentLevel - _lastLevelRateUsDisplayed >= _rateUsLevelFrequency;

            if (_rateUsLevelFrequency == -1 && _firstRateUsLevelToDisplay >= 0)
                return currentLevel > _firstRateUsLevelToDisplay;

            return (currentLevel - _lastLevelRateUsDisplayed >= _rateUsLevelFrequency ||
                    _lastLevelRateUsDisplayed < 1) &&
                   currentLevel > _firstRateUsLevelToDisplay;
        }

        public void ShowRateUsLevelBased()
        {
            var isLevelReady = IsRateUsLevelReady();
            var isTimerReady = IsRateUsTimerReady(Time.realtimeSinceStartup);

            if (!isTimerReady || !isLevelReady)
            {
                ElephantLog.Log("RATE-US", "LOCKED on ShowRateUs");

                var notShowCalledParams = Params.New();
                notShowCalledParams.Set("is_level_ready", isLevelReady.ToString());
                notShowCalledParams.Set("is_timer_ready", isTimerReady.ToString());
                notShowCalledParams.Set("time_since_last_time_rate_us_displayed", _timeSinceLastRateUsDisplayed);
                notShowCalledParams.Set("last_displayed_rate_us_time", _lastTimeRateUsDisplayed);
                Elephant.Event(RateUsEventPrefix + "_NotShowCalled",
                    MonitoringUtils.GetInstance().GetCurrentLevel().level, notShowCalledParams);
                return;
            }

            RequestRateUs();
        }

        public void ShowRateUsIncremental()
        {
            var isTimerReady = IsRateUsTimerReady(Time.realtimeSinceStartup);

            if (!isTimerReady)
            {
                ElephantLog.Log("RATE-US", "LOCKED on ShowRateUs");

                var notShowCalledParams = Params.New();
                notShowCalledParams.Set("is_timer_ready", isTimerReady.ToString());
                notShowCalledParams.Set("time_since_last_time_rate_us_displayed", _timeSinceLastRateUsDisplayed);
                notShowCalledParams.Set("last_displayed_rate_us_time", _lastTimeRateUsDisplayed);
                Elephant.Event(RateUsEventPrefix + "_NotShowCalled",
                    MonitoringUtils.GetInstance().GetCurrentLevel().level, notShowCalledParams);
                return;
            }

            RequestRateUs();
        }

        private void RequestRateUs()
        {
            ElephantLog.Log("RATE-US", "ShowRateUs called");

            var showCalledParams = Params.New();
            showCalledParams.Set("time_since_last_time_rate_us_displayed", _timeSinceLastRateUsDisplayed);
            showCalledParams.Set("last_displayed_rate_us_time", _lastTimeRateUsDisplayed);
            Elephant.Event(RateUsEventPrefix + "_ShowCalled", MonitoringUtils.GetInstance().GetCurrentLevel().level,
                showCalledParams);

            var popupShown = false;

#if UNITY_IOS && !UNITY_EDITOR
            popupShown = Device.RequestStoreReview();
#endif

            if (popupShown)
            {
                _lastTimeRateUsDisplayed = Time.realtimeSinceStartup;
                _lastLevelRateUsDisplayed = MonitoringUtils.GetInstance().GetCurrentLevel().level;
                ElephantLog.Log("RATE-US", "Rate Us popup was shown successfully");
            }
            else
            {
                ElephantLog.Log("RATE-US", "Rate Us popup couldn't be shown");
            }
        }

        #endregion

        #region Banner Timer Management

        private void InitBannerTimer()
        {
            _firstBannerDelay = RemoteConfig.GetInstance().GetInt("gamekit_ads_first_banner_delay", 1200);

            _bannerTimer = new Timer(1000);
            _bannerRemainingTime = (int)((_firstBannerDelay - ElephantCore.Instance.timeSpend / 1000));
            if (_bannerRemainingTime > 0)
            {
                _bannerTimer.Elapsed += OnBannerTimedEvent;
                _bannerTimer.Start();
            }
            else
            {
                StartCoroutine(loadBannerAsync());
            }
        }

        private void OnBannerTimedEvent(object sender, ElapsedEventArgs e)
        {
            if (_bannerRemainingTime > 0)
            {
                _bannerRemainingTime--;
            }
            else
            {
                ExecuteOnMainThread(() => { StartCoroutine(loadBannerAsync()); });
                _bannerTimer.Stop();
            }
        }

        #endregion
    }
}