using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

namespace ElephantSDK
{
    public class AgeBlockedPopup : BasePopup
    {
        [Header("UI References")]
        [SerializeField] private Button termsOfServiceButton;
        [SerializeField] private Button privacyPolicyButton;
        [SerializeField] private Button okButton;

        private string _tosUrl;
        private string _ppUrl;

        private const string Tag = "AgeBlockedPopup";
        
        public void Initialize(string tosUrl, string ppUrl)
        {
            ElephantLog.Log(Tag, "Initializing age blocked popup");

            _tosUrl = tosUrl;
            _ppUrl = ppUrl;

            SetupButtons();

            termsOfServiceButton.gameObject.SetActive(!string.IsNullOrEmpty(_tosUrl));
            privacyPolicyButton.gameObject.SetActive(!string.IsNullOrEmpty(_ppUrl));
            
            ApplyStyle(PopupType.AgeBlocked);
        }

        private void SetupButtons()
        {
            if (termsOfServiceButton != null)
            {
                termsOfServiceButton.onClick.RemoveAllListeners();
                termsOfServiceButton.onClick.AddListener(OnTermsOfServiceClicked);
            }
            else
            {
                ElephantLog.LogError(Tag, "Terms of Service button reference is missing on AgeBlockedPopup prefab");
            }

            if (privacyPolicyButton != null)
            {
                privacyPolicyButton.onClick.RemoveAllListeners();
                privacyPolicyButton.onClick.AddListener(OnPrivacyPolicyClicked);
            }
            else
            {
                ElephantLog.LogError(Tag, "Privacy Policy button reference is missing on AgeBlockedPopup prefab");
            }

            if (okButton != null)
            {
                okButton.onClick.RemoveAllListeners();
                okButton.onClick.AddListener(OnOkClicked);
            }
            else
            {
                ElephantLog.LogError(Tag, "OK button reference is missing on AgeBlockedPopup prefab");
            }
        }

        private void OnTermsOfServiceClicked()
        {
            if (!string.IsNullOrEmpty(_tosUrl))
            {
                ElephantLog.Log(Tag, "Opening Terms of Service");
                HyperlinkUtils.OpenURL(_tosUrl);
            }
        }

        private void OnPrivacyPolicyClicked()
        {
            if (!string.IsNullOrEmpty(_ppUrl))
            {
                ElephantLog.Log(Tag, "Opening Privacy Policy");
                HyperlinkUtils.OpenURL(_ppUrl);
            }
        }

        private void OnOkClicked()
        {
            ElephantLog.Log(Tag, "OK button clicked - quitting application");
            Application.Quit();
        }
    }
}
