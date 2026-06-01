using System.Collections.Generic;

namespace ElephantSDK
{
    public interface IElephantAdsAdapter : IElephantAdapter
    {
        void StartAdManager();
        void TrackEvent(string eventName, IDictionary<string, string> parameters = null);
    }
}