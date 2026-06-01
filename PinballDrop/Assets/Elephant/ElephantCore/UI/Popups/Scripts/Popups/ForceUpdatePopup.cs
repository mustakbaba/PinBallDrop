using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections;
using System.Globalization;

namespace ElephantSDK
{
    public class ForceUpdatePopup : BasePopup
    {
        [Header("UI References")]
        [SerializeField] private TextMeshProUGUI contentText;
        [SerializeField] private TextMeshProUGUI userIDText;
        [SerializeField] private Button updateButton;
        [SerializeField] private Button helpshiftButton;

        private const string Tag = "UpdatePopup";
        
        public void Initialize(string content, string buttonLabel, string helpshiftLabel, bool helpshiftEnabled = false)
        {
            ElephantLog.Log(Tag, "Initializing force update popup...");
            
            if (contentText != null)
            {
                contentText.text = HyperlinkUtils.CleanText(content);
                ElephantLog.Log(Tag, $"Content set: {content}");
            }
            else
            {
                ElephantLog.LogError(Tag, "contentText is null!");
            }
            
            if (updateButton != null)
            {
                var btnText = updateButton.GetComponentInChildren<TextMeshProUGUI>();
                if (btnText != null) btnText.text = buttonLabel.ToUpper(CultureInfo.InvariantCulture);
            }

            if (helpshiftButton != null)
            {
                var btnText = helpshiftButton.GetComponent<TextMeshProUGUI>();
                if (btnText != null) btnText.text = helpshiftLabel;
            }

            SetupButtons(helpshiftEnabled);

			if (userIDText != null)
			{
				userIDText.text = $"UserID: {ElephantCore.Instance.userId}";
			}
            
            Elephant.OnApplicationFocusTrue += CheckUpdateOnFocus;
            ApplyStyle(PopupType.ForceUpdate);
        }

        private void SetupButtons(bool helpshiftEnabled)
        {
            if (updateButton != null)
            {
                updateButton.onClick.RemoveAllListeners();
                updateButton.onClick.AddListener(OnUpdateClicked);
            }
            
            if (helpshiftButton != null)
            {
                if (helpshiftEnabled)
                {
                    helpshiftButton.gameObject.SetActive(true);
                    helpshiftButton.onClick.RemoveAllListeners();
                    helpshiftButton.onClick.AddListener(OnHelpshiftClicked);
                }
                else
                {
                    helpshiftButton.gameObject.SetActive(false);
                }
            }
        }

        private void OnUpdateClicked()
        {
            ElephantLog.Log(Tag, "Update button clicked - Opening app store...");
            
            var param = Params.New().Set("version_seen", Application.version);
            Elephant.Event("force_update_button_click", -1, param);
            
            OpenAppStore();
        }
        
        private void OnHelpshiftClicked()
        {
            ElephantLog.Log(Tag, "Helpshift button clicked");
            
            var param = Params.New().Set("version_seen", Application.version);
            Elephant.Event("force_update_helpshift_click", -1, param);
            
            Elephant.ShowFAQ();
        }
        
        private void CheckUpdateOnFocus()
        {
            var complianceManager = ElephantCore.Instance?.ElephantComplianceManager;
			if (complianceManager != null && complianceManager.IsForceUpdateNeeded())
			{
				ElephantLog.Log(Tag, "Update not completed. Keeping popup open");
			}
        }
        
        private void OnDestroy()
        {
            Elephant.OnApplicationFocusTrue -= CheckUpdateOnFocus;
        }

        private void OpenAppStore()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            OpenAndroidStore();
#elif UNITY_IOS && !UNITY_EDITOR
            StartCoroutine(OpeniOSStore());
#else
            ElephantLog.Log(Tag, "Force update not available in editor");
#endif
        }

#if UNITY_ANDROID && !UNITY_EDITOR
        private void OpenAndroidStore()
        {
            string packageName = Application.identifier;
            
            try
            {
                string marketUrl = $"market://details?id={packageName}";
                Application.OpenURL(marketUrl);
                ElephantLog.Log(Tag, $"Opened Play Store with market URL: {marketUrl}");
            }
            catch (Exception e)
            {
                string webUrl = $"https://play.google.com/store/apps/details?id={packageName}";
                Application.OpenURL(webUrl);
                ElephantLog.Log(Tag, $"Opened Play Store in browser: {webUrl}");
                ElephantLog.Log(Tag, $"Market URL failed: {e.Message}");
            }
        }
#endif

#if UNITY_IOS && !UNITY_EDITOR
        private IEnumerator OpeniOSStore()
        {
            string bundleId = Application.identifier;
            string lookupUrl = $"https://itunes.apple.com/lookup?bundleId={bundleId}";
            
            ElephantLog.Log(Tag, $"Looking up App Store ID for bundle: {bundleId}");
            
            using (UnityEngine.Networking.UnityWebRequest request = 
                   UnityEngine.Networking.UnityWebRequest.Get(lookupUrl))
            {
                yield return request.SendWebRequest();

                if (request.result == UnityEngine.Networking.UnityWebRequest.Result.Success)
                {
                    try
                    {
                        string jsonResponse = request.downloadHandler.text;
                        ElephantLog.Log(Tag, $"iTunes lookup response: {jsonResponse}");
                        
                        var jsonData = Newtonsoft.Json.JsonConvert.DeserializeObject<iTunesLookupResponse>(jsonResponse);
                        
                        if (jsonData != null && jsonData.resultCount > 0 && jsonData.results != null && jsonData.results.Length > 0)
                        {
                            string trackId = jsonData.results[0].trackId;
                            string appStoreUrl = $"https://apps.apple.com/app/id{trackId}";
                            
                            ElephantLog.Log(Tag, $"Opening App Store with trackId: {trackId}");
                            Application.OpenURL(appStoreUrl);
                        }
                        else
                        {
                            ElephantLog.LogError(Tag, "No results found in iTunes lookup response");
                            Application.OpenURL($"https://apps.apple.com/app/{bundleId}");
                        }
                    }
                    catch (Exception e)
                    {
                        ElephantLog.LogError(Tag, $"Error parsing iTunes response: {e.Message}");
                        Application.OpenURL($"https://apps.apple.com/app/{bundleId}");
                    }
                }
                else
                {
                    ElephantLog.LogError(Tag, $"iTunes lookup failed: {request.error}");
                    Application.OpenURL($"https://apps.apple.com/app/{bundleId}");
                }
            }
        }
        
        [System.Serializable]
        private class iTunesLookupResponse
        {
            public int resultCount;
            public iTunesAppInfo[] results;
        }

        [System.Serializable]
        private class iTunesAppInfo
        {
            public string trackId;
        }
#endif
    }
}