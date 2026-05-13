using System;
using System.Collections.Generic;

namespace ElephantSDK
{
    public interface IAdjustElephantAdapter : IElephantAdapter
    {
        void InitAdjust(string adjustAppKey, bool isCvUpdateServiceEnabled, Action<string> deepLinkCallback, bool isLowerThanIos145 = false);

        void TrackAdjustEvent(string token);

        void SetTrackThirdPartySharing(bool isEea, bool adPersonalizationStatus, bool adUserDataStatus);
        
        void SetTrackThirdPartySharing(bool isEea);
        void SetTrackThirdPartySharingForCcpa(bool enabled);
        string IronSourceAdRevenueSource { get; }
        string AppLovinMAXRevenueSource { get; }

        void TrackAdRevenue(
            string source,
            double revenue,
            string currency,
            string network,
            string unit,
            string placement,
            string format = null,
            string adUnitId = null,
            Dictionary<string, string> extraParams = null
        );

        void GetAdid(Action<string> callback);
        void AddSessionCallbackParameter(string key, string value);
    }
}