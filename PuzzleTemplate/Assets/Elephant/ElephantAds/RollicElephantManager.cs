using ElephantSDK;
using RollicGames.Utils;

namespace RollicGames.Advertisements
{
    public class RollicElephantManager : IRollicAdsElephantAdapter
    {
        public void LogLtv(float usdPrice, bool isCvServiceEnabled)
        {
            LtvManager.GetInstance().UpdateRevenue(usdPrice);
        }
        
        public void LogIapLtv(float usdPrice)
        {
            LtvManager.GetInstance().UpdateIAPRevenue(usdPrice);
        }
    }
}