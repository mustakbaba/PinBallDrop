namespace ElephantSDK
{
    public interface IRollicAdsElephantAdapter : IElephantAdapter
    {
        void LogLtv(float usdPrice, bool isCvServiceEnabled);
        void LogIapLtv(float usdPrice);
    }
}