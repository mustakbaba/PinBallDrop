using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ElephantSDK
{
    public class AlertPopup : BasePopup
    {
        [Header("UI References")]
        [SerializeField] private TextMeshProUGUI contentText;
        [SerializeField] private Button okButton;

        public void Initialize(string title, string message)
        {
            if (titleTexts != null && titleTexts.Length > 0 && titleTexts[0] != null)
            {
                titleTexts[0].text = HyperlinkUtils.CleanText(title ?? string.Empty);
            }

            if (contentText != null)
            {
                contentText.text = HyperlinkUtils.CleanText(message ?? string.Empty);
            }
            else
            {
                ElephantLog.LogError("AlertPopup", "contentText is null!");
            }

            if (okButton != null)
            {
                okButton.onClick.RemoveAllListeners();
                okButton.onClick.AddListener(OnOkClicked);
            }
            else
            {
                ElephantLog.LogError("AlertPopup", "okButton is null!");
            }

            ApplyStyle(PopupType.Alert);
        }

        private void OnOkClicked()
        {
            if (okButton != null)
            {
                okButton.onClick.RemoveAllListeners();
            }

            Close();
        }

        public void Close()
        {
            ElephantPopupManager.Instance.CloseCurrentPopup();
        }
    }
}
