using System;

namespace ElephantSDK
{
    [Serializable]
    public class AdInitConfig
    {
        public bool loadInterstitial = true;
        public bool loadRewarded = true;
        public bool loadBanner = false;
    }
}
