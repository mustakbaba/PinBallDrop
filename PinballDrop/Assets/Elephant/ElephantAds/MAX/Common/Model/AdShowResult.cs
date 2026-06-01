namespace ElephantSDK
{
    public enum AdShowResult
    {
        Allowed,            // All rules pass
        AdFreePeriod,       // Within ad_free_days grace period
        TimerCooldown,      // Too soon since last ad
        LevelNotReached,    // Below first_level_to_display
        LevelFrequency,     // Not enough levels since last ad
        FirstDelayLock,     // Within first_interstitial_delay countdown
        BannerDelayLock,    // Within gamekit_ads_first_banner_delay countdown
        InterstitialDisabled, // gamekit_interstitial_enabled = false
        BannerDisabled      // gamekit_banner_enabled = false
    }
}
