using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.UI;

namespace ElephantSDK
{
    public class LoadingPopup : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private TextMeshProUGUI loadingText;
        [SerializeField] protected Image popupBackgroundImage;

        private Coroutine _autoCloseRoutine;

        public void Initialize(string message = "Loading...", float timeoutSeconds = 5f)
        {
            ElephantLog.Log("LoadingPopup", "Showing loading");

            if (loadingText != null)
            {
                loadingText.text = message;
            }
            
            if (timeoutSeconds > 0f)
            {
                if (_autoCloseRoutine != null)
                {
                    StopCoroutine(_autoCloseRoutine);
                }
                _autoCloseRoutine = StartCoroutine(AutoCloseAfterDelay(timeoutSeconds));
            }
            
            PopupStyleApplier.ApplyBackground(PopupType.Loading, popupBackgroundImage);
        }

        private IEnumerator AutoCloseAfterDelay(float delay)
        {
            yield return new WaitForSeconds(delay);
            ElephantLog.Log("LoadingPopup", $"Auto-closing after {delay} seconds");
            Close();
        }

        public void Close()
        {
            if (_autoCloseRoutine != null)
            {
                StopCoroutine(_autoCloseRoutine);
                _autoCloseRoutine = null;
            }

            ElephantLog.Log("LoadingPopup", "Closing");
            ElephantPopupManager.Instance.CloseCurrentPopup();
        }

        private void OnDisable()
        {
            if (_autoCloseRoutine != null)
            {
                StopCoroutine(_autoCloseRoutine);
                _autoCloseRoutine = null;
            }
        }
    }
}