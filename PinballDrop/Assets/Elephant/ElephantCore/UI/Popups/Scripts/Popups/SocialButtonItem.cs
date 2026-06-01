using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

namespace ElephantSDK
{
    /// <summary>
    /// Represents a single social media button inside the SocialPopup.
    /// </summary>
    public class SocialButtonItem : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Button _button;
        [SerializeField] private Image _buttonBackground;
        [SerializeField] private Image _logo;
        [SerializeField] private GameObject _coinIcon;
        [SerializeField] private TextMeshProUGUI _coinText;

        private SocialMediaType _platform;

        /// <summary>
        /// Initializes the button visuals
        /// </summary>
        /// <param name="logo">Platform logo sprite</param>
        /// <param name="coinAmount">Reward amount</param>
        /// <param name="platform">Social media platform</param>
        /// <param name="rewardTaken">Whether the user has already collected the reward</param>
        public void Initialize(Sprite logo, int coinAmount, SocialMediaType platform, bool rewardTaken)
        {
            _platform = platform;

            if (_logo != null && logo != null)
            {
                _logo.sprite = logo;
            }

			if (_coinText != null)
			{
				if (coinAmount > 0)
				{
        			_coinText.text = $"+{coinAmount}";
				}
    			else
				{
					_coinText.text = "";
				}
			}

			if (_coinIcon != null)
			{
    			_coinIcon.SetActive(coinAmount > 0 && !rewardTaken);
			}

            SetupButton();
        }

        private void SetupButton()
        {
            if (_button != null)
            {
                _button.onClick.RemoveAllListeners();
                _button.onClick.AddListener(OnSocialButtonClicked);
            }
        }

        public void ApplyButtonColor(Color color)
        {
            if (_buttonBackground == null)
            {
                return;
            }
            
            _buttonBackground.color = color;
        }

        /// <summary>
        /// Handles click logic and hides coin icon
        /// </summary>
		public void OnSocialButtonClicked()
        { 
			ElephantSocialIntegration.SocialButtonClick(_platform);
            if (_coinIcon != null && _coinIcon.activeSelf)
            {
                _coinIcon.SetActive(false);
            }
        }
    }
}
