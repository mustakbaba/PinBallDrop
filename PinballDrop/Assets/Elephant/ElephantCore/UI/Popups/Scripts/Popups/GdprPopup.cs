using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Globalization;

namespace ElephantSDK
{
    public class GdprPopup : BasePopup
    {
        [Header("UI References")]
        [SerializeField] private TextMeshProUGUI contentText;
        [SerializeField] private Button agreeButton;
        [SerializeField] private Button declineButton;
        [SerializeField] private Button cancelButton;

        private Action _onAgree;
        private Action _onDecline;

        public void Initialize(
            string title,
            string content,
            string privacyPolicyText,
            string privacyPolicyUrl,
            string declineButtonText,
            string agreeButtonText,
            string cancelButtonText,
            Action onAgree,
            Action onDecline)
        {
            ElephantLog.Log("GdprPopup", "Initializing GDPR ads consent popup");

            _onAgree = onAgree;
            _onDecline = onDecline;

            SetupContent(content, privacyPolicyText, privacyPolicyUrl);
            SetupButtons(declineButtonText, agreeButtonText, cancelButtonText);

            ApplyStyle(PopupType.Gdpr);
            ApplyDeclineSecondaryStyle();
        }

        private void ApplyDeclineSecondaryStyle()
        {
            if (declineButton == null)
            {
                return;
            }

            var style = PopupUIStyleConfig.GetStyle(PopupType.Gdpr);
            if (style?.layout == null)
            {
                return;
            }

            if (!PopupStyleApplier.TryParseHexColor(style.layout.buttonSecondary, out var color))
            {
                return;
            }

            var image = declineButton.targetGraphic as Image ?? declineButton.GetComponent<Image>();
            if (image != null)
            {
                image.color = color;
            }
        }

        private void SetupContent(string content, string privacyPolicyText, string privacyPolicyUrl)
        {
            if (contentText == null)
            {
                ElephantLog.LogError("GdprPopup", "contentText is null!");
                return;
            }

            var hyperlinks = new List<HyperlinkData>
            {
                new HyperlinkData(HyperlinkUtils.PRIVACY_MASK, privacyPolicyText, privacyPolicyUrl)
            };

            string processedContent = HyperlinkUtils.ProcessHyperlinks(content, hyperlinks);
            contentText.text = HyperlinkUtils.CleanText(processedContent);

            SetupLinkInteraction();
        }

        private void SetupButtons(string declineButtonText, string agreeButtonText, string cancelButtonText)
        {
            BindButton(agreeButton, agreeButtonText, OnAgreeClicked);
            BindButton(declineButton, declineButtonText, OnDeclineClicked);
            BindButton(cancelButton, cancelButtonText, Close);
        }

        private void SetupLinkInteraction()
        {
            if (contentText == null) return;

            var trigger = contentText.gameObject.GetComponent<EventTrigger>();
            if (trigger == null)
            {
                trigger = contentText.gameObject.AddComponent<EventTrigger>();
            }

            trigger.triggers.Clear();

            var entry = new EventTrigger.Entry { eventID = EventTriggerType.PointerClick };
            entry.callback.AddListener(data => OnLinkClicked((PointerEventData)data));
            trigger.triggers.Add(entry);
        }

        private void OnLinkClicked(PointerEventData eventData)
        {
            if (contentText == null) return;

            int linkIndex = TMP_TextUtilities.FindIntersectingLink(contentText, eventData.position, null);
            if (linkIndex == -1) return;

            TMP_LinkInfo linkInfo = contentText.textInfo.linkInfo[linkIndex];
            string url = linkInfo.GetLinkID();
            if (string.IsNullOrEmpty(url)) return;

            HyperlinkUtils.OpenURL(url);
        }

        private void OnAgreeClicked()
        {
            try
            {
                _onAgree?.Invoke();
            }
            catch (Exception e)
            {
                ElephantLog.LogError("GdprPopup", e.Message);
            }
            finally
            {
                Close();
            }
        }

        private void OnDeclineClicked()
        {
            try
            {
                _onDecline?.Invoke();
            }
            catch (Exception e)
            {
                ElephantLog.LogError("GdprPopup", e.Message);
            }
            finally
            {
                Close();
            }
        }

        private static void BindButton(Button button, string label, Action onClick)
        {
            if (button == null) return;

            var btnText = button.GetComponentInChildren<TextMeshProUGUI>();
            if (btnText != null && !string.IsNullOrEmpty(label))
            {
                btnText.text = label.ToUpper(CultureInfo.InvariantCulture);
            }

            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => onClick?.Invoke());
        }

        private static void Close()
        {
            ElephantPopupManager.Instance.CloseCurrentPopup();
        }
    }
}

