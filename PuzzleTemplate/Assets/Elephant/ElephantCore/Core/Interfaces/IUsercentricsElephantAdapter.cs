using System;

namespace ElephantSDK
{
    public interface IUsercentricsElephantAdapter : IElephantAdapter
    {
        void InitializeUc(bool isUcEnabled, bool isEea, bool isUcForced, bool isAutoDeny, bool shouldInitWithDelay,Action<bool, bool> onInitialize, Action onAdjustTrackingSet, Action onLoadGame);
        void ShowSecondLayer();
        bool DidCrashlyticsConsentSet();
        bool DidAnalyticsConsentSet();
        bool DidAdjustConsentSet();
        bool GetCrashlyticsConsentStatus();
        bool GetAnalyticsConsentStatus();
        bool GetAdjustConsentStatus();
        bool GetIsUcInitialized();
    }
}