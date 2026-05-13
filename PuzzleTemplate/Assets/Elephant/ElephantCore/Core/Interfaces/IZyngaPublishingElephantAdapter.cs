namespace ElephantSDK
{
    public interface IZyngaPublishingElephantAdapter : IElephantAdapter
    {
        string ZyngaGameId { get; set; }
        string RollicUserId { get; set; }
        void Initialize();
        void Stop();
        void LogAdjustInitializedEvent();
        void LogShowTosEvent();
        void LogAcceptTosEvent();
        void LogPurchaseEvent(IapVerifyRequest request, IapVerification verification);
        void LogAdLoadEvent(string impressionId);
        void LogAdLoadedEvent(string impressionId, string creativeId, long loadMs);
        void LogAdLoadedDetailsEvent(string adUnitId, string impressionId, string networkName, ZPAdFormat adFormat, double revenue);
        void LogAdLoadFailedEvent(string impressionId, long loadMs, string errorCode, string errorMessage);
        void LogAdClickEvent(string impressionId, string creativeId, long loadMs);
        void LogAdFailedEvent(string impressionId, string creativeId, long loadMs, string errorCode, string errorMessage);
        void LogAdImpressionEvent(string adUnitId, string impressionId, ZPAdFormat adFormat, string networkAdaptor, string networkPlacementId, double revenue, string revenueAccuracy);
        void LogGameEconomyEvent(string action, string currency, long amount, long aggregateTotalStartingBalance, long aggregateTotalEndingBalance);
    }
    
    public enum ZPAdFormat
    {
        Banner,
        Interstitial,
        Rewarded
    }
}