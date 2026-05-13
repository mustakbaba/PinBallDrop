using UnityEngine;

namespace RollicGames.Utils
{
    public partial class RevenueForwardingUtils
    {
        public void UpdateRevenue(MaxSdkBase.AdInfo adInfo)
        {
            var revenue = adInfo.Revenue;

            _lifeTimeRevenue += revenue;
            _revenueToSend += revenue;
            PlayerPrefs.SetFloat(KeyLtv, (float)_lifeTimeRevenue);
            PlayerPrefs.Save();

            PlayerPrefs.SetFloat(KeyRevenueToSend, (float)_revenueToSend);
            PlayerPrefs.Save();

            // day0 check is for first threshold
            if (IsInFirst24Hours())
            {
                SetLock(_revenueToSend >= _threshold);
            }
            // following checks for repetitive events with lower thresholds
            else
            {
                if (_firstForwarding)
                {
                    SetLock(_revenueToSend >= _threshold);
                }
                else
                {
                    SetLock(false);
                }
            }

            ForwardRevenue(_revenueToSend);
        }
        
        public void UpdateRevenue(float revenue)
        {
            _lifeTimeRevenue += revenue;
            _revenueToSend += revenue;
            PlayerPrefs.SetFloat(KeyLtv, (float)_lifeTimeRevenue);
            PlayerPrefs.Save();

            PlayerPrefs.SetFloat(KeyRevenueToSend, (float)_revenueToSend);
            PlayerPrefs.Save();

            // day0 check is for first threshold
            if (IsInFirst24Hours())
            {
                SetLock(_revenueToSend >= _threshold);
            }
            // following checks for repetitive events with lower thresholds
            else
            {
                if (_firstForwarding)
                {
                    SetLock(_revenueToSend >= _threshold);
                }
                else
                {
                    SetLock(false);
                }
            }

            ForwardRevenue(_revenueToSend);
        }
    }
}