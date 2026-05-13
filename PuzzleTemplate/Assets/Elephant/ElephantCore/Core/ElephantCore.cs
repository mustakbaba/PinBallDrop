using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;
#if UNITY_IOS
using UnityEngine.iOS;
#endif
using UnityEngine.Networking;
using UnityEngine.Serialization;

namespace ElephantSDK
{
    public delegate void OnInitialized();

    public delegate void OnOpenResult(bool gdprRequired, ComplianceTosResponse tos);

    public delegate void OnRemoteConfigLoaded();

    public delegate void OnNotificationOpened();

    public delegate void OnOfferUIFetched();
    
    public delegate void InternetConnectionChanged(bool isConnected);

    public class ElephantCore : MonoBehaviour
    {
        [FormerlySerializedAs("GameID")] public string gameID = "";
        [FormerlySerializedAs("GameSecret")] public string gameSecret = "";

        public static ElephantCore Instance;
        private Queue<ElephantRequest> _queue = new Queue<ElephantRequest>();
        private List<ElephantRequest> _failedQueue = new List<ElephantRequest>();
        private bool processQueues = false;
        private bool processFailedBatch = true;

        public UserOps UserOps = new();
        public IapOps IapOps = new();
        public VitalOps VitalOps = new();

        private static string QUEUE_DATA_FILE = "ELEPHANT_DATA_QUEUE_";

        public bool sdkIsReady = false;
        public bool circuitBreakerEnabled = false;
        public int healthCheckRetryPeriod = 300;
        public int failRetryCount = 4;
        private float startingWaitTime = 0.5f;
        private float maxWaitTime = 5f;

        private bool openRequestWaiting;
        private bool openRequestSucceded;
        private SessionData currentSession;
        public long realSessionId;
        internal long firstInstallTime;
        public long timeSpend;
        public long installTime;
        public DateTime installTimeForCv;
        public string idfa = "";
        public string idfv = "";
        public string adjustId = "";
        internal string buildNumber = "";
        public string consentStatus = "NotDetermined";
        public string userId = "";
        internal List<MirrorData> mirrorData;
        internal int eventOrder = 0;
        internal float focusLostTime = 0;
        public string networkName = "";
        public string campaignName = "";
        public string adGroupName = "";
        public string creativeName = "";
        public double uaCost;
        public long firstOpenTimeStamp;

        private OpenResponse openResponse = new();
        private string cachedOpenResponse;
        private ElephantComplianceManager _elephantComplianceManager;

        private static int MAX_FAILED_COUNT = 250;

        public static event OnInitialized onInitialized;
        public static event OnOpenResult onOpen;
        public static event OnRemoteConfigLoaded onRemoteConfigLoaded;
        public static event OnNotificationOpened onNotificationOpened;
        public static event OnOfferUIFetched onOfferUIFetched;
        public static event InternetConnectionChanged OnInternetConnectionChanged;

        private List<IElephantAdapter> Adapters;
        public IFacebookElephantAdapter FacebookElephantAdapter;
        public IAdjustElephantAdapter AdjustElephantAdapter;
        public IRollicAdsElephantAdapter RollicAdsElephantAdapter;
        public ILiveOpsElephantAdapter LiveOpsElephantAdapter;
        public IPushElephantAdapter PushElephantAdapter;
        public IUsercentricsElephantAdapter UsercentricsElephantAdapter;
        public IHelpShiftElephantAdapter HelpShiftElephantAdapter;
        public IZyngaPublishingElephantAdapter ZyngaPublishingElephantAdapter;
        public IFirebaseElephantAdapter FirebaseElephantAdapter;
        public IElephantAdsAdapter ElephantAdsAdapter;

        private readonly float[] _fpsBuffer = new float[60];
        private float _lastUpdated;
        private int _c = 0;
        private float _fps;

        private static MetaDataUtils _metaDataUtils;

        public bool isSoundFixEnabled = false;
        public bool isUcEnabled = false;

        public bool isStorageRequestDone = false;
        public bool isCollectiblesRequestDone = false;
        public bool isStorageLoaded = false;
        
        private bool lastInternetState = true;
        
        void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            DontDestroyOnLoad(gameObject);
#if !UNITY_EDITOR && UNITY_ANDROID
            ElephantAndroid.Init();
#endif
            bool logsEnabled = false;
            string savedConfig = Utils.ReadFromFile(ElephantConstants.REMOTE_CONFIG_FILE);
    
            if (!string.IsNullOrEmpty(savedConfig))
            {
                try
                {
                    var configData = JsonConvert.DeserializeObject<ConfigResponse>(savedConfig);
                    if (configData?.data != null)
                    {
                        for (int i = 0; i < configData.data.keys.Count; i++)
                        {
                            if (configData.data.keys[i] == "elephant_logs_enabled")
                            {
                                bool.TryParse(configData.data.values[i], out logsEnabled);
                                break;
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    Debug.Log($"Could not parse saved config for logging: {e.Message}");
                }
            }
            else
            {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                logsEnabled = true;
#else
                logsEnabled = false;
#endif
            }
    
            ElephantLog.GetInstance(logsEnabled ? ElephantLogLevel.Debug : ElephantLogLevel.Prod);
    
            Adapters = new List<IElephantAdapter>();
        }

        public void AddAdapters(IElephantAdapter adapter)
        {
            ElephantLog.Log("Adapters",adapter.GetType().ToString());
            Adapters.Add(adapter);
        }

        void Start()
        {
            RebuildQueue();
            processQueues = true;
        }

        void Update()
        {
            if (!sdkIsReady) return;
            
            if (openResponse.internal_config.monitoring_enabled)
            {
                CheckInternetConnection();
            }

            if (openResponse.internal_config.monitoring_enabled)
            {
                LogMonitoringData();
            }
        }
        
        private void CheckInternetConnection()
        {
            var isConnected = Utils.IsConnected();

            if (isConnected == lastInternetState) return;
            lastInternetState = isConnected;

            ElephantLog.Log("INTERNET CONNECTION", $"Connection State Changed: {(isConnected ? "Connected" : "Disconnected")}");

            OnInternetConnectionChanged?.Invoke(isConnected);
        }
        
        private void LogMonitoringData()
        {
            if (openResponse.internal_config.memory_usage_enabled)
            {
#if UNITY_EDITOR
#elif UNITY_IOS
                MonitoringUtils.GetInstance().SetMemoryUsage(ElephantIOS.gameMemoryUsage());
                MonitoringUtils.GetInstance().SetMemoryUsagePercentage(ElephantIOS.gameMemoryUsagePercent());
#elif UNITY_ANDROID
                MonitoringUtils.GetInstance().SetMemoryUsage(ElephantAndroid.GameMemoryUsage());
                MonitoringUtils.GetInstance().SetMemoryUsagePercentage(ElephantAndroid.GameMemoryUsagePercentage());
#endif
            }

            _fpsBuffer[_c] = 1.0f / Time.deltaTime;
            _c = (_c + 1) % _fpsBuffer.Length;
            if (Time.time - _lastUpdated >= 1)
            {
                _lastUpdated = Time.time;
                _fps = MonitoringUtils.GetInstance().CalculateFps(_fpsBuffer);

                if (float.IsInfinity(_fps) || float.IsNaN(_fps)) return;
                MonitoringUtils.GetInstance().LogFps(Math.Round(_fps, 1));
                MonitoringUtils.GetInstance().LogCurrentLevel();
            }
        }
        
        private void OnDeepLink(string deeplinkURL) 
        { 
            Elephant.TriggerDeepLink(deeplinkURL);
        }

        #region Init

        public void Init()
        {
            InvokeRepeating(nameof(CheckApiHealth), 1, healthCheckRetryPeriod);
            
            InitCredentials();

            InitAdapters();
            
            InitUtilities();

            InitAnalyticsTools();
            
            StartCoroutine(InitSDK());
        }

        private void InitAnalyticsTools()
        {
            FacebookElephantAdapter?.ActivateFacebook(ElephantThirdPartyIds.FacebookAppId,
                ElephantThirdPartyIds.FacebookClientToken);
            
#if !UNITY_EDITOR && UNITY_ANDROID
            AdjustElephantAdapter?.InitAdjust(ElephantThirdPartyIds.AdjustAppKey,
                RemoteConfig.GetInstance().GetBool("conversion_value_service_enabled", false), OnDeepLink);
#elif UNITY_IOS
            try
            {
                if (VersionCheckUtils.GetInstance()
                        .CompareVersions(Device.systemVersion, "14.5") < 0)
                {
                    AdjustElephantAdapter?.InitAdjust(ElephantThirdPartyIds.AdjustAppKey,
                        RemoteConfig.GetInstance().GetBool("conversion_value_service_enabled", false),
                        OnDeepLink ,true);
                } 
            }
            catch (Exception e)
            {
                ElephantLog.LogError("ELEPHANT", "Error on Adjust Init: " + e.Message);
            }
#endif
        }

        private void InitUtilities()
        {
            VersionCheckUtils.GetInstance();

#if UNITY_EDITOR
            buildNumber = "";
#elif UNITY_ANDROID
            buildNumber = ElephantAndroid.getBuildNumber();
#elif UNITY_IOS
            buildNumber = ElephantIOS.getBuildNumber();
#else
            buildNumber = "";
#endif

#if UNITY_EDITOR
            // No-op
            firstInstallTime = 0;
#elif UNITY_IOS
            firstInstallTime = ElephantIOS.getFirstInstallTime();
#elif UNITY_ANDROID
            firstInstallTime = ElephantAndroid.GetFirstInstallTime();
#else
            firstInstallTime = 0;
#endif

            MonitoringUtils.GetInstance().LogBatteryLevel(MonitoringUtils.KeySessionStart);
        }

        private void InitAdapters()
        {
            foreach (var elephantAdapter in Adapters)
            {
                switch (elephantAdapter)
                {
                    case IFacebookElephantAdapter facebookAdapter:
                        FacebookElephantAdapter = facebookAdapter;
                        ElephantLog.Log("FACEBOOK-ELEPHANT", "FACEBOOK is Initialized");
                        break;
                    case IAdjustElephantAdapter adjustAdapter:
                        AdjustElephantAdapter = adjustAdapter;
                        ElephantLog.Log("ADJUST-ELEPHANT", "ADJUST is Initialized");
                        break;
                    case IRollicAdsElephantAdapter rollicAdsAdapter:
                        RollicAdsElephantAdapter = rollicAdsAdapter;
                        ElephantLog.Log("ROLLICADS-ELEPHANT", "ROLLICADS is Initialized");
                        break;
                    case ILiveOpsElephantAdapter liveOpsAdapter:
                        LiveOpsElephantAdapter = liveOpsAdapter;
                        ElephantLog.Log("LIVEOPS-ELEPHANT", "LIVEOPS is Initialized");
                        break;
                    case IPushElephantAdapter pushAdapter:
                        PushElephantAdapter = pushAdapter;
                        ElephantLog.Log("PUSH-ELEPHANT", "PUSH is Initialized");
                        break;
                    case IUsercentricsElephantAdapter usercentricsAdapter:
                        UsercentricsElephantAdapter = usercentricsAdapter;
                        ElephantLog.Log("UC-ELEPHANT", "UC is Initialized");
                        break;
                    case IHelpShiftElephantAdapter helpShiftAgent:
                        HelpShiftElephantAdapter = helpShiftAgent;
                        ElephantLog.Log("HelpShift-ELEPHANT", "HelpShift is Initialized");
                        break;
                    case IZyngaPublishingElephantAdapter zyngaPublishingElephantAdapter:
                        ZyngaPublishingElephantAdapter = zyngaPublishingElephantAdapter;
                        ElephantLog.Log("ZyngaPublishing-ELEPHANT", "ZyngaPublishingElephantAdapter is Initialized");
                        break;
                    case IFirebaseElephantAdapter firebaseAdapter:
                        FirebaseElephantAdapter = firebaseAdapter;
                        ElephantLog.Log("FIREBASE-ELEPHANT", "FIREBASE is Initialized");
                        break;
                    case IElephantAdsAdapter elephantAdsAdapter:
                        ElephantAdsAdapter = elephantAdsAdapter;
                        ElephantLog.Log("ELEPHANT-ADS", "ELEPHANT ADS is Initialized");
                        break;
                    default:
                        ElephantLog.LogError("ELEPHANT", "UNKNOWN ADAPTER");
                        break;
                }
            }

        }

        private void InitCredentials()
        {
            this.gameID = ElephantThirdPartyIds.GameId;
            this.gameSecret = ElephantThirdPartyIds.GameSecret;

            if (gameID.Trim().Length == 0 || gameSecret.Trim().Length == 0)
            {
                ElephantLog.LogError("ELEPHANT INIT",
                    "Game ID and Game Secret are not present, make sure you replace them with yours using Window -> Elephant -> Edit Settings");
            }
        }

        #endregion
        
        private IEnumerator InitSDK()
        {
            string savedConfig = Utils.ReadFromFile(ElephantConstants.REMOTE_CONFIG_FILE);
            userId = Utils.ReadFromFile(ElephantConstants.USER_DB_ID) ?? "";
#if UNITY_IOS && !UNITY_EDITOR
            if (KeyChainUtils.KeyExists(ElephantConstants.USER_DB_ID))
            {
                userId = KeyChainUtils.GetValue(ElephantConstants.USER_DB_ID);
            }
#endif

            var firstOpenString = Utils.ReadFromFile(ElephantConstants.FIRST_OPEN_TIME);
            if (!string.IsNullOrEmpty(firstOpenString))
            {
                var longFirstOpenTime = Convert.ToInt64(firstOpenString);
                installTime = longFirstOpenTime;
            }

            ElephantLog.Log("Init", "Remote Config From File --> " + savedConfig);

            var isUsingRemoteConfig = 0;
            
            if (savedConfig != null)
            {
                RemoteConfig.GetInstance().Init(savedConfig);
                RemoteConfig.GetInstance().SetFirstOpen(false);
                openResponse.remote_config_json = savedConfig;
                if (DateTime.TryParse(Utils.ReadFromFile(ElephantConstants.INSTALL_TIME_FOR_CV), out var installTimeCv))
                {
                    installTimeForCv = installTimeCv;
                }
            }
            else
            {
                // First open 
                RemoteConfig.GetInstance().SetFirstOpen(true);
                firstOpenTimeStamp = Utils.Timestamp();
                Utils.SaveToFile(ElephantConstants.FIRST_OPEN_TS, firstOpenTimeStamp.ToString());
                installTime = Utils.Timestamp();
                installTimeForCv = DateTime.UtcNow;
                Utils.SaveToFile(ElephantConstants.INSTALL_TIME_FOR_CV, installTimeForCv.ToString("o"));
                Utils.SaveToFile(ElephantConstants.FIRST_OPEN_TIME, installTime.ToString());
            }

            openResponse.user_id = userId;

            openRequestWaiting = true;
            openRequestSucceded = false;

            float startTime = Time.time;
            var realTimeSinceStartup = Time.realtimeSinceStartup;
            var realTimeBeforeRequest = DateTime.Now;
            var savedTimeSpend = Utils.ReadFromFile(ElephantConstants.TimeSpend);
            try
            {
                timeSpend = !string.IsNullOrEmpty(savedTimeSpend) ? long.Parse(savedTimeSpend) : 0;
            }
            catch (Exception e)
            {
                ElephantLog.LogError("ELEPHANT", "Error on time spend: " + e.Message);
            }

            RequestIDFAAndOpen();

            while (openRequestWaiting && (Time.time - startTime) < 5f)
            {
                yield return null;
            }

            isUsingRemoteConfig = openRequestSucceded ? 1 : -1;

            if (!openRequestSucceded)
            {
                var createNewUserJob = UserOps.CreateOrGetNewUser(response =>
                {
                    if (response.responseCode != 200 || response.data == null) return;

                    var openResponseForNewUser = response.data;
                    this.userId = openResponseForNewUser.user_id;
                    if (ZyngaPublishingElephantAdapter != null)
                    {
                        ZyngaPublishingElephantAdapter.RollicUserId = userId;
                    }
                }, s => { ElephantLog.Log("COMPLIANCE", "Error on new user creation: " + s); });
                StartCoroutine(createNewUserJob);
            }

            ElephantLog.Log("OPEN REQUEST", JsonConvert.SerializeObject(openResponse));

            RemoteConfig.GetInstance().Init(openResponse.remote_config_json);
            Utils.SaveToFile(ElephantConstants.REMOTE_CONFIG_FILE, openResponse.remote_config_json);
            Utils.SaveToFile(ElephantConstants.USER_DB_ID, openResponse.user_id);
            
            var logsEnabled = RemoteConfig.GetInstance().GetBool("elephant_logs_enabled", false);
            ElephantLog.UpdateLogLevel(logsEnabled ? ElephantLogLevel.Debug : ElephantLogLevel.Prod);
            
#if UNITY_IOS && !UNITY_EDITOR
            KeyChainUtils.SaveValue(ElephantConstants.USER_DB_ID, openResponse.user_id);
#endif

            Utils.SaveToFile(ElephantConstants.CACHED_OPEN_RESPONSE, JsonConvert.SerializeObject(openResponse));
            userId = openResponse.user_id;
            mirrorData = openResponse.mirror_data ?? new List<MirrorData>();
            currentSession.user_tag = RemoteConfig.GetInstance().GetTag();
            
            var parameters = Params.New()
                .Set("real_duration", (DateTime.Now - realTimeBeforeRequest).TotalMilliseconds)
                .Set("game_duration", (Time.time - startTime) * 1000)
                .Set("real_time_since_startup", (Time.realtimeSinceStartup - realTimeSinceStartup) * 1000)
                .Set("is_using_remote_config", isUsingRemoteConfig)
                .CustomString(JsonConvert.SerializeObject(openResponse));

            Elephant.Event("open_request", -1, parameters);

#if !UNITY_EDITOR
            // T0 - Check Network Reachability
            if (ElephantCore.Instance.GetOpenResponse().internal_config.reachability_check_enabled)
            {
                Elephant.ShowNetworkOfflineDialog();

                var waitTime = startingWaitTime;
                while (!Utils.IsConnected())
                {
                    yield return new WaitForSeconds(waitTime);
                    waitTime = Mathf.Min(waitTime * 2f, maxWaitTime);
                }
            }
#endif

            _elephantComplianceManager = ElephantComplianceManager.GetInstance(openResponse);

            isSoundFixEnabled = RemoteConfig.GetInstance().GetBool("sound_fix_enabled", false);
            isUcEnabled = RemoteConfig.GetInstance().GetBool("usercentrics_enabled", true);

            // T0.5 - Get Adapter Resources
            LiveOpsElephantAdapter?.RetrieveOfferAssetUrls();
            
            ElephantStorageManager.GetInstance().RequestStorage();
            var shouldWaitForStorage = openResponse.internal_config.storage_remote_enabled;
            while ((!isStorageRequestDone || !isCollectiblesRequestDone) && shouldWaitForStorage)
            {
                yield return null;
            }

            // T1 - First check: Force Update
#if !UNITY_EDITOR
            if (_elephantComplianceManager.CheckForceUpdate()) yield break;
#endif
            // T2 - check if the user is blocked from data deletion
            _elephantComplianceManager.ShowBlockedPopUp();
            if (openResponse.compliance.blocked.is_blocked) yield break;

            if (onOpen != null)
            {
                // T3 - show tos and pp (replacement for old gdpr)
                _elephantComplianceManager.ShowTosAndPp(onOpen);
            }
            else
            {
                ElephantLog.Log("ELEPHANT INIT", "ElephantSDK onOpen event is not handled");
            }

            // T4 - start Zynga Publishing SDK
            if (ZyngaPublishingElephantAdapter != null)
            {
                ZyngaPublishingElephantAdapter.ZyngaGameId = openResponse.player_data.app_id;
                ZyngaPublishingElephantAdapter.RollicUserId = openResponse.user_id;
            }
            
            // Check if Pub SDK is enabled in the remote config
            var isPubSdkEnabledInRemoteConfig = RemoteConfig.GetInstance().GetBool("rollic_zynga_publishing_sdk_enabled", true);
            if (isPubSdkEnabledInRemoteConfig)
            {
                ZyngaPublishingElephantAdapter?.Initialize();
            }
            else
            {
                ElephantLog.Log("ELEPHANT INIT", "Zynga Publishing SDK is disabled in remote config. Stopping the SDK.");
                ZyngaPublishingElephantAdapter?.Stop();
            }

            // Log the event for the PubSDK status
            var pubSdkStatusParams = Params.New()
                .Set("zynga_game_id", ZyngaPublishingElephantAdapter?.ZyngaGameId)
                .Set("rollic_user_id", ZyngaPublishingElephantAdapter?.RollicUserId)
                .Set("has_pubsdk", ZyngaPublishingElephantAdapter != null ? 1 : 0)
                .Set("init_status", isPubSdkEnabledInRemoteConfig ? 1 : 0);
            Elephant.Event("pubsdk_status", -1, pubSdkStatusParams);

            // T5 - if offline session flag is filled, send the data and flush it
            var offlineFlag = Utils.ReadFromFile(ElephantConstants.OFFLINE_FLAG);
            if (!string.IsNullOrEmpty(offlineFlag))
            {
                var param = Params.New().Set("sessionId", offlineFlag);
                Elephant.Event("previous_offline_session", -1, param);
                Utils.SaveToFile(ElephantConstants.OFFLINE_FLAG, "");
            }

            if (openResponse.internal_config.helpshift_enabled)
            {
                
#if UNITY_ANDROID
                var domainName = ElephantThirdPartyIds.HelpShiftDomainAndroid;
                var appId = ElephantThirdPartyIds.HelpShiftAppIdAndroid;
#else
                var domainName = ElephantThirdPartyIds.HelpshiftDomainIOS;
                var appId = ElephantThirdPartyIds.HelpShiftAppIdIOS;
#endif
                
                HelpShiftElephantAdapter?.Init(domainName, appId);
            }

            if (openResponse.internal_config.dynamic_events_enabled)
            {
                StartCoroutine(RollicEventUtils.GetInstance().FetchTokenLogicsFromEndpoint());
            }

            sdkIsReady = true;
            onRemoteConfigLoaded?.Invoke();
        }
        public void ShowSecondLayer(string message)
        {
#if UNITY_EDITOR
            ElephantLog.Log("USERCENTRICS", "Not supported in editor");
            return;
#else
                
            ElephantCore.Instance.UsercentricsElephantAdapter?.ShowSecondLayer();
#endif
        }
        
        public void OpenIdfaConsent()
        {
#if UNITY_IOS && !UNITY_EDITOR
            ElephantUI.Instance.StartIDFAListener();
            if (ElephantCore.Instance.GetOpenResponse().internal_config.idfa_consent_enabled)
            {
                InternalConfig internalConfig = ElephantCore.Instance.GetOpenResponse().internal_config;

                Elephant.Event("ask_idfa_consent", -1);
                ElephantIOS.showIdfaConsent(internalConfig.idfa_consent_type,
                    internalConfig.idfa_consent_delay, internalConfig.idfa_consent_position,
                    internalConfig.consent_text_body, internalConfig.consent_text_action_body,
                    internalConfig.consent_text_action_button, internalConfig.terms_of_service_text,
                    internalConfig.privacy_policy_text, internalConfig.terms_of_service_url,
                    internalConfig.privacy_policy_url);
            }
#endif
        }
        
        private void SendVersionsEvent()
        {
            var versionCheckUtils = VersionCheckUtils.GetInstance();
            var versionData = new VersionData(Application.version, ElephantVersion.SDK_VERSION,
                SystemInfo.operatingSystem, versionCheckUtils.UnityVersion, versionCheckUtils.GameKitVersion);

            var parameters = Params.New()
                .CustomString(JsonConvert.SerializeObject(versionData));
            
            ElephantLog.LogCustomKey("sdk_version", ElephantVersion.SDK_VERSION);

            Elephant.Event("elephant_sdk_versions_info", -1, parameters);
        }

        public OpenResponse GetOpenResponse()
        {
            return openResponse;
        }

        private void RequestIDFAAndOpen()
        {
            idfv = SystemInfo.deviceUniqueIdentifier;
#if UNITY_EDITOR
            idfa = SystemInfo.deviceUniqueIdentifier;
            StartCoroutine(OpenRequest());
#elif UNITY_IOS
            idfa = ElephantIOS.IDFA();
            consentStatus = ElephantIOS.getConsentStatus();
            StartCoroutine(OpenRequest());
#elif UNITY_ANDROID
            idfa = ElephantAndroid.FetchAdId();
            StartCoroutine(OpenRequest());
#else
            idfa = "UNITY_UNKOWN_IDFA";
            StartCoroutine(OpenRequest());
#endif
        }

        private IEnumerator OpenRequest()
        {
            if (onInitialized != null)
                onInitialized();


            currentSession = SessionData.CreateSessionData();
            realSessionId = currentSession.GetSessionID();
            SendVersionsEvent();

            var openData = OpenData.CreateOpenData();
            openData.session_id = currentSession.GetSessionID();
            openData.idfv = idfv;
            openData.idfa = idfa;
            openData.user_id = userId;
            openData.tc_string = Instance.GetTCString();
            
            cachedOpenResponse = Utils.ReadFromFile(ElephantConstants.CACHED_OPEN_RESPONSE);
            if (!string.IsNullOrEmpty(cachedOpenResponse))
            {
                var tempOpenResponse = JsonConvert.DeserializeObject<OpenResponse>(cachedOpenResponse);
                if (tempOpenResponse != null)
                {
                    // Previous open response has successfully saved. Send Hash..
                    openData.hash = tempOpenResponse.hash;
                }
            }

            var json = JsonConvert.SerializeObject(openData);
            var bodyJson = JsonConvert.SerializeObject(new ElephantData(json, GetCurrentSession().GetSessionID()));
            var networkManager = new GenericNetworkManager<OpenResponse>();
            var postWithResponse = networkManager.PostWithResponse(ElephantConstants.OPEN_EP, bodyJson, response =>
            {
                switch (response.responseCode)
                {
                    case 200 when response.data != null:
                        openRequestSucceded = true;
                        openResponse = response.data;
                        break;
                    case 204 when !string.IsNullOrEmpty(cachedOpenResponse):
                    {
                        var data = JsonConvert.DeserializeObject<OpenResponse>(cachedOpenResponse);
                        if (data != null)
                        {
                            Elephant.Event("hashed_open_response", -1);
                            openRequestSucceded = true;
                            openResponse = data;
                        }
                        break;
                    }
                    default:
                    {
                        if (!string.IsNullOrEmpty(cachedOpenResponse))
                        {
                            var data = JsonConvert.DeserializeObject<OpenResponse>(cachedOpenResponse);
                            if (data != null)
                            {
                                Elephant.Event("fallback_open_response", -1);
                                openResponse = data;
                            }
                        }
                        break;
                    }
                }

                openRequestWaiting = false;
            }, s =>
            {
                if (!string.IsNullOrEmpty(cachedOpenResponse))
                {
                    var data = JsonConvert.DeserializeObject<OpenResponse>(cachedOpenResponse);
                    if (data != null)
                    {
                        Elephant.Event("fallback_open_response", -1);
                        openResponse = data;
                    }
                }
                openRequestWaiting = false;
            });

            yield return postWithResponse;
            
            ElephantLog.LogCustomKey("user_id", userId);
            ElephantLog.LogCustomKey("session_id", currentSession.GetSessionID().ToString());
            ElephantLog.LogCustomKey("install_time", installTime.ToString());
    
            if (!string.IsNullOrEmpty(adjustId)) {
                ElephantLog.LogCustomKey("adjust_id", adjustId);
            }
        }

        public void CheckApiHealth()
        {
            StartCoroutine(VitalOps.CheckApiHealth());
        }

        public void PinRequest()
        {
            StartCoroutine(UserOps.PinRequest());
        }

        public void VerifyPurchase(IapVerifyRequest request, Action<bool> callback)
        {
            StartCoroutine(IapOps.VerifyPurchase(request, callback));
        }

        public void IsIapBanned(Action<bool, string> callback)
        {
            StartCoroutine(IapOps.IsIapBannedRequest(callback));
        }
        
        public void EnsureUser(Action onSuccess, Action<string> onError)
        {
            if (!string.IsNullOrEmpty(userId))
            {
                onSuccess?.Invoke();
                return;
            }
            
            StartCoroutine(CreateOrGetNewUserCoroutine(onSuccess, onError));
        }

        private IEnumerator CreateOrGetNewUserCoroutine(Action onSuccess, Action<string> onError)
        {
            var createNewUserJob = UserOps.CreateOrGetNewUser(response =>
                {
                    if (response.responseCode != 200)
                    {
                        onError?.Invoke($"Failed to get or create user. Response code: {response.responseCode}");
                        return;
                    }

                    var openResponseForNewUser = response.data;
                    this.userId = openResponseForNewUser.user_id;
                    if (ZyngaPublishingElephantAdapter != null)
                    {
                        ZyngaPublishingElephantAdapter.RollicUserId = userId;
                    }
                    onSuccess?.Invoke();
                }, 
                error => 
                {
                    onError?.Invoke($"Error on fetching user id: {error}");
                });

            yield return createNewUserJob;
        }

        public SessionData GetCurrentSession()
        {
            if (currentSession != null) return currentSession;
            ElephantLog.LogError("SESSION", "Current session was null when accessed");
            currentSession = SessionData.CreateSessionData();
            return currentSession;
        }

        public void AddToQueue(ElephantRequest data)
        {
            this._queue.Enqueue(data);
        }

        void OnApplicationFocus(bool focus)
        {
            if (focus)
            {
                var savedTimeSpend = Utils.ReadFromFile(ElephantConstants.TimeSpend);
                
                try
                {
                    timeSpend = !string.IsNullOrEmpty(savedTimeSpend) ? long.Parse(savedTimeSpend) : 0;
                }
                catch (Exception e)
                {
                    ElephantLog.LogError("ELEPHANT", "Error on time spend: " + e.Message);
                }
                
                ElephantLog.LogCustomKey("total_time_spent", timeSpend.ToString());

                currentSession = SessionData.CreateSessionData();


                // Reset real session id and event order if necessary time passes
                // Sometimes unity won't reset game after a long focus-free session
                if (openResponse?.internal_config != null && Time.unscaledDeltaTime - Instance.focusLostTime > openResponse.internal_config.focus_interval)
                {
                    _metaDataUtils = MetaDataUtils.GetInstance();
                    _metaDataUtils.IncrementByOne(MetaDataKeys.KeyTotalSessionCount);
                    _metaDataUtils.IncrementDailySessionCount();
                    _metaDataUtils.FlushSession();
                    Instance.realSessionId = currentSession.GetSessionID();
                    Instance.eventOrder = 0;
                    ElephantLog.LogCustomKey("current_session_count", _metaDataUtils.GetTotalSessionCount().ToString());
                }

                ElephantLog.Log("APP STATE", "Focus Gained");
                // rebuild queues from disk..
                RebuildQueue();

                // start queue processing
                processQueues = true;

#if !UNITY_EDITOR && UNITY_ANDROID
                ElephantAndroid.askForIntent();
#endif
            }
            else
            {
                ElephantLog.Log("APP STATE", "Focus Lost - Flow Start");

                currentSession ??= SessionData.CreateSessionData();
                
                var currentSessionTS = Utils.Timestamp() - currentSession.GetSessionID();
                var totalTS = timeSpend + currentSessionTS;
                Utils.SaveToFile(ElephantConstants.TimeSpend, totalTS.ToString());


                RollicEventUtils.GetInstance().SendTimeSpendEvents(totalTS / 1000 / 60);
                MonitoringUtils.GetInstance().LogBatteryLevel(MonitoringUtils.KeySessionEnd);

                // Time saved for next focus
                Instance.focusLostTime = Time.unscaledDeltaTime;

                // pause late update
                processQueues = false;

                // send session log
                var sessionEndCurrentSession = ElephantCore.Instance.GetCurrentSession();
                sessionEndCurrentSession.RefreshBaseData();
                sessionEndCurrentSession.end_time = Utils.Timestamp();

                var sessionReq = new ElephantRequest(ElephantConstants.SESSION_EP, sessionEndCurrentSession);
                AddToQueue(sessionReq);

                var monitoringReq = new ElephantRequest(ElephantConstants.MONITORING_EP, MonitoringData.CreateMonitoringData());
                AddToQueue(monitoringReq);

                // process queues
                ProcessQueues(true);

                // drain queues and persist them to send after gaining focus
                SaveQueues();

                if (isStorageLoaded)
                {
                    Elephant.SaveStorage();
                }
                
                ElephantLog.Log("APP STATE", "Focus Lost Flow End");
            }
        }
        private void RebuildQueue()
        {
            try
            {
                var json = Utils.ReadFromFile(QUEUE_DATA_FILE);
                if (json == null) return;
                ElephantLog.Log("APP STATE", "QUEUE <- " + json);
                var d = JsonConvert.DeserializeObject<QueueData>(json);
                if (d?.queue == null) return;
                _failedQueue = d.queue;
                foreach (var r in _failedQueue)
                {
                    r.tryCount = 0;
                }
            }
            catch (JsonException ex)
            {
                ElephantLog.LogError("APP STATE", "Failed to deserialize queue data: " + ex.Message);
                Utils.SaveToFile(QUEUE_DATA_FILE, "");
                _failedQueue.Clear();
            }
            catch (Exception ex)
            {
                ElephantLog.LogError("APP STATE", "Error rebuilding queue: " + ex.Message);
                _failedQueue.Clear();
            }
        }

        private void SaveQueues()
        {
            while (_queue.Count > 0)
            {
                ElephantRequest data = _queue.Dequeue();
                _failedQueue.Add(data);
            }

            var queueJson = JsonConvert.SerializeObject(new QueueData(_failedQueue));
            ElephantLog.Log("APP STATE", "QUEUE -> " + queueJson);

            Utils.SaveToFile(QUEUE_DATA_FILE, queueJson);

            _failedQueue.Clear();
        }

        private void LateUpdate()
        {
            if (!sdkIsReady) return;

            ProcessQueues(false);
        }


        private void ProcessQueues(bool forceToSend)
        {
            if (!forceToSend && (!processQueues || !sdkIsReady)) return;
            if (openResponse.internal_config.request_logic_enabled)
            {
                if (processFailedBatch)
                {
                    StartCoroutine(BatchPost());
                }
            }
            else
            {
                int failedCount = _failedQueue.Count;
                for (int i = failedCount - 1; i >= 0; --i)
                {
                    ElephantRequest data = _failedQueue[i];
                    int tc = data.tryCount % 6;
                    int backoff = (int)(Math.Pow(2, tc) * 1000);

                    if (Utils.Timestamp() - data.lastTryTS > backoff)
                    {
                        _failedQueue.RemoveAt(i);
                        StartCoroutine(Post(data));
                    }
                }
            }


            while (_queue.Count > 0)
            {
                ElephantRequest data = _queue.Dequeue();
                StartCoroutine(Post(data));
            }
        }

        IEnumerator BatchPost()
        {
            if (_failedQueue.Count == 0) yield break;
            processFailedBatch = false;

            while (_failedQueue.Count > 0)
            {
                var counter = 0;
                var listCounter = _failedQueue.Count - 1;
                ElephantLog.Log("BatchPost", "start new batch ");

                while (counter < 10 && listCounter >= 0)
                {
                    var request = _failedQueue[listCounter];
                    int tc = request.tryCount % 6;
                    int backoff = (int)(Math.Pow(2, tc) * 1000);


                    if (Utils.Timestamp() - request.lastTryTS > backoff)
                    {
                        _failedQueue.RemoveAt(listCounter);
                        if (!circuitBreakerEnabled)
                        {
                            ElephantLog.Log("BatchPost", "request: " + request.url);
                            ElephantLog.Log("BatchPost", "batch count: " + _failedQueue.Count);
                            StartCoroutine(Post(request));
                        }
                    }

                    counter++;
                    listCounter--;
                }

                ElephantLog.Log("BatchPost", "wait 10: ");
                yield return new WaitForSeconds(3);
            }

            processFailedBatch = true;
        }

        IEnumerator Post(ElephantRequest elephantRequest)
        {
            ElephantLog.Log("POST WITH F&F", elephantRequest.tryCount + " - " +
                                             (Utils.Timestamp() - elephantRequest.lastTryTS) + " -> " +
                                             elephantRequest.url + " : " + elephantRequest.data);

            if (openResponse.internal_config.reachability_check_enabled)
            {
                Elephant.ShowNetworkOfflineDialog();

                var waitTime = startingWaitTime;
                while (!Utils.IsConnected())
                {
                    yield return new WaitForSeconds(waitTime);
                    waitTime = Mathf.Min(waitTime * 2f, maxWaitTime);
                }
            }

            elephantRequest.tryCount++;
            elephantRequest.lastTryTS = Utils.Timestamp();

            var elephantData = new ElephantData(elephantRequest.data, GetCurrentSession().GetSessionID(),
                elephantRequest.isOffline, elephantRequest.statusCode > 0);

            string bodyJsonString = JsonConvert.SerializeObject(elephantData);

            string authToken = Utils.SignString(bodyJsonString, gameSecret);


#if UNITY_EDITOR

            using (var request = new UnityWebRequest(elephantRequest.url, "POST"))
            {
                byte[] bodyRaw = Encoding.UTF8.GetBytes(bodyJsonString);
                request.uploadHandler = new UploadHandlerRaw(bodyRaw);
                request.downloadHandler = new DownloadHandlerBuffer();
                request.SetRequestHeader("Content-Type", "application/json");
                request.SetRequestHeader("Content-Encoding", "gzip");
                request.SetRequestHeader("Authorization", authToken);
                request.SetRequestHeader("GameID", ElephantCore.Instance.gameID);

                yield return request.SendWebRequest();

                ElephantLog.Log("POST WITH F&F", "Status Code: " + request.responseCode);

                if (request.responseCode != 200)
                {
                    // failed will be retried
                    if (_failedQueue.Count < MAX_FAILED_COUNT)
                    {
                        _failedQueue.Add(elephantRequest);
                    }
                    else
                    {
                        ElephantLog.LogError("POST WITH F&F", "Failed Queue size -> " + _failedQueue.Count);
                    }
                }
            }

#else
#if UNITY_IOS
            ElephantIOS.ElephantPost(elephantRequest.url, bodyJsonString, gameID, authToken, elephantRequest.tryCount);
#elif UNITY_ANDROID
            ElephantAndroid.ElephantPost(elephantRequest.url, bodyJsonString, gameID, authToken, elephantRequest.tryCount);
#endif
            yield return null;
#endif
        }

        // Triggered from native plugins
        public void ReferralData(string referralDataJson)
        {
            try
            {
                var param = Params.New().CustomString(referralDataJson);
                Elephant.Event("referralData", -1, param);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        public void FailedRequest(string reqJson)
        {
            try
            {
                var req = JsonConvert.DeserializeObject<ElephantRequest>(reqJson);
                req.lastTryTS = Utils.Timestamp();

                // trick..
                var body = JsonConvert.DeserializeObject<ElephantData>(req.data);
                req.data = body.data;

                if (_failedQueue.Count < MAX_FAILED_COUNT)
                {
                    _failedQueue.Add(req);
                }
                else
                {
                    ElephantLog.Log("POST WITH F&F", "Failed Queue size -> " + _failedQueue.Count);
                }
            }
            catch (Exception e)
            {
                ElephantLog.Log("POST WITH F&F", e.Message);
            }
        }

        void setConsentStatus(string message)
        {
#if UNITY_IOS && !UNITY_EDITOR
            idfa = ElephantIOS.IDFA();
#endif
            triggerConsentResult(message);
            var parameters = Params.New();
            parameters.Set("status", message);
            Elephant.Event("idfa_consent_change", -1, parameters);
            consentStatus = message;
        }

        void sendUiConsentStatus(string message)
        {
            if (message.Equals("denied"))
            {
                triggerConsentResult(message);
            }

            var parameters = Params.New();
            parameters.Set("status", message);
            Elephant.Event("idfa_ui_consent_change", -1, parameters);
            consentStatus = message;
        }

        void triggerConsentResult(string message)
        {
            var parameters = Params.New();
            parameters.Set("status", message);
            Elephant.Event("set_idfa_consent_result", -1, parameters);
            IdfaConsentResult.GetInstance().SetIdfaResultValue(message);
            IdfaConsentResult.GetInstance().SetStatus(IdfaConsentResult.Status.Resolved);

#if UNITY_IOS
            AdjustElephantAdapter?.InitAdjust(ElephantThirdPartyIds.AdjustAppKey,
                RemoteConfig.GetInstance().GetBool("conversion_value_service_enabled", false), OnDeepLink);
            // AdjustConfig config = new AdjustConfig(ElephantThirdPartyIds.AdjustAppKey, AdjustEnvironment.Production);
            // config.setAttributionChangedDelegate(OnAttrChange);
            // if (RemoteConfig.GetInstance().GetBool("conversion_value_service_enabled", false))
            //     config.deactivateSKAdNetworkHandling();
            // Adjust.start(config);

            _elephantComplianceManager.ShowCcpa();
            _elephantComplianceManager.ShowGdprAdConsent();
#endif
        }

        public void UserConsentAction(string userAction)
        {
            switch (userAction)
            {
                case "TOS_ACCEPT":
                    _elephantComplianceManager.SendTosAccept();
                    break;
                case "VPPA_ACCEPT":
                    _elephantComplianceManager.SendVppaAccept();
                    break;
                case "GDPR_AD_CONSENT_AGREE":
                    _elephantComplianceManager.SendGdprAdConsentStatus(true);
                    break;
                case "GDPR_AD_CONSENT_DECLINE":
                    _elephantComplianceManager.SendGdprAdConsentStatus(false);
                    break;
                case "PERSONALIZED_ADS_AGREE":
                    _elephantComplianceManager.SendCcpaStatus(true);
                    break;
                case "PERSONALIZED_ADS_DECLINE":
                    _elephantComplianceManager.SendCcpaStatus(false);
                    break;
                case "CALL_DATA_REQUEST":
                    PinRequest();
                    break;
                case "DELETE_REQUEST_CANCEL":
                    var createNewUserJob = UserOps.CreateOrGetNewUser(response =>
                    {
                        if (response.responseCode != 200) return;

                        var openResponseForNewUser = response.data;
                        this.userId = openResponseForNewUser.user_id;
                        if (ZyngaPublishingElephantAdapter != null)
                        {
                            ZyngaPublishingElephantAdapter.RollicUserId = userId;
                        }

                        _elephantComplianceManager.UpdateOpenResponse(openResponseForNewUser);
                        _elephantComplianceManager.ShowTosAndPp(onOpen);
                    }, s => { ElephantLog.Log("COMPLIANCE", "Error on new user creation: " + s); });
                    StartCoroutine(createNewUserJob);
                    break;
                case "RETRY_CONNECTION":
                    if (Utils.IsConnected())
                    {
                        Utils.ResumeGame();
                    }
                    else
                    {
                        Elephant.ShowNetworkOfflineDialog();
                    }

                    break;
            }
        }

        public void GetSettingsContent(Action<GenericResponse<SettingsResponse>> onResponse, Action<string> onError)
        {
            var data = new BaseData();
            data.FillBaseData(Instance.GetCurrentSession().GetSessionID());
            var json = JsonConvert.SerializeObject(data);
            var bodyJson = JsonConvert.SerializeObject(new ElephantData(json, Instance.GetCurrentSession().GetSessionID()));
            var networkManager = new GenericNetworkManager<SettingsResponse>();
            var postWithResponse = networkManager.PostWithResponse(ElephantConstants.SETTINGS_EP, bodyJson, onResponse, onError);

            StartCoroutine(postWithResponse);
        }
        
        public void SetTCString(string tcString)
        {
#if UNITY_EDITOR
            ElephantLog.Log("USERCENTRICS", "Setting TCString in the editor.");
            return;
#else
            Utils.SaveToFile(ElephantConstants.TC_STRING, tcString);
#endif
        }
        
        public string GetTCString()
        {
#if UNITY_EDITOR
            return "EDITOR";
#else
            return Utils.ReadFromFile(ElephantConstants.TC_STRING);
#endif
        }

        #region Push Notification Methods

        public void ReceiveNotificationPermission(string response)
        {
            PushElephantAdapter?.ReceiveNotificationPermission(response);
        }

        public void SetDeviceToken(string token)
        {
            if (string.IsNullOrEmpty(token) || token == "ERROR")
            {
                ElephantLog.Log("SetDeviceToken", "Device token registration failed");
                return;
            }
            
            PushElephantAdapter?.SetDeviceToken(token);
        }

        public void ReceiveNotificationMessage(string message)
        {
            ElephantLog.Log("ReceiveNotificationMessage", message);
        }

        public void SendPushNotificationOpenEvent(string combinedIds)
        {
            onNotificationOpened?.Invoke();
            PushElephantAdapter?.SendPushNotificationOpenEvent(combinedIds);
        }

        #endregion

        #region Live Ops

        public void ReceiveLocalizedPrice(string concatenatedPrices)
        {
            LiveOpsElephantAdapter?.ReceiveLocalizedPrice(concatenatedPrices);
        }

        public void ReceiveLocalizedPriceError(string err)
        {
            ElephantLog.LogError("OFFER PRICE", err);
        }

        public void ReceiveCollectibleResponse(string msg)
        {
            ElephantStorageManager.GetInstance().ReceiveCollectibleResponse();
            var param = Params.New().CustomString(msg);
            Elephant.Event("collectible_claimed", -1, param);
            Elephant.TriggerCollectibleClaimed();
        }

        public void TriggerOnOfferUIFetched()
        {
            if (onOfferUIFetched != null)
                onOfferUIFetched();
        }

        #endregion

        #region PROTOTYPE
        private long DaysSinceFirstInstall()
        {
            var days = (Utils.Timestamp() - firstInstallTime) / (1000 * 60 * 60 * 24);
            return days;
        }
        public bool CheckAdFreePeriod()
        {
            var adFreeDays = RemoteConfig.GetInstance().GetInt("ad_free_days", 7);
            var daysSinceInstall = DaysSinceFirstInstall();
            var isAdFreePeriod = daysSinceInstall < adFreeDays;

            return isAdFreePeriod;
        }

        #endregion

    }
}