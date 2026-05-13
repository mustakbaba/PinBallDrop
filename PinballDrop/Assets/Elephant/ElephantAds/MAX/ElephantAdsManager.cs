using RollicGames.Advertisements;

namespace ElephantSDK
{
    public class ElephantAdsManager : IElephantAdsAdapter
    {
        public void StartAdManager()
        {
            RLAdvertisementManager.Instance.InitInternal();
        }
    }
}