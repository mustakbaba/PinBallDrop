using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using System;
using System.Collections.Generic;

namespace ElephantSDK
{
    public class TOSPopup : BasePopup
    {
        [Header("UI References")]
        [SerializeField] private TextMeshProUGUI contentText;
        [SerializeField] private Button acceptButton;

        private string tosUrl;
        private string privacyUrl;
        private Action onAcceptCallback;

        public void Initialize(string content, string tosText, string tosUrl,
                              string privacyText, string privacyUrl,
                              string acceptButtonLabel, Action onAccept)
        {
            ElephantLog.Log("TOSPopup", "Initializing...");

            this.tosUrl = tosUrl;
            this.privacyUrl = privacyUrl;
            this.onAcceptCallback = onAccept;

            if (contentText != null)
            {
                // Process hyperlinks in the content
                var hyperlinks = new List<HyperlinkData>
                {
                    new HyperlinkData(HyperlinkUtils.PRIVACY_MASK, privacyText, privacyUrl),
                    new HyperlinkData(HyperlinkUtils.TERMS_MASK, tosText, tosUrl)
                };

                string processedContent = HyperlinkUtils.ProcessHyperlinks(content, hyperlinks);
                contentText.text = HyperlinkUtils.CleanText(processedContent);

                ElephantLog.Log("TOSPopup", "Content set with hyperlinks processed");

                SetupLinkInteraction();
            }
            else
            {
                ElephantLog.LogError("TOSPopup", "contentText is null!");
            }

            if (acceptButton != null)
            {
                var btnText = acceptButton.GetComponentInChildren<TextMeshProUGUI>();
                if (btnText != null) btnText.text = acceptButtonLabel;
			}
            else
            {
                ElephantLog.LogError("TOSPopup", "acceptButton is null!");
            }

            ApplyStyle(PopupType.Tos);
            SetupButtons();
        }

        private void SetupLinkInteraction()
        {
            if (contentText == null) return;

            EventTrigger trigger = contentText.gameObject.GetComponent<EventTrigger>();
            if (trigger == null)
            {
                trigger = contentText.gameObject.AddComponent<EventTrigger>();
            }

            trigger.triggers.Clear();

            EventTrigger.Entry entry = new EventTrigger.Entry();
            entry.eventID = EventTriggerType.PointerClick;
            entry.callback.AddListener((data) => { OnLinkClicked((PointerEventData)data); });
            trigger.triggers.Add(entry);

            ElephantLog.Log("TOSPopup", "Link interaction setup complete");
        }

        private void OnLinkClicked(PointerEventData eventData)
        {
            if (contentText == null) return;

            int linkIndex = TMP_TextUtilities.FindIntersectingLink(contentText, eventData.position, null);
            
            if (linkIndex != -1)
            {
                TMP_LinkInfo linkInfo = contentText.textInfo.linkInfo[linkIndex];
                string url = linkInfo.GetLinkID();
                
                if (!string.IsNullOrEmpty(url))
                {
                    ElephantLog.Log("TOSPopup", $"Link clicked: {url}");
                    HyperlinkUtils.OpenURL(url);
                }
            }
        }

        private void SetupButtons()
        {
            if (acceptButton != null)
            {
                acceptButton.onClick.RemoveAllListeners();
                acceptButton.onClick.AddListener(OnAcceptClicked);
            }
        }

        private void OnAcceptClicked()
        {
            ElephantLog.Log("TOSPopup", "Accept clicked");
            Close();
            onAcceptCallback?.Invoke();
        }

        private void Close()
        {
            ElephantLog.Log("TOSPopup", "Closing");
            ElephantPopupManager.Instance.CloseCurrentPopup();
        }
    }
}