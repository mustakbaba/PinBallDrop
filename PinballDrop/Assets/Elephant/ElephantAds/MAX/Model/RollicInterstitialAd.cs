namespace ElephantSDK
{
    public partial class RollicInterstitialAd
    { 
        public static RollicInterstitialAd VideoAdReady(RollicInterstitialAd rollicInterstitialAd, Ilrd ilrd)
        {
            rollicInterstitialAd.mediation_info = ilrd.creativeId + "|" + ilrd.networkName + "|" + ilrd.placement;
            ElephantLog.Log(Tag, "VideoFailedToPlay " + rollicInterstitialAd);

            return rollicInterstitialAd;
        }
    }
}