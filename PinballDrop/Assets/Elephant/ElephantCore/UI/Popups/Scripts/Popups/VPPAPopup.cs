using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Globalization;

namespace ElephantSDK
{
    public class VPPAPopup : BasePopup
    {
        [Header("UI References")]
        [SerializeField] private TextMeshProUGUI contentText;
        [SerializeField] private Button acceptButton;
        
        private Action onAcceptCallback;

        public void Initialize(string content, string acceptButtonLabel, Action onAccept)
        {
            ElephantLog.Log("VPPAPopup", "Initializing...");

            if (!string.IsNullOrEmpty(content))
            {
                string gameName = Application.productName;
                if (!string.IsNullOrEmpty(gameName))
                {
                    content = content.Replace("{{name}}", gameName).Replace("{{game_name}}", gameName).Replace("{game_name}", gameName);
                }
            }

            if (contentText != null)
            {
                contentText.text = HyperlinkUtils.CleanText(content);
                ElephantLog.Log("VPPAPopup", $"Content set: {content.Substring(0, Mathf.Min(50, content.Length))}...");
            }
            else
            {
                ElephantLog.LogError("VPPAPopup", "contentText is null!");
            }
            
            if (acceptButton != null)
            {
                var btnText = acceptButton.GetComponentInChildren<TextMeshProUGUI>();
                if (btnText != null) btnText.text = acceptButtonLabel.ToUpper(CultureInfo.InvariantCulture);
            }
            
            this.onAcceptCallback = onAccept;

            ApplyStyle(PopupType.Vppa);
            SetupButtons();
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
            ElephantLog.Log("VPPAPopup", "Accept clicked");
            onAcceptCallback?.Invoke();
            Close();
        }

        public void Close()
        {
            ElephantLog.Log("VPPAPopup", "Closing");
            ElephantPopupManager.Instance.CloseCurrentPopup();
        }
    }
}