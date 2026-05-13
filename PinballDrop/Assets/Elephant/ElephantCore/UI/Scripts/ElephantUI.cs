using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ElephantSDK
{
    public class ElephantUI : MonoBehaviour
    {
        private GameObject loaderUI;
        private static float _startTime;

        public static ElephantUI Instance;

        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            Application.targetFrameRate = 60;
            Init();
        }


        public void Init()
        {
            _startTime = Time.time;
            loaderUI = GameObject.Find("loader");

            ShowLoaderUI();

            ElephantCore.onInitialized += OnInitialized;
            ElephantCore.onOpen += OnOpen;
            ElephantCore.onRemoteConfigLoaded += OnRemoteConfigLoaded;
            

            var isOldUser = false;
            Elephant.Init(isOldUser, true);
        }

        private void OnInitialized()
        {
            ElephantLog.Log("INIT", "Elephant Initialized");
        }

        private void OnOpen(bool gdprRequired, ComplianceTosResponse tos)
        {
            Debug.Log("Elephant Open Result, we can start the game or show gdpr -> " + gdprRequired);
            if (gdprRequired)
            {
#if UNITY_EDITOR
                // no-op
                ElephantLog.Log("ELEPHANT-FLOW", "2. ElephantUI - ONOPEN - EDITOR - GDPR IS REQUIRED");
                LoadGameSceneWithCmp();
                ElephantLog.Log("COMPLIANCE", "ShowToSAndPPDialog");
#elif UNITY_ANDROID
                ElephantLog.Log("ELEPHANT-FLOW", "2. ElephantUI - ONOPEN - ANDROID - GDPR IS REQUIRED");
                 ElephantAndroid.ShowConsentDialogOnUiThread("CONTENT", tos.content, tos.consent_text_action_button, tos.privacy_policy_text,
                    tos.privacy_policy_url, tos.terms_of_service_text, tos.terms_of_service_url);
#elif UNITY_IOS
                ElephantLog.Log("ELPHANT-FLOW", "2. ElephantUI - ONOPEN - IOS - GDPR IS REQUIRED");
                ElephantIOS.showPopUpView("CONTENT", tos.content, tos.consent_text_action_button, tos.privacy_policy_text,
                    tos.privacy_policy_url, tos.terms_of_service_text, tos.terms_of_service_url);
#else
                // no-op
#endif
            }
            else
            {
#if UNITY_EDITOR
                // no-op
                ElephantLog.Log("ELEPHANT-FLOW", "2. ElephantUI - ONOPEN - EDITOR - GDPR IS NOT REQUIRED");
                LoadGameSceneWithCmp();
                ElephantLog.Log("COMPLIANCE", "ShowToSAndPPDialog");
#elif UNITY_ANDROID
                ElephantLog.Log("ELEPHANT-FLOW", "2. ElephantUI - ONOPEN - ANDROID - GDPR IS NOT REQUIRED");
                LoadGameSceneWithCmp();
#elif UNITY_IOS
                // no-op
#else
                // no-op
#endif
            }
        }

        private void OnRemoteConfigLoaded()
        {
            Debug.Log(
                "Elephant Remote Config Loaded, we can retrieve configuration params via RemoteConfig.GetInstance().Get() or other variant methods..");
        }


        private void ShowLoaderUI()
        {
            loaderUI.SetActive(true);
        }

        public void StartIDFAListener()
        {
            StartCoroutine(CheckIdfaStatus());
        }
        
        private IEnumerator CheckIdfaStatus()
        {
            ElephantLog.Log("ELEPHANT-FLOW", "X. ELEPHANTUI - CheckIdfaStatus");
            var startTime = Time.time;
            while (IdfaConsentResult.GetInstance().GetStatus() == IdfaConsentResult.Status.Waiting && (Time.time - startTime) < 3f)
            {
                yield return null;
            }
            ElephantLog.Log("ELEPHANT-FLOW", "XX. ELEPHANTUI - CheckIdfaStatus: " + IdfaConsentResult.GetInstance().GetStatus());
            PlayGame();
        }

        public void PlayGame()
        {
#if UNITY_IOS && !UNITY_EDITOR
            if (RemoteConfig.GetInstance().GetBool("gamekit_ads_enabled", false))
            {
                ElephantCore.Instance.ElephantAdsAdapter?.StartAdManager();
            }

            ElephantLog.Log("ELEPHANT-FLOW", "3. ELEPHANTUI - PlayGame - IOS");
            LoadGameSceneWithCmp(IdfaConsentResult.GetInstance().GetIdfaResultValue().ToLower());
#elif UNITY_ANDROID && !UNITY_EDITOR
            ElephantLog.Log("ELEPHANT-FLOW", "3. ELEPHANTUI - PlayGame - ANDROID");
            LoadGameSceneWithCmp();
#else 
            ElephantLog.Log("ELEPHANT-FLOW", "3. ELEPHANTUI - PlayGame - EDITOR");
            LoadGameScene();
#endif
        }

        public void LoadGameSceneWithCmp(string idfaResult = null)
        {
#if UNITY_EDITOR
            ElephantLog.Log("ELEPHANT-FLOW", "4. ELEPHANTUI - LoadGameSceneWithCmp - EDITOR");
             LoadGameScene();
             return;
#else
            bool isAutoDeny;
            bool shouldInitWithDelay;
            switch (idfaResult)
            {
                case null:
                case "authorized":
                    isAutoDeny = false;
                    shouldInitWithDelay = false;
                    break;
                default:
                {
                    var shouldAutoDenyCmp = RemoteConfig.GetInstance().GetBool("is_auto_deny_enabled", true);
                    isAutoDeny = shouldAutoDenyCmp;
                    var delay = RemoteConfig.GetInstance().GetInt("cmp_delay_hour", 24);
                    shouldInitWithDelay = delay != 0;
                    break;
                }
            }
            
            var isUcEnabled = ElephantCore.Instance.isUcEnabled;
            var isEea = Utils.IsEeaCountry(ElephantCore.Instance.GetOpenResponse().user_country);
            var isUcForced = RemoteConfig.GetInstance().GetBool("usercentrics_forced", false);
            
            var usercentricsElephantAdapter = ElephantCore.Instance.UsercentricsElephantAdapter;
            if (usercentricsElephantAdapter == null)
            {
                ElephantLog.Log("ELEPHANT-FLOW", "4. ELEPHANTUI - LoadGameSceneWithCmp - USERCENTRICS NULL");
                ElephantLog.LogError("USERCENTRICS-ELEPHANT", "UsercentricsElephantAdapter IS NULL");
                LoadGameScene();
                return;
            }

            usercentricsElephantAdapter.InitializeUc(isUcEnabled, isEea, isUcForced, isAutoDeny, shouldInitWithDelay,(isInitialized, consentStatus) =>
            {
                if (isInitialized)
                {
                    var paramsForIsCmpReady = Params.New().Set("shouldCollectConsent", consentStatus.ToString());
                    Elephant.Event("cmp_popup_ready_to_display", -1, paramsForIsCmpReady);

                    if (consentStatus) return;
                    SetAdjustThirdPartySharingOptions();
                    ElephantLog.Log("ELEPHANT-FLOW", "4. ELEPHANTUI - LoadGameSceneWithCmp - IOS - ISINITIALIZED TRUE");
                    LoadGameScene();
                }
                else
                {
                    SetAdjustThirdPartySharingOptions();
                    ElephantLog.Log("ELEPHANT-FLOW", "4. ELEPHANTUI - LoadGameSceneWithCmp - IOS - ISINITIALIZED FALSE");
                    LoadGameScene();
                }
            }, SetAdjustThirdPartySharingOptions, LoadGameScene);
#endif
        }

        private static void SetAdjustThirdPartySharingOptions()
        {
            var isEea = Utils.IsEeaCountry(ElephantCore.Instance.GetOpenResponse().user_country);
            var usercentricsElephantAdapter = ElephantCore.Instance.UsercentricsElephantAdapter;
            
            if (usercentricsElephantAdapter == null)
            {
                ElephantLog.LogError("USERCENTRICS-ELEPHANT", "UsercentricsElephantAdapter IS NULL");
                return;
            }
            
            var isConsentModeV2Enabled = RemoteConfig.GetInstance().GetBool("consent_mode_v2_enabled", false);
            if (usercentricsElephantAdapter.DidAdjustConsentSet() && isConsentModeV2Enabled)
            {
                ElephantCore.Instance.AdjustElephantAdapter?.SetTrackThirdPartySharing(isEea, usercentricsElephantAdapter.GetAdjustConsentStatus(), usercentricsElephantAdapter.GetAdjustConsentStatus());
            }
            else
            {
                ElephantCore.Instance.AdjustElephantAdapter?.SetTrackThirdPartySharing(isEea);
            }
        }

        private static void LoadGameScene()
        {
            ElephantLog.Log("ELEPHANT-FLOW", "END. ELEPHANTUI - LoadGameScene");
#if UNITY_EDITOR
            Debug.Log("You cannot initialize AdManager in Editor mode");
#else
            ElephantCore.Instance.PushElephantAdapter?.AskPushPermission();
#endif
            ElephantCore.Instance.StartCoroutine(LoadGameSceneCoroutine());
        }
        
        private static IEnumerator LoadGameSceneCoroutine()
        {
            float requiredTime = RemoteConfig.GetInstance().GetFloat("elephant_minimum_time_to_load_gamescene", 1.5f);
            while (Time.time - _startTime <= requiredTime)
            {
                yield return null;
            }

            ElephantLog.Log("LOAD", "Loading Game Scene");
            SceneManager.LoadScene(1);
        }
    }
}