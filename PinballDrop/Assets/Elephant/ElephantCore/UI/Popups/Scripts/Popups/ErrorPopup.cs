using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Globalization;

namespace ElephantSDK
{
    public class ErrorPopup : BasePopup
    {
        [Header("UI References")]
        [SerializeField] private TextMeshProUGUI contentText;
        [SerializeField] private Button okButton;
        
        private Action onOkCallback;

        public void Initialize(string errorMessage, string buttonLabel, Action onOk = null)
        {
            ElephantLog.Log("ErrorPopup", "Initializing with error message");

            if (contentText != null)
            {
                contentText.text = HyperlinkUtils.CleanText(errorMessage);
                ElephantLog.Log("ErrorPopup", $"Error message set: {errorMessage}");
            }
            else
            {
                ElephantLog.LogError("ErrorPopup", "contentText is null!");
            }

            if (okButton != null)
            {
                var btnText = okButton.GetComponentInChildren<TextMeshProUGUI>();
                if (btnText != null) btnText.text = buttonLabel.ToUpper(CultureInfo.InvariantCulture);
            }
            else
            {
                ElephantLog.LogError("ErrorPopup", "okButton is null!");
            }
            
            this.onOkCallback = onOk;
            ApplyStyle(PopupType.Error);
            SetupButton();
        }

        private void SetupButton()
        {
            if (okButton != null)
            {
                okButton.onClick.RemoveAllListeners();
                okButton.onClick.AddListener(OnOkClicked);
            }
        }

        private void OnOkClicked()
        {
            ElephantLog.Log("ErrorPopup", "OK button clicked");
            onOkCallback?.Invoke();
            Close();
        }

        public void Close()
        {
            ElephantLog.Log("ErrorPopup", "Closing");
            ElephantPopupManager.Instance.CloseCurrentPopup();
        }
    }
}