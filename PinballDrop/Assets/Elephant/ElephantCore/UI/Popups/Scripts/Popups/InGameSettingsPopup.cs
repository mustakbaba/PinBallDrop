using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ElephantSDK
{
    public class InGameSettingsPopup : BasePopup
    {
        private const string VibrationPrefKey = "elephant_in_game_vibration_enabled";

        [SerializeField] private Image panelBackground;

        [Header("Buttons / Toggles")]
        [SerializeField] private Button closeButton;
        [SerializeField] private Toggle vibrationToggle;
        [SerializeField] private Toggle soundToggle;
        [SerializeField] private GameObject chatButtonObj;
        [SerializeField] private Button chatButton;
        [SerializeField] private Button legalTermsButton;
        [SerializeField] private Button restorePurchasesButton;
        [SerializeField] private Button supportButton;

        [Header("Toggle UI")]
        [SerializeField] private Image vibrationBackgroundImage;
        [SerializeField] private Image soundBackgroundImage;
        [SerializeField] private Image vibrationIcon;
        [SerializeField] private Image soundIcon;
        [SerializeField] private Sprite vibrationOnSprite;
        [SerializeField] private Sprite vibrationOffSprite;
        [SerializeField] private Sprite soundOnSprite;
        [SerializeField] private Sprite soundOffSprite;
        [SerializeField] private Color _toggleOnBackgroundColor;
        [SerializeField] private Color _toggleOffBackgroundColor;

        private Action _onChatRequested;
        private Action _onRestorePurchasesRequested;
        private Action<bool> _onSoundChanged;
        private Action<bool> _onVibrationChanged;

        private bool _isSoundOn;
        private bool _isVibrationOn;

        public void Initialize(
            bool hasChatFeature,
            bool helpshiftEnabled = false,
            Action onChatRequested = null,
            Action onRestorePurchasesRequested = null,
            Action<bool> onSoundChanged = null,
            Action<bool> onVibrationChanged = null)
        {
            ElephantLog.Log("[nGameSettingsPopup", "Initializing in-game settings popup");

            AssignCallbacks(
                onChatRequested,
                onRestorePurchasesRequested,
                onSoundChanged,
                onVibrationChanged);
            LoadInitialToggleStates();
            ResolveToggleColorsFromStyle(PopupUIStyleConfig.GetStyle(PopupType.InGameSettings));

            SetupButtons(hasChatFeature, helpshiftEnabled);
            RefreshToggleUI();
            ApplyPopupStyle();
        }

        private void AssignCallbacks(
            Action onChatRequested,
            Action onRestorePurchasesRequested,
            Action<bool> onSoundChanged,
            Action<bool> onVibrationChanged)
        {
            _onChatRequested = onChatRequested;
            _onRestorePurchasesRequested = onRestorePurchasesRequested;
            _onSoundChanged = onSoundChanged;
            _onVibrationChanged = onVibrationChanged;
        }

        private void LoadInitialToggleStates()
        {
            _isSoundOn = Elephant.IsInGameSoundEnabled();
            _isVibrationOn = PlayerPrefs.GetInt(VibrationPrefKey, 1) == 1;
        }

        private void SetupButtons(bool hasChatFeature, bool helpshiftEnabled)
        {
            BindButton(closeButton, Close);
            BindToggle(soundToggle, _isSoundOn, OnSoundToggleChanged);
            BindToggle(vibrationToggle, _isVibrationOn, OnVibrationToggleChanged);
            SetupRestorePurchasesButton();
            BindButton(legalTermsButton, OnLegalTermsClicked);
            SetupChatButton(hasChatFeature);
            SetupSupportButton(helpshiftEnabled);
        }

        private void SetupChatButton(bool hasChatFeature)
        {
            if (chatButton != null)
            {
                chatButtonObj.gameObject.SetActive(hasChatFeature);
                chatButton.onClick.RemoveAllListeners();
                if (hasChatFeature)
                {
                    chatButton.onClick.AddListener(OnChatClicked);
                }
            }
        }

        private void SetupRestorePurchasesButton()
        {
            if (restorePurchasesButton == null)
            {
                return;
            }

            bool hasHandler = _onRestorePurchasesRequested != null;
            restorePurchasesButton.gameObject.SetActive(hasHandler);
            if (hasHandler)
            {
                BindButton(restorePurchasesButton, OnRestorePurchasesClicked);
            }
            else
            {
                restorePurchasesButton.onClick.RemoveAllListeners();
            }
        }

        private void SetupSupportButton(bool helpshiftEnabled)
        {
            if (supportButton != null)
            {
                if (helpshiftEnabled)
                {
                    supportButton.gameObject.SetActive(true);
                    BindButton(supportButton, OnSupportClicked);
                }
                else
                {
                    supportButton.gameObject.SetActive(false);
                }
            }
        }

        private void ApplyPopupStyle()
        {
            var style = PopupUIStyleConfig.GetStyle(PopupType.InGameSettings);

            ApplyStyle(PopupType.InGameSettings);
            PopupStyleApplier.ApplyToInGameSettings(panelBackground, titleTexts);

            ApplyPrimaryColorToActionButtons(style);
            ApplyChatToggleOnColor(style);
        }

        private void OnSoundToggleChanged(bool isOn)
        {
            _isSoundOn = isOn;
            Elephant.SetInGameSoundEnabled(_isSoundOn);
            _onSoundChanged?.Invoke(_isSoundOn);
            RefreshToggleUI();
        }

        private void OnVibrationToggleChanged(bool isOn)
        {
            _isVibrationOn = isOn;
            PlayerPrefs.SetInt(VibrationPrefKey, _isVibrationOn ? 1 : 0);
            PlayerPrefs.Save();
            _onVibrationChanged?.Invoke(_isVibrationOn);
            RefreshToggleUI();
        }

        private void OnChatClicked()
        {
            _onChatRequested?.Invoke();
        }

        private void OnRestorePurchasesClicked()
        {
            _onRestorePurchasesRequested?.Invoke();
        }

        private void OnSupportClicked()
        {
            Elephant.ShowFAQ();
        }

        private void OnLegalTermsClicked()
        {
            Close();
            Elephant.ShowSettingsView();
        }

        private void RefreshToggleUI()
        {
            UpdateToggleVisual(_isSoundOn, soundBackgroundImage, soundIcon, soundOnSprite, soundOffSprite);
            UpdateToggleVisual(_isVibrationOn, vibrationBackgroundImage, vibrationIcon, vibrationOnSprite, vibrationOffSprite);
        }

        private void UpdateToggleVisual(bool isOn, Image backgroundImage, Image iconImage, Sprite onSprite, Sprite offSprite)
        {
            if (backgroundImage != null)
            {
                backgroundImage.color = isOn ? _toggleOnBackgroundColor : _toggleOffBackgroundColor;
            }

            if (iconImage != null)
            {
                var sprite = isOn ? onSprite : offSprite;
                if (sprite != null)
                {
                    iconImage.sprite = sprite;
                }
            }
        }

        private void ResolveToggleColorsFromStyle(PopupStyleData style)
        {
            if (style?.layout == null)
            {
                return;
            }

            if (TryParseColor(style.layout.toggleOnBackground, out var onColor))
            {
                _toggleOnBackgroundColor = onColor;
            }

            if (TryParseColor(style.layout.toggleOffBackground, out var offColor))
            {
                _toggleOffBackgroundColor = offColor;
            }
        }

        private void ApplyChatToggleOnColor(PopupStyleData style)
        {
            if (chatButton == null)
            {
                return;
            }

            if (style?.layout == null || !TryParseColor(style.layout.toggleOnBackground, out var chatColor))
            {
                return;
            }

            var image = chatButton.targetGraphic as Image ?? chatButton.GetComponent<Image>();
            if (image != null)
            {
                image.color = chatColor;
            }
        }

        private void ApplyPrimaryColorToActionButtons(PopupStyleData style)
        {
            if (style == null || !TryParseColor(style.buttonPrimary, out var primaryColor))
            {
                return;
            }

            ApplyColorToButton(restorePurchasesButton, primaryColor);
            ApplyColorToButton(supportButton, primaryColor);
            ApplyColorToButton(legalTermsButton, primaryColor);
        }

        private static void BindButton(Button button, Action action)
        {
            if (button == null) 
            {
                return;
            }

            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => action?.Invoke());
        }

        private static void BindToggle(Toggle toggle, bool isOn, Action<bool> onValueChanged)
        {
            if (toggle == null)
            {
                return;
            }

            toggle.onValueChanged.RemoveAllListeners();
            toggle.isOn = isOn;
            toggle.onValueChanged.AddListener(value => onValueChanged?.Invoke(value));
        }

        private static bool TryParseColor(string hex, out Color color)
        {
            color = default;
            if (string.IsNullOrWhiteSpace(hex))
            {
                return false;
            }

            return ColorUtility.TryParseHtmlString(hex, out color);
        }

        private static void ApplyColorToButton(Button button, Color color)
        {
            if (button == null)
            {
                return;
            }

            var image = button.targetGraphic as Image ?? button.GetComponent<Image>();
            if (image != null)
            {
                image.color = color;
            }
        }

        public void Close()
        {
            ElephantPopupManager.Instance.CloseCurrentPopup();
        }
    }
}
