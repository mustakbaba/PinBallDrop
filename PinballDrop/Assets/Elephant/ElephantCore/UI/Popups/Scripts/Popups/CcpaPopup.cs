using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace ElephantSDK
{
    public class CcpaPopup : BasePopup
    {
        [Header("UI References")]
        [SerializeField] private TextMeshProUGUI contentText;
        [SerializeField] private Toggle consentCheckbox;
        [SerializeField] private Button submitButton;

        private Action onAgreeCallback;
        private Action onDeclineCallback;
        private string privacyPolicyUrlValue;
        
        public void Initialize(string title, string content,
                              string privacyPolicyText, string privacyPolicyUrl,
                              string declineButtonText, string agreeButtonText,
                              string backButtonText,
                              Action onAgree, Action onDecline)
        {
            ElephantLog.Log("CcpaPopup", "Initializing personalized ads popup");

            this.onAgreeCallback = onAgree;
            this.onDeclineCallback = onDecline;
            privacyPolicyUrlValue = privacyPolicyUrl;

            if (contentText != null)
            {
                var hyperlinks = new List<HyperlinkData>
                {
                    new HyperlinkData(HyperlinkUtils.PRIVACY_MASK, privacyPolicyText, privacyPolicyUrl)
                };

                string processedContent = HyperlinkUtils.ProcessHyperlinks(content, hyperlinks);
                contentText.text = HyperlinkUtils.CleanText(processedContent);

                ElephantLog.Log("CcpaPopup", "Content set with hyperlinks processed");

                SetupLinkInteraction();
            }
            else
            {
                ElephantLog.LogError("CcpaPopup", "contentText is null!");
            }

            if (consentCheckbox != null)
            {
                consentCheckbox.isOn = true;
                consentCheckbox.onValueChanged.RemoveAllListeners();
                consentCheckbox.onValueChanged.AddListener(OnCheckboxChanged);
            }
            else
            {
                ElephantLog.LogError("CcpaPopup", "consentCheckbox is null!");
            }

            if (submitButton != null)
            {
                var btnText = submitButton.GetComponentInChildren<TextMeshProUGUI>();
                if (btnText != null) btnText.text = "SAVE";
                
                submitButton.onClick.RemoveAllListeners();
                submitButton.onClick.AddListener(OnSubmitClicked);
            }
            else
            {
                ElephantLog.LogError("CcpaPopup", "submitButton is null!");
            }

            ApplyStyle(PopupType.Ccpa);
        }

        private void SetupLinkInteraction()
        {
            if (contentText == null) return;

            var existingTrigger = contentText.gameObject.GetComponent<EventTrigger>();
            if (existingTrigger != null)
            {
                Destroy(existingTrigger);
            }

            var handler = contentText.gameObject.GetComponent<TMPLinkClickHandler>();
            if (handler == null)
            {
                handler = contentText.gameObject.AddComponent<TMPLinkClickHandler>();
            }
            handler.Initialize(contentText, null, url =>
            {
                ElephantLog.Log("CcpaPopup", $"Privacy policy link clicked: {url}");
                HyperlinkUtils.OpenURL(url);
            });

            ElephantLog.Log("CcpaPopup", "Link interaction setup complete");
        }

        private void OnCheckboxChanged(bool isChecked)
        {
            ElephantLog.Log("CcpaPopup", $"Checkbox changed to: {isChecked}");
        }

        private void OnPrivacyPolicyClicked()
        {
            if (string.IsNullOrEmpty(privacyPolicyUrlValue))
            {
                ElephantLog.Log("CcpaPopup", "Privacy policy URL is empty");
                return;
            }

            HyperlinkUtils.OpenURL(privacyPolicyUrlValue);
        }

        private void OnSubmitClicked()
        {
            try
            {
                if (consentCheckbox != null && consentCheckbox.isOn)
                {
                    ElephantLog.Log("CcpaPopup", "User AGREED to personalized ads");
                    onAgreeCallback?.Invoke();
                }
                else
                {
                    ElephantLog.Log("CcpaPopup", "User DECLINED personalized ads");
                    onDeclineCallback?.Invoke();
                }
            }
            catch (Exception e)
            {
                ElephantLog.LogError("CcpaPopup", $"{e.Message}");
            }
            finally
            {
                ElephantPopupManager.Instance.CloseCurrentPopup();
            }
        }
    }
}
