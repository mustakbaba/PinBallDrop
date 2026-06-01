using System;
using System.Collections;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using RollicGames.Utils;

namespace ElephantSDK
{
    public class RollicEventUtils
    {
        private static RollicEventUtils _instance;
        private const string Tag = "[RollicEventUtils]";
        private const string KeySentTokens = "RL_Sent_Tokens";
        private const string KeyPlayTimeCounter = "RL_PlayTime_Counter_v2";
        
        private static MetaDataUtils _metaDataUtils;
        private static ExpressionParser _expressionParser;
        private List<Events> _dynamicEvents = new();
        private HashSet<string> _sentTokens;
        private bool _dynamicEventsLoggedOnce = false;
        
        private bool _hasProcessedAppLaunch = false;
        
        private readonly string[] _playTimeTokens =
        {
            AdjustTokens.Timespend_30,
            AdjustTokens.Timespend_60,
            AdjustTokens.Timespend_120,
            AdjustTokens.Timespend_210
        };
        
        private readonly int[] _playTimes =
        {
            30,
            60,
            120,
            210
        };

        private RollicEventUtils()
        {
            _metaDataUtils = MetaDataUtils.GetInstance();
            _expressionParser = new ExpressionParser();
            _sentTokens = new HashSet<string>(PlayerPrefs.GetString(KeySentTokens, "").Split(',').Where(t => !string.IsNullOrEmpty(t)));
            
            _hasProcessedAppLaunch = false;
        }

        public static RollicEventUtils GetInstance()
        {
            return _instance ?? (_instance = new RollicEventUtils());
        }
        
        public void SendLevelEvents(int currentLevel)
        {
            var token = ""; 
            switch (currentLevel)
            {
                case 20:
                    token = AdjustTokens.Level_20;
                    break;
                case 50:
                    token = AdjustTokens.Level_50;
                    break;
                case 100:
                    token = AdjustTokens.Level_100;
                    break;
                case 200:
                    token = AdjustTokens.Level_200;
                    break;
                case 300:
                    token = AdjustTokens.Level_300;
                    break;
                case 500:
                    token = AdjustTokens.Level_500;
                    break;
                case 1000:
                    token = AdjustTokens.Level_1000;
                    break;
            }

            SendToAdjust(token);
            CheckDynamicEvents();
        }

        public void SendRevenueEvents(float revenue)
        {
            var token = "";
            if (revenue >= 10)
                token = AdjustTokens.Revenue_10;
            else if (revenue >= 5)
                token = AdjustTokens.Revenue_5;
            else if (revenue >= 2)
                token = AdjustTokens.Revenue_2;
            else if (revenue >= 1)
                token = AdjustTokens.Revenue_1;

            if (!string.IsNullOrEmpty(token))
            {
                SendToAdjust(token);
                CheckDynamicEvents();
            }
        }
        
        public IEnumerator FetchTokenLogicsFromEndpoint()
        {
            var data = new BaseData();
            data.FillBaseData(ElephantCore.Instance.GetCurrentSession().GetSessionID());
            var json = JsonConvert.SerializeObject(data);
            var bodyJson = JsonConvert.SerializeObject(new ElephantData(json, ElephantCore.Instance.GetCurrentSession().GetSessionID()));
            var networkManager = new GenericNetworkManager<EventLogicResponse>();
            var postWithResponse = networkManager.PostWithResponse(ElephantConstants.LOGICS_EP, bodyJson, response =>
            {
                if(response.data == null)
                {
                    ElephantLog.Log(Tag, "No logic received from the server");
                    _dynamicEvents = new List<Events>();
                    return;
                }
                
                _dynamicEvents = response.data.events;
            }, s =>
            {
                ElephantLog.LogError(Tag, $"Error parsing JSON: {s}");
                _dynamicEvents = new List<Events>();
            });

            return postWithResponse;
        }

        public void CheckDynamicEvents()
        {
            if (ElephantCore.Instance == null) return;
            var openResponse = ElephantCore.Instance.GetOpenResponse();
            if (openResponse?.internal_config == null) return;
            
            if (!openResponse.internal_config.dynamic_events_enabled)
            {
                if (_dynamicEventsLoggedOnce) return;
                ElephantLog.Log("<ADJUSTEVENTS>", "Dynamic Events Disabled");
                _dynamicEventsLoggedOnce = true;
                return;
            }
            
            if (_metaDataUtils == null) return;

                var isNewSession = !_hasProcessedAppLaunch;

            var context = new Dictionary<string, object>
            {
                { "level", _metaDataUtils.GetCurrentLevel() },
                { "levelCompleteCount", _metaDataUtils.GetLevelCompleteCount() },
                { "totalInterstitialCount", _metaDataUtils.GetTotalInterstitialCount() },
                { "totalRewardedCount", _metaDataUtils.GetTotalRewardedCount() },
                { "timespend", GetTimeSpentInMinutes() },
                { "ltv", LtvManager.GetInstance().LifeTimeRevenue },
                { "newSession", isNewSession }
            };
            
            if(_dynamicEvents == null || _dynamicEvents.Count == 0)
            {
                ElephantLog.Log(Tag, "No dynamic events to check");
                return;
            }

            foreach (var dynamicEvent in _dynamicEvents)
            {
                var isUnique = dynamicEvent.unique;
                if (isUnique && _sentTokens.Contains(dynamicEvent.token))
                    continue;
                    
                if (!_expressionParser.EvaluateExpression(context, dynamicEvent.condition)) 
                    continue;
                
                SendToAdjust(dynamicEvent.token);
                
                if (isUnique)
                {
                    _sentTokens.Add(dynamicEvent.token);
                    SaveSentTokens();
                }
            }
            
            if (isNewSession)
            {
                _hasProcessedAppLaunch = true;
            }
        }

        private void SaveSentTokens()
        {
            PlayerPrefs.SetString(KeySentTokens, string.Join(",", _sentTokens));
            PlayerPrefs.Save();
        }

        public void SendInterstitialEvents()
        {
            var token = "";
            switch (_metaDataUtils.GetTotalInterstitialCount())
            {
                case 10:
                    token = AdjustTokens.FullScreenWatched_10;
                    break;
                case 25:
                    token = AdjustTokens.FullScreenWatched_25;
                    break;
                case 50:
                    token = AdjustTokens.FullScreenWatched_50;
                    break;
            }

            _metaDataUtils.IncrementByOne(MetaDataKeys.KeyInterstitialWatchCounter);
            _metaDataUtils.IncrementByOne(MetaDataKeys.KeySessionInterstitialWatchCounter);
            SendToAdjust(token);
            CheckDynamicEvents();
        }
        
        public void SendRewardedEvents()
        {
            var token = "";
            switch (_metaDataUtils.GetTotalRewardedCount())
            {
                case 10:
                    token = AdjustTokens.RewardedWatched_10;
                    break;
                case 25:
                    token = AdjustTokens.RewardedWatched_25;
                    break;
                case 50:
                    token = AdjustTokens.RewardedWatched_50;
                    break;
            }
            
            _metaDataUtils.IncrementByOne(MetaDataKeys.KeyRewardedAdWatchCounter);
            _metaDataUtils.IncrementByOne(MetaDataKeys.KeySessionRewardedAdWatchCounter);
            SendToAdjust(token);
            CheckDynamicEvents();
        }
        
        public void SendTimeSpendEvents(long timeSpendInMinutes)
        {
            var eventCounter = PlayerPrefs.GetInt(KeyPlayTimeCounter, 0);

            if (eventCounter >= _playTimeTokens.Length) return;

            if (timeSpendInMinutes >= _playTimes[eventCounter])
            {
                var token = _playTimeTokens[eventCounter];
                SendToAdjust(token);

                eventCounter += 1;
                PlayerPrefs.SetInt(KeyPlayTimeCounter, eventCounter);
            }

            CheckDynamicEvents();
        }

        private static void SendToAdjust(string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                ElephantLog.Log(Tag, "Token is not filled, no condition is matched");
                return;
            }

            if (token.Contains("TEMP_GAMEKIT"))
            {
                ElephantLog.LogError(Tag, "Token is not injected. Make sure you imported the gamekit correctly.");
                return;
            }

            ElephantCore.Instance.AdjustElephantAdapter?.TrackAdjustEvent(token);
        }

        private long GetTimeSpentInMinutes()
        {
            var totalTimeSpendMs = Utils.ReadLongFromFile(ElephantConstants.TimeSpend, 0);
            return totalTimeSpendMs / 1000 / 60;
        }
    }
}