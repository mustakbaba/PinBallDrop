using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace ElephantSDK
{
    public class SettingsPopup : BasePopup
    {
        [Header("UI References")] 
        [SerializeField] private TextMeshProUGUI elephantIdText;
        [SerializeField] private Transform actionButtonsContainer;
        [SerializeField] private Button closeButton;
        [SerializeField] private Button elephantIdButton;
        [SerializeField] private TextMeshProUGUI copyFeedbackText;

        [Header("Prefabs")] 
        [SerializeField] private GameObject actionButtonPrefab;

        private SettingsResponse settingsData;
        private string elephantId;
        private bool showCmpButton;
        private Action onCmpButtonClicked;
        private List<GameObject> spawnedButtons = new List<GameObject>();

        public void Initialize(SettingsResponse settings, string elephantId,
            bool showCmpButton, Action onCmpClicked = null)
        {
            ElephantLog.Log("SETTINGSPopup", "Initializing settings popup");

            if (settings == null)
            {
                ElephantLog.LogError("SETTINGSPopup", "Settings data is null!");
                return;
            }

            this.settingsData = settings;
            this.elephantId = elephantId;
            this.showCmpButton = showCmpButton;
            this.onCmpButtonClicked = onCmpClicked;

            SetupUI();
            PopulateActions();
            ApplyStyle(PopupType.Settings);
        }

        private void SetupUI()
        {
            if (closeButton != null)
            {
                closeButton.onClick.RemoveAllListeners();
                closeButton.onClick.AddListener(OnCloseClicked);
            }

            if (elephantIdButton != null && !string.IsNullOrEmpty(elephantId))
            {
                elephantIdButton.onClick.RemoveAllListeners();
                elephantIdButton.onClick.AddListener(OnElephantIdClicked);
                ApplyPrimaryStyleToDynamicButton(elephantIdButton);
                
                if (elephantIdText != null)
                {
                    elephantIdText.text = elephantId;
                }
            }
        }

        private void PopulateActions()
        {
            if (settingsData?.actions == null || settingsData.actions.Length == 0)
            {
                ElephantLog.Log("SETTINGSPopup", "No actions to display");
                return;
            }

            ClearActionButtons();

            foreach (var action in settingsData.actions)
            {
                CreateActionButton(action);
            }
            
            if (showCmpButton && onCmpButtonClicked != null)
            {
                CreateCmpButton();
            }
        }

        private void CreateActionButton(SettingsActionResponse action)
        {
            GameObject buttonObj = Instantiate(actionButtonPrefab, actionButtonsContainer);
            spawnedButtons.Add(buttonObj);

            var textComponent = buttonObj.GetComponentInChildren<TextMeshProUGUI>();
            if (textComponent != null) textComponent.text = action.title ?? "Unknown Action";

            var button = buttonObj.GetComponent<Button>();
            if (button != null)
            {
                ApplyPrimaryStyleToDynamicButton(button);
                button.onClick.AddListener(() => OnActionButtonClicked(action));
            }

            ElephantLog.Log("SETTINGSPopup", $"Created action button: {action.title}");
        }

        private void CreateCmpButton()
        {
            if (actionButtonPrefab == null)
            {
                ElephantLog.LogError("SETTINGSPopup", "Cannot create CMP button - actionButtonPrefab is null");
                return;
            }
            
            GameObject cmpButtonObj = Instantiate(actionButtonPrefab, actionButtonsContainer);
            spawnedButtons.Add(cmpButtonObj);

            var textComponent = cmpButtonObj.GetComponentInChildren<TextMeshProUGUI>();
            if (textComponent != null) textComponent.text = "Consent Settings";

            var button = cmpButtonObj.GetComponent<Button>();
            if (button != null)
            {
                ApplyPrimaryStyleToDynamicButton(button);
                button.onClick.AddListener(OnCmpButtonClicked);
            }

            ElephantLog.Log("SETTINGSPopup", "Created CMP button as last item");
        }

        private void OnActionButtonClicked(SettingsActionResponse action)
        {
            ElephantLog.Log("SETTINGSPopup", $"Action clicked: {action.title} ({action.action})");

            if (string.IsNullOrEmpty(action.action))
            {
                ElephantLog.LogError("SETTINGSPopup", "Action type is null or empty");
                return;
            }

            switch (action.action.ToUpper())
            {
                case "URL":
                    HandleUrlAction(action);
                    break;

                case "DATA_REQUEST":
                    HandleDataRequestAction();
                    break;

                case "CCPA":
                    HandleCcpaAction(action);
                    break;

                case "GDPR_AD_CONSENT":
                    HandleGdprAdConsentAction(action);
                    break;

                case "CUSTOM_POPUP":
                    HandleCustomPopupAction(action);
                    break;

                default:
                    ElephantLog.Log($"SETTINGSPopup", $"Unknown action type: {action.action}");
                    break;
            }
        }

        private void HandleUrlAction(SettingsActionResponse action)
        {
            try
            {
                var payload = JsonConvert.DeserializeObject<URLPayload>(action.payload);

                if (payload != null && !string.IsNullOrEmpty(payload.url))
                {
                    ElephantLog.Log("SETTINGSPopup", $"Opening URL: {payload.url}");
                    Application.OpenURL(payload.url);
                }
                else
                {
                    ElephantLog.LogError("SETTINGSPopup", "URL payload is invalid");
                }
            }
            catch (Exception e)
            {
                ElephantLog.LogError("SETTINGSPopup", $"Failed to parse URL payload: {e.Message}");
            }
        }
        
        private void HandleDataRequestAction()
        {
            ElephantLog.Log("SETTINGSPopup", "Data request action triggered");
            Close();
            ElephantCore.Instance.PinRequest();
        }
        
        private void HandleCcpaAction(SettingsActionResponse action)
        {
            ElephantLog.Log("SETTINGSPopup", "CCPA action triggered");
            ShowPersonalizedAdsPopup(action, true);
        }
        
        private void HandleGdprAdConsentAction(SettingsActionResponse action)
        {
            ElephantLog.Log("SETTINGSPopup", "GDPR Ad Consent action triggered");
            ShowPersonalizedAdsPopup(action, false);
        }
        
        private void ShowPersonalizedAdsPopup(SettingsActionResponse action, bool isCcpa)
        {
            try
            {
                var payload = JsonConvert.DeserializeObject<PersonalizedAdsPayload>(action.payload);

                if (payload == null)
                {
                    ElephantLog.LogError("SETTINGSPopup", "Failed to parse PersonalizedAds payload");
                    return;
                }

                if (isCcpa)
                {
                    var ccpaPopup = ElephantPopupManager.Instance.ShowPopup<CcpaPopup>("ElephantUI/Ccpa/CcpaPopup");

                    if (ccpaPopup == null)
                    {
                        ElephantLog.LogError("SETTINGSPopup", "Failed to show CCPA PersonalizedAds popup");
                        return;
                    }

                    ccpaPopup.Initialize(
                        payload.title,
                        HyperlinkUtils.CleanText(payload.content),
                        payload.privacy_policy_text,
                        payload.privacy_policy_url,
                        payload.decline_text_action_button,
                        payload.agree_text_action_button,
                        payload.consent_text_action_button,
                        () => OnPersonalizedAdsAgree(true),
                        () => OnPersonalizedAdsDecline(true)
                    );
                }
                else
                {
                    var gdprPopup = ElephantPopupManager.Instance.ShowPopup<GdprPopup>("ElephantUI/Gdpr/GdprPopup");

                    if (gdprPopup == null)
                    {
                        ElephantLog.LogError("SETTINGSPopup", "Failed to show GDPR ads consent popup");
                        return;
                    }

                    gdprPopup.Initialize(
                        payload.title,
                        HyperlinkUtils.CleanText(payload.content),
                        payload.privacy_policy_text,
                        payload.privacy_policy_url,
                        payload.decline_text_action_button,
                        payload.agree_text_action_button,
                        payload.consent_text_action_button,
                        () => OnPersonalizedAdsAgree(false),
                        () => OnPersonalizedAdsDecline(false)
                    );
                }
            }
            catch (Exception e)
            {
                ElephantLog.LogError("SETTINGSPopup", $"Error showing {(isCcpa ? "CCPA" : "GDPR")} ads consent popup: {e.Message}");
            }
        }
        
        private void OnPersonalizedAdsAgree(bool isCcpa)
        {
            ElephantLog.Log("SETTINGSPopup", $"User agreed to {(isCcpa ? "CCPA" : "GDPR")}");

            if (isCcpa)
            {
                ElephantCore.Instance.ElephantComplianceManager.SendCcpaStatus(true);
            }
            else
            {
                ElephantCore.Instance.ElephantComplianceManager.SendGdprAdConsentStatus(true);
            }

            Close();
        }

        private void OnPersonalizedAdsDecline(bool isCcpa)
        {
            ElephantLog.Log("SETTINGSPopup", $"User declined {(isCcpa ? "CCPA" : "GDPR")}");

            if (isCcpa)
            {
                ElephantCore.Instance.ElephantComplianceManager.SendCcpaStatus(false);
            }
            else
            {
                ElephantCore.Instance.ElephantComplianceManager.SendGdprAdConsentStatus(false);
            }

            Close();
        }
        
        private void HandleCustomPopupAction(SettingsActionResponse action)
        {
            ElephantLog.Log("SETTINGSPopup", "Custom popup action triggered - not implemented");
        }

        private void OnCmpButtonClicked()
        {
            ElephantLog.Log("SETTINGSPopup", "CMP button clicked");
            onCmpButtonClicked?.Invoke();
        }

        private void OnElephantIdClicked()
        {
            if (string.IsNullOrEmpty(elephantId)) return;

            GUIUtility.systemCopyBuffer = elephantId;
            ElephantLog.Log("SETTINGSPopup", $"Copied Elephant ID to clipboard: {elephantId}");
    
            StartCoroutine(ShowCopyFeedback());
        }

        private IEnumerator ShowCopyFeedback()
        {
            if (copyFeedbackText != null)
            {
                copyFeedbackText.text = "Copied!";
                copyFeedbackText.gameObject.SetActive(true);
        
                yield return new WaitForSeconds(1.5f);
        
                copyFeedbackText.gameObject.SetActive(false);
            }
        }

        private void OnCloseClicked()
        {
            ElephantLog.Log("SETTINGSPopup", "Close button clicked");
            Close();
        }

        public void Close()
        {
            ElephantLog.Log("SETTINGSPopup", "Closing settings popup");
            ClearActionButtons();
            ElephantPopupManager.Instance.CloseCurrentPopup();
        }

        private void ClearActionButtons()
        {
            foreach (var button in spawnedButtons)
            {
                if (button != null) Destroy(button);
            }
            spawnedButtons.Clear();
        }

        private void OnDestroy()
        {
            ClearActionButtons();
        }

        private void ApplyPrimaryStyleToDynamicButton(Button button)
        {
            if (button == null)
            {
                return;
            }

            var style = PopupUIStyleConfig.GetStyle(PopupType.Settings);
            if (style == null || string.IsNullOrWhiteSpace(style.buttonPrimary))
            {
                return;
            }

            if (!ColorUtility.TryParseHtmlString(style.buttonPrimary, out var parsedColor))
            {
                return;
            }

            var image = button.targetGraphic as Image ?? button.GetComponent<Image>();
            if (image != null)
            {
                image.color = parsedColor;
            }
        }
    }
}