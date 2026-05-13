namespace ElephantSDK
{
    public partial class RollicRewardedAd
    { 
        public static RollicRewardedAd VideoShown(RollicRewardedAd rollicRewardedAd, Ilrd ilrd)
        {
            rollicRewardedAd._result = RewardedAdResult.Success;
            rollicRewardedAd._eventType = RewardedAdEventType.Tapped;
            rollicRewardedAd.mediation_info = ilrd.creativeId + "|" + ilrd.networkName + "|" + ilrd.placement;

            ElephantLog.Log(Tag, "VideoShown " + rollicRewardedAd);
            return rollicRewardedAd;
        }
    }
}