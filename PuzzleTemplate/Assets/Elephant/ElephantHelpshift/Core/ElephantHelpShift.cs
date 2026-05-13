using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using ElephantSDK;
using UnityEngine;
using Helpshift;
using RollicGames.Utils;
using UnityEngine.UI;

namespace ElephantSDK
{
    public class ElephantHelpShift :  IHelpShiftElephantAdapter
    {
        public static bool IsActive => _instance != null && _instance._intialized;
        private static ElephantHelpShift _instance;

        private bool _intialized;
        private HelpshiftSdk help;

        public void Init(string domainName, string appId)
        {
            if (!ElephantCore.Instance.GetOpenResponse().internal_config.helpshift_enabled)
                return;
            
            if (string.IsNullOrEmpty(domainName))
            {
                ElephantLog.LogError("Helpshift", "Domain name is empty!");
                return;
            }
            
            if (string.IsNullOrEmpty(appId))
            {
                ElephantLog.LogError("Helpshift", "AppID is empty!");
                return;
            }

#if UNITY_EDITOR
            return;
#else
            _instance = this;
            help = HelpshiftSdk.GetInstance();
            var configMap = new Dictionary<string, object>();
            help.Install(appId, domainName, configMap);
            _intialized = true;
            
#endif
        }

        public void ShowConversation()
        {
#if UNITY_EDITOR
            return;
#else
            if (!IsActive)
            {
                return;
            }
            
            Elephant.Event("helpshift_show_conversation", MonitoringUtils.GetInstance().GetCurrentLevel().level);
            var configMap = _instance.GetConfigMap();
            _instance.help.ShowConversation(configMap);
#endif
        }

        public void ShowFAQs()
        {
#if UNITY_EDITOR
            return;
#else
            if (!IsActive)
            {
                return;
            }
            
            Elephant.Event("helpshift_show_FAQ", MonitoringUtils.GetInstance().GetCurrentLevel().level);
            Dictionary<string, object> configMap = _instance.GetConfigMap();
            _instance.help.ShowFAQs(configMap);
#endif
        }

        private Dictionary<string, object> GetConfigMap()
        {
            var userId = ConvertStringDataSingleLine(ElephantCore.Instance.userId);
            // For Applovin MAX user journey
            var userJourneyId = ConvertStringDataSingleLine(ElephantCore.Instance.userId + "|" + ElephantCore.Instance.adjustId);
            var platform = ConvertStringDataSingleLine(Application.platform.ToString());
            var device = ConvertStringDataSingleLine(SystemInfo.deviceModel);
            var appVersion = ConvertStringDataSingleLine(Application.version);
            var level = ConvertNumberData(MonitoringUtils.GetInstance().GetCurrentLevel().level);
            var userTag = ConvertStringDataSingleLine(RemoteConfig.GetInstance().GetTag());
            var ltv = ConvertNumberData(LtvManager.GetInstance().LifeTimeRevenue);
            var buyer = ConvertBooleanData(LtvManager.GetInstance().IsBuyer);
            var iapLtv = ConvertNumberData(LtvManager.GetInstance().IapLifetimeRevenue);

            var cifDictionary = new Dictionary<string, object>();
            cifDictionary.Add("platform", platform);
            cifDictionary.Add("device", device);
            cifDictionary.Add("user_id", userId);
            cifDictionary.Add("userJourneyId", userJourneyId);
            cifDictionary.Add("app_version", appVersion);
            cifDictionary.Add("level", level);
            cifDictionary.Add("user_tag", userTag);
            cifDictionary.Add("game_id", ElephantThirdPartyIds.GameId);
            cifDictionary.Add("idfv", ElephantCore.Instance.idfv);
            cifDictionary.Add("ltv", ltv);
            cifDictionary.Add("buyer", buyer);
            cifDictionary.Add("iap_ltv", iapLtv);

            var configMap = new Dictionary<string, object>();
            configMap.Add("cifs", cifDictionary);

            return configMap;
        }

        private Dictionary<string, string> ConvertStringDataSingleLine(string value)
        {
            return ConvertData("singleline", value);
        }

        private Dictionary<string, string> ConvertBooleanData(bool value)
        {
            return ConvertData("checkbox", value ? "true" : "false");
        }

        private Dictionary<string, string> ConvertNumberData(int value)
        {
            return ConvertData("number", value.ToString());
        }
        
        private Dictionary<string, string> ConvertNumberData(float value)
        {
            return ConvertData("number", value.ToString(CultureInfo.InvariantCulture));
        }

        private Dictionary<string, string> ConvertData(string dataType, string value)
        {
            var data = new Dictionary<string, string>
            {
                { "type", dataType },
                { "value", value }
            };
            return data;
        }
    }
}