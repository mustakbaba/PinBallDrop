using System;
using System.Collections.Generic;
using UnityEngine;

namespace ElephantSDK
{
    public static class PopupUIConfig
    {
        private static PopupUIConfigData _cachedConfig;
        private static bool _isInitialized;

        private static readonly PopupUIConfigData DefaultConfig = new()
        {
            force_update = true,
            blocked = true,
            ccpa = true,
            gdpr = true,
            vppa = true,
            tos = true,
            pin = true,
            loading = true,
            error = true,
            settings = true,
            network_offline = true,
            collectibles = true,
            alert = true
        };

        private static readonly Dictionary<PopupType, Func<PopupUIConfigData, bool?>> PopupConfigMap =
            new()
            {
                { PopupType.ForceUpdate,    c => c.force_update },
                { PopupType.Blocked,        c => c.blocked },
                { PopupType.Ccpa,           c => c.ccpa },
                { PopupType.Gdpr,           c => c.gdpr },
                { PopupType.Vppa,           c => c.vppa },
                { PopupType.Tos,            c => c.tos },
                { PopupType.Pin,            c => c.pin },
                { PopupType.Loading,        c => c.loading },
                { PopupType.Error,          c => c.error },
                { PopupType.Settings,       c => c.settings },
                { PopupType.NetworkOffline, c => c.network_offline },
                { PopupType.Collectibles, c => c.collectibles },
                { PopupType.Alert, c => c.alert },
            };

        public static bool UseNewPopupSystem(PopupType popupType)
        {
            if (!PopupConfigMap.TryGetValue(popupType, out var selector))
			{
				return false;
        	}

            var config = GetConfig();
            return selector(config) == true;
        }

        private static PopupUIConfigData GetConfig()
        {
            if (_isInitialized)
            {
			    return _cachedConfig;
            }

            _isInitialized = true;

            var json = RemoteConfig.GetInstance().Get("popup_ui_config", "");

            if (string.IsNullOrEmpty(json))
            {
                _cachedConfig = DefaultConfig;
                return _cachedConfig;
            }

            try
            {
                _cachedConfig = Newtonsoft.Json.JsonConvert.DeserializeObject<PopupUIConfigData>(json) ?? DefaultConfig;
            }
            catch (Exception e)
            {
                ElephantLog.LogError("PopupUIConfig",$"Error parsing popup_ui_config: {e.Message}");
                _cachedConfig = DefaultConfig;
            }

            return _cachedConfig;
        }

        [Serializable]
        private class PopupUIConfigData
        {
            public bool? force_update;
            public bool? blocked;
            public bool? ccpa;
            public bool? gdpr;
            public bool? vppa;
            public bool? tos;
            public bool? pin;
            public bool? loading;
            public bool? error;
            public bool? settings;
            public bool? network_offline;
            public bool? collectibles;
            public bool? alert;
        }
    }
}
