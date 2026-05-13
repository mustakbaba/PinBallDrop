using System;
using ElephantSDK;
using Facebook.Unity;
using UnityEngine;

namespace RollicGames.Utils
{
    public partial class RevenueForwardingUtils
    {
        private const string Tag = "RevenueForwardingUtils";
        
        private const string KeyLtv = "ltv_storage_key";
        private const string KeyRevenueForwardingLock = "revenue_forwarding_lock_key";
        private const string KeyFirstForward = "first_forward_key";
        private const string KeyRevenueToSend = "revenue_to_send_key";

        private static RevenueForwardingUtils _instance;
        /**
         * The first block on sending events to Facebook (or other platforms)
         * if LTV bigger than this, it's a go
         */
        private double _threshold;
        /**
         * After passing _threshold, this is the event trigger frequency value
         */
        private readonly double _frequency;
        private double _revenueToSend;
        private double _lifeTimeRevenue;
        /**
         * This is the ultimate check for revenue forwarding:
         *  if _lifeTimeRevenue > threshold within the first 24 hours, this is unlocked
         */
        private bool _isRevenueForwardingUnlocked;
        private bool _firstForwarding;
        
        public static RevenueForwardingUtils GetInstance(double threshold, int frequencyPercentage)
        {
            return _instance ?? (_instance = new RevenueForwardingUtils(threshold, frequencyPercentage));
        }

        private RevenueForwardingUtils(double threshold, int frequencyValue)
        {
            _threshold = threshold;
            _frequency = _threshold * frequencyValue / 100;

            _lifeTimeRevenue = PlayerPrefs.GetFloat(KeyLtv, 0);
            _revenueToSend = PlayerPrefs.GetFloat(KeyRevenueToSend, 0);
            _isRevenueForwardingUnlocked = Convert.ToBoolean(PlayerPrefs.GetString(KeyRevenueForwardingLock, "false"));
            _firstForwarding = Convert.ToBoolean(PlayerPrefs.GetString(KeyFirstForward, "false"));

            if (_firstForwarding)
            {
                _threshold = _frequency;
            }
            
            ElephantLog.Log(Tag, "_isRevenueForwardingUnlocked value = "+ _isRevenueForwardingUnlocked);
        }

        private void ForwardRevenue(double revenueToSend)
        {
            if (!_isRevenueForwardingUnlocked) return;

            FB.LogPurchase((float) revenueToSend, "USD");
            var parameters = Params.New()
                .Set("lifetime_revenue", _lifeTimeRevenue)
                .Set("revenue_to_send", revenueToSend)
                .Set("frequency", _frequency)
                .Set("threshold", _threshold);
            Elephant.Event("fb_revenue_forwarding", MonitoringUtils.GetInstance().GetCurrentLevel().level, parameters);
            
            _revenueToSend = 0;
            PlayerPrefs.SetFloat(KeyRevenueToSend, 0);
            PlayerPrefs.Save();

            _firstForwarding = true;
            _threshold = _frequency;
            PlayerPrefs.SetString(KeyFirstForward, _firstForwarding.ToString());
            PlayerPrefs.Save();

            SetLock(false);
        }

        private void SetLock(bool isUnlocked)
        {
            _isRevenueForwardingUnlocked = isUnlocked;
            PlayerPrefs.SetString(KeyRevenueForwardingLock, _isRevenueForwardingUnlocked.ToString());
            PlayerPrefs.Save();
        }
        
        private bool IsInFirst24Hours()
        {
            var firstOpenTsString = ElephantSDK.Utils.ReadFromFile(ElephantConstants.FIRST_OPEN_TS);
            if (string.IsNullOrEmpty(firstOpenTsString))
            {
                return false;
            }
            
            var firstOpenTimeStamp = long.Parse(firstOpenTsString);
            var currentTs = ElephantSDK.Utils.Timestamp();

            return currentTs - firstOpenTimeStamp < 86400;
        }
    }
}