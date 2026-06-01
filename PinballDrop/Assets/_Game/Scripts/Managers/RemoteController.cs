using System;
using System.Collections;
using System.Collections.Generic;
using ElephantSDK;
using RollicGames.Advertisements;
using SincappStudio;
using UnityEngine;

public class RemoteController : MonoSingleton<RemoteController>
{
    public float FillAddAmountEachLevel;
    public int LoopStartLevel;
    public bool IsDebugModeEnabled;
    public bool IsAdsReady { get; private set; }

    protected override void Awake()
    {
        base.Awake();
        
        FillAddAmountEachLevel = RemoteConfig.GetInstance().GetFloat("FillAddAmountEachLevel", 0.1f);
        LoopStartLevel = RemoteConfig.GetInstance().GetInt("LoopStartLevel", 15);
        IsDebugModeEnabled = RemoteConfig.GetInstance().GetBool("IsDebugModeEnabled", false);
        
        if (!Application.isEditor)
        {
            RLAdvertisementManager.Instance.Init(new AdInitConfig
            {
                loadInterstitial = true,
                loadRewarded = true,
                loadBanner = false
            });

            RLAdvertisementManager.OnRollicAdsSdkInitializedEvent += OnAdsReady;
        }
    }
    
        public void OnAdsReady()
        {
            IsAdsReady = true;
    
            var bannerRules = RLAdvertisementManager.Instance.ShouldShowBanner();
    
            if (bannerRules == AdShowResult.Allowed)
            {
                RLAdvertisementManager.Instance.loadBanner();
            }
        }
}
