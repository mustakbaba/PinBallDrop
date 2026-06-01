using System.Collections.Generic;
using RollicGames.Advertisements;

namespace ElephantSDK
{
    public class ElephantAdsManager : IElephantAdsAdapter
    {
        public void StartAdManager()
        {
            RLAdvertisementManager.Instance.InitInternal();
        }

        public void TrackEvent(string eventName, IDictionary<string, string> parameters = null)
        {
            MaxSdk.TrackEvent(eventName, parameters);
        }
    }
}