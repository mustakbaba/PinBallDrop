using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections.Generic;
using UnityEngine.Serialization;
using UnityEngine.EventSystems;
using System.Collections;

namespace ElephantSDK
{
	/// <summary>
	/// This class is responsible for dynamically generating social media buttons from remote configuration and presenting them inside the popup.
	/// </summary>
    public class SocialPopup : BasePopup
    {
        [Header("UI References")]
        [SerializeField] private GameObject _buttonPrefab;
        [SerializeField] private Button _closeButton;
        [SerializeField] private TextMeshProUGUI _contentText;
		[SerializeField] private Transform _topContainer;
		[SerializeField] private Transform _bottomContainer;
        [SerializeField] private List<SocialIconData> _iconList = new List<SocialIconData>();

        private const string Tag = "SocialPopup";
        
        /// <summary>
        /// Initializes the popup content and creates the social buttons based on the remote configuration.
        /// </summary>
        /// <param name="content"></param>
        public void Initialize(string content)
        {
	        ElephantLog.Log(Tag, "Initializing");

            ApplyStyle(PopupType.Social);
            SetupContentText(content);
            SetupButton();
            CreateButtonsFromConfig();
        }

        private void SetupContentText(string content)
        {
	        if (_contentText != null)
	        {
		        _contentText.text = HyperlinkUtils.CleanText(content);
		        ElephantLog.Log(Tag, $"Content set: {content}");
	        }
	        else
	        {
		        ElephantLog.LogError(Tag, "contentText is null!");
	        }
        }

        private void SetupButton()
        {
	        if (_closeButton != null)
	        {
		        _closeButton.onClick.RemoveAllListeners();
		        _closeButton.onClick.AddListener(OnCloseButtonClicked);
	        }
        }

        /// <summary>
        /// Creates the social media buttons dynamically based on the remote config.
        /// </summary>
		private void CreateButtonsFromConfig()
		{
			var activeButtons = ElephantSocialIntegration.GetActiveButtons();
            bool hasButtonPrimaryColor = TryGetPrimaryButtonColor(out var buttonColor);
			
			if (activeButtons == null || activeButtons.Count == 0)
			{
				ElephantLog.LogError(Tag, "No active social buttons found in config");
				return;
			}
			
			int buttonCount = 0;
			foreach (var entry in activeButtons)
			{
				buttonCount++;
				
                var parent = buttonCount <= 2 ? _topContainer : _bottomContainer;
				var itemGO = Instantiate(_buttonPrefab, parent, false);

				if (!itemGO.TryGetComponent(out SocialButtonItem socialButton))
				{
					ElephantLog.LogError(Tag, "SocialButtonItem component not found on prefab");
					Destroy(itemGO);
					continue;
				}
				
				var platform = ElephantSocialIntegration.ParsePlatform(entry.name);
				var logo = GetLogoForPlatform(platform);
				var rewardTaken = ElephantSocialIntegration.HasReceivedSocialReward(platform);
				var rewardAmount = entry.reward;
				
				socialButton.Initialize(logo, rewardAmount, platform, rewardTaken);
                if (hasButtonPrimaryColor)
                {
                    socialButton.ApplyButtonColor(buttonColor);
                }
				ElephantLog.Log(Tag, $"Button created for platform: {platform} with reward: {rewardAmount}");
			}
		}

        private bool TryGetPrimaryButtonColor(out Color color)
        {
            color = default;
            var style = PopupUIStyleConfig.GetStyle(PopupType.Social);
            if (style == null || string.IsNullOrWhiteSpace(style.buttonPrimary))
            {
                return false;
            }

            return ColorUtility.TryParseHtmlString(style.buttonPrimary, out color);
        }
		
		/// <summary>
		/// Retrieves the correct sprite icon for a given social platform.
		/// </summary>
		/// <param name="platform"></param>
		/// <returns></returns>
		private Sprite GetLogoForPlatform(SocialMediaType platform)
		{
			foreach (var item in _iconList)
			{
				if (item.platform == platform)
				{
					return item.logo;
				}
			}
		    
			ElephantLog.LogError(Tag, $"Icon not found for platform: {platform}");
			return null;
		}

        private void OnCloseButtonClicked()
        {
	        ElephantLog.Log(Tag, "Close button clicked");
            Close();
        }

        public void Close()
        {
	        ElephantLog.Log(Tag, "Closing");
            ElephantPopupManager.Instance.CloseCurrentPopup();
        }
    }

	/// <summary>
	/// Maps a social media platform to its associated icon sprite
	/// </summary>
	[System.Serializable]
	public class SocialIconData
	{
    	public SocialMediaType platform;
    	public Sprite logo;
	}
}
