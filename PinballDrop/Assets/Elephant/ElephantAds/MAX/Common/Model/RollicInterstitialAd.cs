using System.Runtime.Serialization;
using NUlid;

namespace ElephantSDK
{
    public partial class RollicInterstitialAd
    {
        private const string Tag = "RollicInterstitialAd";

        public string _source;
        public string adUuid;
        private string mediation_info;
        private string _eventType;
        private int _result = -1;

        private static void FireEvent(RollicInterstitialAd rollicInterstitialAd)
        {
            Elephant.InterstitialEvent(rollicInterstitialAd._eventType, MonitoringUtils.GetInstance().GetCurrentLevel(),
                rollicInterstitialAd._source, rollicInterstitialAd.adUuid, rollicInterstitialAd._result,
                rollicInterstitialAd.mediation_info);
        }

        public static RollicInterstitialAd ShowCalled(RollicInterstitialAd rollicInterstitialAd,
            InterstitialAdSource source)
        {
            rollicInterstitialAd._eventType = InterstitialAdEventType.TypeShowCalled;
            rollicInterstitialAd._source = Utils.GetEnumMemberValue(source);
            FireEvent(rollicInterstitialAd);

            ElephantLog.Log(Tag, "ShowCalled " + rollicInterstitialAd);
            return rollicInterstitialAd;
        }

        public static void VideoFailedToPlay(RollicInterstitialAd rollicInterstitialAd)
        {
            rollicInterstitialAd._eventType = InterstitialAdEventType.ShowFailed;
            rollicInterstitialAd._result = InterstitialAdResult.ShowFailed;

            ElephantLog.Log(Tag, "VideoFailedToPlay " + rollicInterstitialAd);
            FireEvent(rollicInterstitialAd);
        }

        public static RollicInterstitialAd VideoFailedToLoad(RollicInterstitialAd rollicInterstitialAd)
        {
            rollicInterstitialAd._result = InterstitialAdResult.LoadFailed;
            ElephantLog.Log(Tag, "VideoFailedToPlay " + rollicInterstitialAd);

            return rollicInterstitialAd;
        }

        public static RollicInterstitialAd VideoShown(RollicInterstitialAd rollicInterstitialAd)
        {
            rollicInterstitialAd._eventType = InterstitialAdEventType.TypeShowCalled;
            rollicInterstitialAd._result = InterstitialAdResult.Success;

            ElephantLog.Log(Tag, "VideoShown " + rollicInterstitialAd);
            return rollicInterstitialAd;
        }

        public static void VideoClosed(RollicInterstitialAd rollicInterstitialAd)
        {
            rollicInterstitialAd._eventType = InterstitialAdEventType.Closed;
            rollicInterstitialAd._result = InterstitialAdResult.Success;

            ElephantLog.Log(Tag, "VideoClosed " + rollicInterstitialAd);
            FireEvent(rollicInterstitialAd);
        }

        public static RollicInterstitialAd Impression(RollicInterstitialAd rollicInterstitialAd)
        {
            rollicInterstitialAd._eventType = InterstitialAdEventType.TypeImpression;
            rollicInterstitialAd._result = InterstitialAdResult.Success;

            ElephantLog.Log(Tag, "Impression " + rollicInterstitialAd);
            return rollicInterstitialAd;
        }

        public static RollicInterstitialAd RefreshAd()
        {
            ElephantLog.Log(Tag, "RefreshAd");
            return new RollicInterstitialAd
            {
                adUuid = Ulid.NewUlid().ToString()
            };
        }

        public override string ToString()
        {
            return
                $"{nameof(_source)}: {_source}, {nameof(adUuid)}: {adUuid}, {nameof(_eventType)}: {_eventType}, {nameof(_result)}: {_result}";
        }

        public enum InterstitialAdSource
        {
            [EnumMember(Value = "in_level")] InLevel,
            [EnumMember(Value = "level_complete")] LevelComplete,
            [EnumMember(Value = "level_fail")] LevelFail,
            [EnumMember(Value = "restart")] Restart,
            [EnumMember(Value = "in_meta")] InMeta,
            [EnumMember(Value = "in_side_loop")] InSideLoop,
            [EnumMember(Value = "other")] Other
        }

        private class InterstitialAdEventType
        {
            public const string TypeShowCalled = "ad_placement_interstitial_ad_show_called";
            public const string ShowFailed = "ad_placement_interstitial_ad_show_failed";
            public const string Closed = "ad_placement_interstitial_ad_closed";
            public const string TypeImpression = "ad_placement_interstitial_ad_impression";
        }

        private class InterstitialAdResult
        {
            public const int Success = 0;
            public const int LoadFailed = 1;
            public const int ShowFailed = 2;
        }
    }
}