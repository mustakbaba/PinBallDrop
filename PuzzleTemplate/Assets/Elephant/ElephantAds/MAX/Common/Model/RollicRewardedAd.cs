using System.Runtime.Serialization;
using NUlid;

namespace ElephantSDK
{
    public partial class RollicRewardedAd
    {
        private const string Tag = "RollicRewardedAd";

        public string _category;
        public string _source;
        public string _item;
        public string adUuid;
        private string mediation_info;
        private string _eventType;
        private int _result = -1;

        private static void FireEvent(RollicRewardedAd rollicRewardedAd)
        {
            Elephant.RewardedEvent(rollicRewardedAd._eventType, MonitoringUtils.GetInstance().GetCurrentLevel(),
                rollicRewardedAd._category, rollicRewardedAd._source, rollicRewardedAd._item, rollicRewardedAd.adUuid,
                rollicRewardedAd._result, rollicRewardedAd.mediation_info);
        }

        public static RollicRewardedAd ShowTapped(RollicRewardedAd rollicRewardedAd, RewardedAdCategory category,
            RewardedAdSource source, string item)
        {
            rollicRewardedAd._eventType = RewardedAdEventType.Tapped;
            rollicRewardedAd._category = Utils.GetEnumMemberValue(category);
            rollicRewardedAd._source = Utils.GetEnumMemberValue(source);
            rollicRewardedAd._item = item;
            FireEvent(rollicRewardedAd);

            ElephantLog.Log(Tag, "ShowTapped " + rollicRewardedAd);
            return rollicRewardedAd;
        }

        public static void VideoFailedToPlay(RollicRewardedAd rollicRewardedAd)
        {
            rollicRewardedAd._eventType = RewardedAdEventType.ShowFailed;
            rollicRewardedAd._result = RewardedAdResult.ShowFailed;

            ElephantLog.Log(Tag, "VideoFailedToPlay " + rollicRewardedAd);
            FireEvent(rollicRewardedAd);
        }

        public static void VideoSkipped(RollicRewardedAd rollicRewardedAd)
        {
            rollicRewardedAd._result = RewardedAdResult.Success;
            rollicRewardedAd._eventType = RewardedAdEventType.Skipped;

            ElephantLog.Log(Tag, "VideoSkipped " + rollicRewardedAd);
            FireEvent(rollicRewardedAd);
        }

        public static RollicRewardedAd Impression(RollicRewardedAd rollicRewardedAd)
        {
            rollicRewardedAd._result = RewardedAdResult.Success;
            rollicRewardedAd._eventType = RewardedAdEventType.TypeImpression;

            ElephantLog.Log(Tag, "Impression " + rollicRewardedAd);
            return rollicRewardedAd;
        }

        public static RollicRewardedAd RefreshAd()
        {
            ElephantLog.Log(Tag, "RefreshAd");
            return new RollicRewardedAd
            {
                adUuid = Ulid.NewUlid().ToString()
            };
        }

        public override string ToString()
        {
            return
                $"{nameof(_category)}: {_category}, {nameof(_source)}: {_source}, {nameof(_item)}: {_item}, {nameof(adUuid)}: {adUuid}, {nameof(_eventType)}: {_eventType}, {nameof(_result)}: {_result}";
        }

        public enum RewardedAdCategory
        {
            [EnumMember(Value = "upgrade")] Upgrade,
            [EnumMember(Value = "cosmetic")] Cosmetic,
            [EnumMember(Value = "booster")] Booster,
            [EnumMember(Value = "income")] Income,
            [EnumMember(Value = "gotcha")] Gatcha,
            [EnumMember(Value = "level")] Level,
            [EnumMember(Value = "other")] Other
        }

        public enum RewardedAdSource
        {
            [EnumMember(Value = "normal_level")] NormalLevel,
            [EnumMember(Value = "shop")] Shop,
            [EnumMember(Value = "special_level")] SpecialLevel,
            [EnumMember(Value = "meta")] Meta,
            [EnumMember(Value = "iap_store")] IapStore,
            [EnumMember(Value = "live_event")] LiveEvent,
            [EnumMember(Value = "other")] Other
        }

        private class RewardedAdEventType
        {
            public const string Tapped = "ad_placement_rewarded_ad_tapped";
            public const string ShowFailed = "ad_placement_rewarded_ad_show_failed";
            public const string Skipped = "ad_placement_rewarded_ad_skipped";
            public const string TypeImpression = "ad_placement_rewarded_ad_impression";
        }

        private class RewardedAdResult
        {
            public const int Success = 0;
            public const int LoadFailed = 1;
            public const int ShowFailed = 2;
        }
    }
}