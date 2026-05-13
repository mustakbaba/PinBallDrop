using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

namespace ElephantSDK
{
    public class MonitoringUtils
    {
        public const string KeySessionStart = "sessionStart";
        public const string KeySessionEnd = "sessionEnd";
        private const string KeyCurrentLevel = "ElephantSDK_CurrentLevel";
        
        private static MonitoringUtils _instance;

        private readonly List<double> _fpsSessionLog;
        private readonly List<int> _currentLevelLog;

        private ElephantLevel _currentLevel;
        private int _memoryUsage;
        private int _memoryUsagePercentage;
        
        private float sessionStartBatteryLevel;
        private float sessionEndBatteryLevel;
        
        private RollicEventUtils _eventUtils;

        private MonitoringUtils()
        {
            _fpsSessionLog = new List<double>();
            _currentLevelLog = new List<int>();
            _currentLevel = LoadCurrentLevel();
            _memoryUsage = 0;
            _memoryUsagePercentage = 0;
            _eventUtils = RollicEventUtils.GetInstance();
        }

        private ElephantLevel LoadCurrentLevel()
        {
            var json = PlayerPrefs.GetString(KeyCurrentLevel, "");
            return string.IsNullOrEmpty(json) ? new ElephantLevel() : JsonConvert.DeserializeObject<ElephantLevel>(json);
        }

        private void SaveCurrentLevel()
        {
            var json = JsonConvert.SerializeObject(_currentLevel);
            PlayerPrefs.SetString(KeyCurrentLevel, json);
            PlayerPrefs.Save();
        }

        public static MonitoringUtils GetInstance()
        {
            return _instance ?? (_instance = new MonitoringUtils());
        }

        public void LogFps(double fpsValue)
        {
            _fpsSessionLog.Add(fpsValue);
        }
        
        public float CalculateFps(float[] fpsBuffer)
        {
            float total = 0;

            foreach (var v in fpsBuffer)
            {
                total += v;
            }

            return Mathf.Round(total / fpsBuffer.Length);
        }

        public void LogCurrentLevel()
        {
            _currentLevelLog.Add(_currentLevel.level);
        }

        public List<double> GetFpsSessionLog()
        {
            return _fpsSessionLog;
        }
        
        public List<int> GetCurrentLevelLog()
        {
            return _currentLevelLog;
        }

        public void SetCurrentLevel(int currentLevel, string originalLevelId)
        {
            var level = new ElephantLevel
            {
                level = currentLevel,
                original_level = originalLevelId,
                level_time = Utils.Timestamp()
            };
            _currentLevel = level;
            SaveCurrentLevel();
            
            _eventUtils.SendLevelEvents(currentLevel);
        }

        public ElephantLevel GetCurrentLevel()
        {
            return _currentLevel;
        }
        
        public void SetMemoryUsage(int memoryUsageValue)
        {
            _memoryUsage = memoryUsageValue;
        }
        
        public int GetMemoryUsage()
        {
            return _memoryUsage;
        }

        public void LogBatteryLevel(string type)
        {
            if (ElephantCore.Instance == null) return;
            
            switch (type)
            {
                case KeySessionStart:
#if UNITY_EDITOR
                    sessionStartBatteryLevel = SystemInfo.batteryLevel;
#elif UNITY_ANDROID
                    sessionStartBatteryLevel = ElephantAndroid.GetBatteryLevel();
#elif UNITY_IOS
                    sessionStartBatteryLevel = ElephantIOS.getBatteryLevel();
#else
                    sessionStartBatteryLevel = SystemInfo.batteryLevel;
#endif
                    break;
                case KeySessionEnd:
#if UNITY_EDITOR
                    sessionEndBatteryLevel = SystemInfo.batteryLevel;
#elif UNITY_ANDROID
                    sessionEndBatteryLevel = ElephantAndroid.GetBatteryLevel();
#elif UNITY_IOS
                    sessionEndBatteryLevel = ElephantIOS.getBatteryLevel();
#else
                    sessionEndBatteryLevel = SystemInfo.batteryLevel;
#endif
                    break;
            }
        }

        public float GetSessionStartBatteryLevel()
        {
            return sessionStartBatteryLevel;
        }
        
        public float GetSessionEndBatteryLevel()
        {
            return sessionEndBatteryLevel;
        }
        
        public void SetMemoryUsagePercentage(int memoryUsagePercentage)
        {
            _memoryUsagePercentage = memoryUsagePercentage;
        }
        
        public int GetMemoryUsagePercentage()
        {
            return _memoryUsagePercentage;
        }

        public void Flush()
        {
            _fpsSessionLog?.Clear();
            _currentLevelLog?.Clear();
        }
    }
}