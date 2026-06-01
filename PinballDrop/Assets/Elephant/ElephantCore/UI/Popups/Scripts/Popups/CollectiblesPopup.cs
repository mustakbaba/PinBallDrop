using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ElephantSDK
{
    public class CollectiblesPopup : BasePopup
    {
        [Header("UI References")]
        [SerializeField] private TextMeshProUGUI contentText;
        [SerializeField] private Button confirmButton;

        public void Initialize(string message, string buttonLabel)
        {
            if (contentText != null)
            {
                contentText.text = HyperlinkUtils.CleanText(message);
            }
            else
            {
                ElephantLog.LogError("CollectiblesPopup", "contentText is null!");
            }

            if (confirmButton != null)
            {
                var btnText = confirmButton.GetComponentInChildren<TextMeshProUGUI>();
                if (btnText != null)
                {
                    btnText.text = buttonLabel;
                }

                confirmButton.onClick.RemoveAllListeners();
                confirmButton.onClick.AddListener(OnConfirmClicked);
            }
            else
            {
                ElephantLog.LogError("CollectiblesPopup", "confirmButton is null!");
            }

            ApplyStyle(PopupType.Collectibles);
        }

        private void OnConfirmClicked()
        {
            if (confirmButton != null)
            {
                confirmButton.onClick.RemoveAllListeners();
            }

            ElephantCore.Instance?.ReceiveCollectibleResponse("OK");

            if (ElephantStorageManager.GetInstance().GetPendingCollectiblesCount() == 0)
            {
                Close();
            }
        }

        public void Close()
        {
            ElephantPopupManager.Instance.CloseCurrentPopup();
        }
    }
}
