using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Globalization;

namespace ElephantSDK
{
    public class BlockedPopup : BasePopup
    {
        [Header("UI References")]
        [SerializeField] private TextMeshProUGUI contentText;
        [SerializeField] private TextMeshProUGUI warningText;
        [SerializeField] private Button actionButton;
        
        private Action onActionCallback;

        public void Initialize(string content, string warning,
                              string buttonLabel, Action onAction)
        {
            ElephantLog.Log("BLOCKEDPopup", "Initializing...");

            if (contentText != null)
            {
                contentText.text = HyperlinkUtils.CleanText(content);
                ElephantLog.Log("BLOCKEDPopup", $"Content set: {content.Substring(0, Mathf.Min(50, content.Length))}...");
            }
            else
            {
                ElephantLog.LogError("BLOCKEDPopup", "contentText is null!");
            }

            if (warningText != null)
            {
                warningText.text = warning;
                ElephantLog.Log("BLOCKEDPopup", $"Warning set: {warning}");
            }
            else
            {
                ElephantLog.LogError("BLOCKEDPopup", "warningText is null!");
            }
            
            if (actionButton != null)
            {
                var btnText = actionButton.GetComponentInChildren<TextMeshProUGUI>();
                if (btnText != null) btnText.text = buttonLabel.ToUpper(CultureInfo.InvariantCulture);
            }
            
            this.onActionCallback = onAction;

            ApplyStyle(PopupType.Blocked);
            SetupButtons();
        }

        private void SetupButtons()
        {
            if (actionButton != null)
            {
                actionButton.onClick.RemoveAllListeners();
                actionButton.onClick.AddListener(OnActionClicked);
            }
        }

        private void OnActionClicked()
        {
            ElephantLog.Log("BLOCKEDPopup", "Action button clicked");
            onActionCallback?.Invoke();
            Close();
        }

        public void Close()
        {
            ElephantLog.Log("BLOCKEDPopup", "Closing");
            ElephantPopupManager.Instance.CloseCurrentPopup();
        }
    }
}