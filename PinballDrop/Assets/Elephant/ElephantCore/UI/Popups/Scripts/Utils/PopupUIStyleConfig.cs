using System;
using System.Collections.Generic;

namespace ElephantSDK
{
    public static class PopupUIStyleConfig
    {
        private static Dictionary<string, PopupStyleData> _cachedStyles;
        private static bool _isInitialized;

        private static readonly Dictionary<PopupType, string> PopupTypeToKey = new()
        {
            { PopupType.ForceUpdate, "ForceUpdate" },
            { PopupType.Blocked, "Blocked" },
            { PopupType.Ccpa, "Ccpa" },
            { PopupType.Gdpr, "Gdpr" },
            { PopupType.Vppa, "Vppa" },
            { PopupType.Tos, "Tos" },
            { PopupType.Pin, "Pin" },
            { PopupType.Loading, "Loading" },
            { PopupType.Error, "Error" },
            { PopupType.Settings, "Settings" },
            { PopupType.NetworkOffline, "NetworkOffline" },
            { PopupType.InGameSettings, "InGameSettings" },
            { PopupType.Social, "Social" },
            { PopupType.AgeBlocked, "AgeBlocked" },
            { PopupType.Collectibles, "Collectibles" },
            { PopupType.Alert, "Alert" }
        };

        private const string RemoteConfigKey = "popup_ui_style_config";

        /// <summary>
        /// Returns style for the given popup type. Null if no style config exists for this popup.
        /// </summary>
        public static PopupStyleData GetStyle(PopupType popupType)
        {
            var config = GetConfig();
            if (config == null) 
            {
                return null;
            }
            string key = PopupTypeToKey.TryGetValue(popupType, out var k) ? k : popupType.ToString();
            if (config.TryGetValue(key, out var style))
            {
                return style;
            }

            return null;
        }

        private static Dictionary<string, PopupStyleData> GetConfig()
        {
            if (_isInitialized)
            {
                return _cachedStyles;
            }

            _isInitialized = true;
            string json = RemoteConfig.GetInstance().Get(RemoteConfigKey, "");

            if (string.IsNullOrEmpty(json))
            {
                _cachedStyles = new Dictionary<string, PopupStyleData>();
                return _cachedStyles;
            }

            try
            {
                _cachedStyles = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, PopupStyleData>>(json) ?? new Dictionary<string, PopupStyleData>();
            }
            catch (Exception e)
            {
                ElephantLog.LogError("PopupUIStyleConfig", $"Error parsing {RemoteConfigKey}: {e.Message}");
                _cachedStyles = new Dictionary<string, PopupStyleData>();
            }

            return _cachedStyles;
        }
    }

    /// <summary>
    /// Remote JSON keys (e.g. "#RRGGBB") Omit keys to keep prefab colors
    /// </summary>
    [Serializable]
    public class PopupStyleData
    {
        public string background;
        public string titleBackground;
        public string title;
        public string titleOutline;
        public bool? titleOutlineLinearCorrected;
        public string buttonContainer;
        public string buttonPrimary;
        public string divider;
        public PopupStyleLayout layout; // Settings/InGameSettings can use these for layout-specific coloring.
    }

    /// <summary>
    /// Popup-specific layout accents.
    /// </summary>
    [Serializable]
    public class PopupStyleLayout
    {
        public string panel;
        public string buttonSecondary;
        public string toggleOnBackground;
        public string toggleOffBackground;
    }
}
