using System;
using System.Collections.Generic;
using AdjustSdk;

namespace ElephantSDK
{
    public class ElephantAdjustManager : IAdjustElephantAdapter
    { 
        public void InitAdjust(string adjustAppKey, bool isCvUpdateServiceEnabled, Action<string> deepLinkCallback, bool isLowerThanIos145 = false)
        {
            ElephantLog.Log("ADJUST-ELEPHANT", "InitAdjust is Called");
            
            if (string.IsNullOrEmpty(adjustAppKey))
            {
                ElephantLog.LogError("ElephantAdjustManager", "AdjustAppKey is not filled, check Elephant Dashboard");
                return;
            }
            
            var config = new AdjustConfig(ElephantThirdPartyIds.AdjustAppKey, AdjustEnvironment.Production);
            config.AttributionChangedDelegate = OnAttrChange;
            config.FbAppId = ElephantThirdPartyIds.FacebookAppId;
            if (!isLowerThanIos145 && RemoteConfig.GetInstance().GetBool("conversion_value_service_enabled", false))
                config.IsSkanAttributionEnabled = false;
            config.IsDeferredDeeplinkOpeningEnabled = true;
            config.DeferredDeeplinkDelegate = deepLinkCallback;
            Adjust.InitSdk(config);
            ElephantCore.Instance.ZyngaPublishingElephantAdapter?.LogAdjustInitializedEvent();
        }
        
        private void OnAttrChange(AdjustAttribution adjustAttribution)
        {
            var adjustId = "";
            GetAdid(adId => {
                adjustId = adId;
                ElephantCore.Instance.adjustId = adId;
            });
            
            ElephantCore.Instance.networkName = adjustAttribution.Network;
            ElephantCore.Instance.campaignName = adjustAttribution.Campaign;
            ElephantCore.Instance.adGroupName = adjustAttribution.Adgroup;
            ElephantCore.Instance.creativeName = adjustAttribution.Creative;
            if (adjustAttribution.CostAmount != null)
            {
                ElephantCore.Instance.uaCost = (double)adjustAttribution.CostAmount;
            }
            
            ElephantLog.Log("Adjust attr",adjustId);
            ElephantLog.Log("Adjust attr",adjustAttribution.Network);
        }


        public void TrackPurchaseRevenue(string token, double revenue, string currency)
        {
            ElephantLog.Log("ADJUST-ELEPHANT", "TrackPurchaseRevenue is Called");
            var adjustEvent = new AdjustEvent(token);
            adjustEvent.SetRevenue(revenue, currency);
            Adjust.TrackEvent(adjustEvent);
        }

        public void TrackAdjustEvent(string token)
        {
            ElephantLog.Log("ADJUST-ELEPHANT", "TrackAdjustEvent is Called");
            var adjustEvent = new AdjustEvent(token);
            Adjust.TrackEvent(adjustEvent);
        }

        public void SetTrackThirdPartySharing(bool isEea, bool adPersonalizationStatus, bool adUserDataStatus)
        {
            ElephantLog.Log("ADJUST-ELEPHANT", "SetTrackThirdPartySharing is Called");
            var adjustThirdPartySharing = new AdjustThirdPartySharing(null);
            adjustThirdPartySharing.AddGranularOption("google_dma", "eea", isEea ? "1" : "0");
            adjustThirdPartySharing.AddGranularOption("google_dma", "ad_personalization", adPersonalizationStatus ? "1" : "0");
            adjustThirdPartySharing.AddGranularOption("google_dma", "ad_user_data", adUserDataStatus ? "1" : "0");
            Adjust.TrackThirdPartySharing(adjustThirdPartySharing);
        }

        public void SetTrackThirdPartySharing(bool isEea)
        {
            ElephantLog.Log("ADJUST-ELEPHANT", "SetTrackThirdPartySharing is Called");
            var adjustThirdPartySharing = new AdjustThirdPartySharing(null);
            adjustThirdPartySharing.AddGranularOption("google_dma", "eea", isEea ? "1" : "0");
            adjustThirdPartySharing.AddGranularOption("google_dma", "ad_personalization", "1");
            adjustThirdPartySharing.AddGranularOption("google_dma", "ad_user_data", "1");
            Adjust.TrackThirdPartySharing(adjustThirdPartySharing);
        }
        
        public void SetTrackThirdPartySharingForCcpa(bool enabled)
        {
            ElephantLog.Log("ADJUST-ELEPHANT", "SetAdjustConsentCcpa is Called");
            var adjustThirdPartySharing = new AdjustThirdPartySharing(enabled);
            Adjust.TrackThirdPartySharing(adjustThirdPartySharing);
        }

        public string IronSourceAdRevenueSource => "ironsource_sdk";

        public string AppLovinMAXRevenueSource => "applovin_max_sdk";

        public void TrackAdRevenue(
            string source,
            double revenue,
            string currency,
            string network,
            string unit,
            string placement,
            string format = null,
            string adUnitId = null,
            Dictionary<string, string> extraParams = null)
        {
            var adRevenue = new AdjustAdRevenue(source);
            adRevenue.SetRevenue(revenue, currency);
            adRevenue.AdRevenueNetwork = network;
            adRevenue.AdRevenueUnit = unit;
            adRevenue.AdRevenuePlacement = placement;

            if (!string.IsNullOrEmpty(format))
                adRevenue.AddPartnerParameter("ad_format", format);
            
            if (!string.IsNullOrEmpty(adUnitId))
                adRevenue.AddPartnerParameter("ad_unit_id", adUnitId);

            if (extraParams != null)
            {
                foreach (var param in extraParams)
                {
                    adRevenue.AddCallbackParameter(param.Key, param.Value);
                }
            }

            Adjust.TrackAdRevenue(adRevenue);
        }

        public void GetAdid(Action<string> callback)
        {
            Adjust.GetAdid(adId => {
                callback?.Invoke(adId);
            });
        }

        public void AddSessionCallbackParameter(string key, string value)
        {
            Adjust.AddGlobalCallbackParameter(key, value);
        }
    }
}