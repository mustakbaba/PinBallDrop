using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Globalization;

namespace ElephantSDK
{
    public class NetworkOfflinePopup : BasePopup
    {
        [Header("UI References")]
        [SerializeField] private TextMeshProUGUI contentText;
        [SerializeField] private Button retryButton;
        
        private Action onRetryCallback;

        public void Initialize(string content, string buttonLabel, Action onRetry)
        {
            ElephantLog.Log("NetworkOfflinePopup", "Initializing");

            if (contentText != null)
            {
                contentText.text = HyperlinkUtils.CleanText(content);
                ElephantLog.Log("NetworkOfflinePopup", $"Content set: {content}");
            }
            else
            {
                ElephantLog.LogError("NetworkOfflinePopup", "contentText is null!");
            }

            if (retryButton != null)
            {
                var btnText = retryButton.GetComponentInChildren<TextMeshProUGUI>();
                if (btnText != null) btnText.text = buttonLabel.ToUpper(CultureInfo.InvariantCulture);
            }
            else
            {
                ElephantLog.LogError("NetworkOfflinePopup", "retryButton is null!");
            }
            
            this.onRetryCallback = onRetry;
            ApplyStyle(PopupType.NetworkOffline);
            SetupButton();
        }

        private void SetupButton()
        {
            if (retryButton != null)
            {
                retryButton.onClick.RemoveAllListeners();
                retryButton.onClick.AddListener(OnRetryClicked);
            }
        }

        private void OnRetryClicked()
        {
            ElephantLog.Log("NetworkOfflinePopup", "Retry button clicked");
            bool isConnected = Utils.IsConnected();
            onRetryCallback?.Invoke();

            if (isConnected)
            {
                Close();
            }
        }

        public void Close()
        {
            ElephantLog.Log("NetworkOfflinePopup", "Closing");
            ElephantPopupManager.Instance.CloseCurrentPopup();
        }
    }
}