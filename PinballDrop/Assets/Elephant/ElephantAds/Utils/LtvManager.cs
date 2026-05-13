using ElephantSDK;
using UnityEngine;

namespace RollicGames.Utils
{
    public class LtvManager
    {
        private const string Tag = "LtvManager";
        private const string KeyLtv = Tag + "ltv_storage_key";
        private const string KeyCvHistory = Tag + "cv_history";
        private const string IapKeyLtv = Tag + "iap_ltv_storage_key";
        private static LtvManager _instance;

        public float LifeTimeRevenue { get; private set; }
        public int ConversionValue { get; private set; }
        public bool IsBuyer { get; private set; }
        public float IapLifetimeRevenue { get; private set; }

        public static LtvManager GetInstance()
        {
            return _instance = _instance ?? new LtvManager();
        }

        private LtvManager()
        {
            LifeTimeRevenue = PlayerPrefs.GetFloat(KeyLtv, 0);
            ConversionValue = PlayerPrefs.GetInt(KeyCvHistory, -1);
            IapLifetimeRevenue = PlayerPrefs.GetFloat(IapKeyLtv, 0);
            IsBuyer = IapLifetimeRevenue > 0;
        }

        public void UpdateRevenue(float rev)
        {
            LifeTimeRevenue += rev;
            PlayerPrefs.SetFloat(KeyLtv, LifeTimeRevenue);
            PlayerPrefs.Save();
            RollicEventUtils.GetInstance().SendRevenueEvents(rev);
            RollicEventUtils.GetInstance().CheckDynamicEvents();
        }

        public void UpdateConversionValue(int conversionValue)
        {
            ConversionValue = conversionValue;
            PlayerPrefs.SetFloat(KeyCvHistory, ConversionValue);
            PlayerPrefs.Save();
        }
        
        public void UpdateIAPRevenue(float rev)
        {
            IsBuyer = true;
            IapLifetimeRevenue += rev;
            PlayerPrefs.SetFloat(IapKeyLtv, IapLifetimeRevenue);
            PlayerPrefs.Save();
        }
    }
}