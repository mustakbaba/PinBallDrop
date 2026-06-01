using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace ElephantSDK
{
    /// <summary>
    /// Provides methods to show popup, get rewards, and handle social button clicks.
    /// </summary>
    public class ElephantSocialIntegration
    {
        public static event Action<SocialMediaType> OnSocialPopupRewardGrant;
        
        private static bool _hasOpenedSocialApp = false;
        private static SocialMediaType _lastOpenedPlatform = SocialMediaType.Unknown;

        private static SocialButtonList _cachedConfig;
        private static bool _isConfigLoaded;

        private const string Tag = "ElephantSocialIntegration";

        // Mapping between SocialMediaType enum and platform string names
        private static readonly Dictionary<SocialMediaType, string> PlatformNames = new()
        {
            { SocialMediaType.Instagram, "instagram" },
            { SocialMediaType.Facebook, "facebook" },
            { SocialMediaType.TikTok, "tiktok" },
            { SocialMediaType.Reddit, "reddit" }
        };
        
        /// <summary>
        /// Holds information about rewards of active buttons.
        /// </summary>
        public struct RewardInfo
        {
            public int MaxReward { get; set; }
            public bool AllSame { get; set; }

            public RewardInfo(int maxReward, bool allSame)
            {
                MaxReward = maxReward;
                AllSame = allSame;
            }
        }
        
        static ElephantSocialIntegration()
        {
            Elephant.OnApplicationFocusTrue += CheckSocialRewardOnFocus;
        }

        /// <summary>
        /// Shows the social popup with reward information based on active buttons.
        /// </summary>
        public static void ShowSocialPopup()
        {
            SocialPopup popup = ElephantPopupManager.Instance.ShowPopup<SocialPopup>("ElephantUI/Social/SocialPopup");
            if (popup == null)
            {
                return;
            }

            var activeButtons = GetActiveButtons();
            if (activeButtons == null || activeButtons.Count == 0)
            {
                popup.Initialize("Follow us for updates and giveaways.");
                return;
            }

            RewardInfo rewardInfo = GetRewardInfo(activeButtons);

            string message = rewardInfo.MaxReward == 0
                ? "Follow us for updates and giveaways."
                : rewardInfo.AllSame
                    ? $"Follow us for updates, giveaways, and {rewardInfo.MaxReward} coins just for hopping on board."
                    : $"Follow us for updates, giveaways, and up to {rewardInfo.MaxReward} coins just for hopping on board.";

            popup.Initialize(message);

			var buttonNames = string.Join(",", activeButtons.Select(b => b.name));
			var param = Params.New() .Set("socialButtons", buttonNames);
			Elephant.Event("rollic_socials_show_popup", -1, param);
        }

        /// <summary>
        /// Calculates the maximum reward and whether all rewards are the same among active buttons.
        /// </summary>
        /// <param name="buttons"></param>
        /// <returns>RewardInfo struct containing max reward and allSame flag</returns>
        private static RewardInfo GetRewardInfo(List<SocialButtonData> buttons)
        {
            int maxReward = 0;
            int firstReward = -1;
            bool allSame = true;

            foreach (var button in buttons)
            {
                if (button == null)
                {
                    continue;
                }

                int reward = Math.Max(button.reward, 0);
                maxReward = Math.Max(maxReward, reward);

                if (firstReward == -1)
                {
                    firstReward = reward;
                }
                else if (reward != firstReward)
                {
                    allSame = false;
                }
            }

            return new RewardInfo(maxReward, allSame);
        }
        
        /// <summary>
        /// Loads the social configuration from remote config if not already cached.
        /// </summary>
        /// <returns></returns>
        private static SocialButtonList GetConfig()
        {
            if (_isConfigLoaded)
            {
                return _cachedConfig;
            }

            var jsonConfig = RemoteConfig.GetInstance().Get("rollic_social_popup_config", "");
            if (!string.IsNullOrEmpty(jsonConfig))
            {
                try
                {
                    _cachedConfig = JsonUtility.FromJson<SocialButtonList>(jsonConfig);
                }
                catch (Exception e)
                {
                    ElephantLog.LogError(Tag, $"Error parsing social media config: {e.Message}");
                    _cachedConfig = null;
                }
            }

            _isConfigLoaded = true;
            return _cachedConfig;
        }

        /// <summary>
        /// Checks whether a specific social media platform is enabled in the remote configuration.
        /// </summary>
        /// <param name="type">The social media platform</param>
        /// <returns>True if the platform is active. Otherwise, false.</returns>
        public static bool IsSocialEnabled(SocialMediaType type)
        {
            if (!PlatformNames.ContainsKey(type))
            {
                return false;
            }

            var buttons = GetConfig()?.buttons;
            if (buttons == null)
            {
                return false;
            }

            var platform = PlatformNames[type];
            foreach (var button in buttons)
            {
                if (string.Equals(button.name, platform, StringComparison.OrdinalIgnoreCase) && button.active)
                {
                    if (string.IsNullOrEmpty(button.url))
                    {
                        return false;
                    }
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Gets the reward amount for a specific social media platform.
        /// </summary>
        /// <param name="type">The social media platform</param>
        /// <returns>Reward amount, 0 if not available</returns>
        public static int GetReward(SocialMediaType type)
        {
            if (!PlatformNames.ContainsKey(type))
            {
                return 0;
            }

            var buttons = GetConfig()?.buttons;
            if (buttons == null)
            {
                return 0;
            }

            string platformName = PlatformNames[type];
            foreach (var button in buttons)
            {
                if (string.Equals(button.name, platformName, StringComparison.OrdinalIgnoreCase) && button.active)
                {
                    return button.reward > 0 ? button.reward : 0;
                }
            }

            return 0;
        }

        /// <summary>
        /// Retrieves the URL associated with a specific social media platform.
        /// </summary>
        /// <param name="type"></param>
        /// <returns>URL string if available</returns>
        public static string GetUrl(SocialMediaType type)
        {
            if (!PlatformNames.ContainsKey(type))
            {
                return null;
            }

            var buttons = GetConfig()?.buttons;
            if (buttons == null)
            {
                return null;
            }

            string platformName = PlatformNames[type];
            foreach (var button in buttons)
            {
                if (string.Equals(button.name, platformName, StringComparison.OrdinalIgnoreCase) && button.active)
                {
                    return button.url;
                }
            }

            return null;
        }

        /// <summary>
        /// Opens the social URL and tracks reward.
        /// </summary>
        /// <param name="type"></param>
        public static void SocialButtonClick(SocialMediaType type)
        {
            if (!IsSocialEnabled(type))
            {
                return;
            }

            var url = GetUrl(type);
            if (string.IsNullOrEmpty(url))
            {
                return;
            }

            _hasOpenedSocialApp = true;
            _lastOpenedPlatform = type;

			var param = Params.New().Set("socialApp", type.ToString());
			Elephant.Event("rollic_socials_button_click", -1, param);
            ElephantLog.Log(Tag, $"Opening social media URL for {type}: {url}");

            Application.OpenURL(url);
        }

        /// <summary>
        /// Checks whether the user returned from a social app and grants reward if applicable.
        /// Should be called when the application gains focus.
        /// </summary>
        public static void CheckSocialRewardOnFocus()
        {
            if (_hasOpenedSocialApp && _lastOpenedPlatform != SocialMediaType.Unknown)
            {
                if (!HasReceivedSocialReward(_lastOpenedPlatform))
                {
                    MarkSocialRewardReceived(_lastOpenedPlatform);
                    OnSocialPopupRewardGrant?.Invoke(_lastOpenedPlatform);

                    var coinAmount = GetReward(_lastOpenedPlatform);
                    ElephantLog.Log(Tag, $"Reward granted for {_lastOpenedPlatform}: {coinAmount} coins");
                
					var parameters = Params.New().Set("platform", _lastOpenedPlatform.ToString()).Set("reward", coinAmount);
					Elephant.Event("rollic_socials_reward_grant", -1, parameters);
				}
                else
                {
                    ElephantLog.Log(Tag, $"Reward already received for {_lastOpenedPlatform}");
                }

                _hasOpenedSocialApp = false;
                _lastOpenedPlatform = SocialMediaType.Unknown;
            }
        }

        /// <summary>
        /// Checks if the reward for a platform has already been claimed.
        /// </summary>
        /// <param name="platform"></param>
        /// <returns></returns>
        public static bool HasReceivedSocialReward(SocialMediaType platform)
        {
            return PlayerPrefs.GetInt($"social_reward_{platform}", 0) == 1;
        }

        /// <summary>
        /// Marks the reward for a platform as received.
        /// </summary>
        /// <param name="platform"></param>
        private static void MarkSocialRewardReceived(SocialMediaType platform)
        {
            PlayerPrefs.SetInt($"social_reward_{platform}", 1);
            PlayerPrefs.Save();
        }

        /// <summary>
        /// Returns a list of all currently active social buttons that match the platform names.
        /// </summary>
        /// <returns></returns>
        public static List<SocialButtonData> GetActiveButtons()
        {
            var buttons = GetConfig()?.buttons;
            if (buttons == null)
            {
                return new List<SocialButtonData>();
            }

            return buttons.FindAll(button =>
            {
                return button.active && PlatformNames.ContainsValue(button.name.ToLower()) && !string.IsNullOrEmpty(button.url);
            });
        }

        /// <summary>
        /// Parses a platform name string and returns the corresponding SocialMediaType enum.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static SocialMediaType ParsePlatform(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return SocialMediaType.Unknown;
            }

            foreach (var pair in PlatformNames)
            {
                if (string.Equals(pair.Value, name, StringComparison.OrdinalIgnoreCase))
                {
                    return pair.Key;
                }
            }

            return SocialMediaType.Unknown;
        }
    }
}
