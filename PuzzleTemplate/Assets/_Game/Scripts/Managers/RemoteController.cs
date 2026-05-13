using System;
using System.Collections;
using System.Collections.Generic;
using ElephantSDK;
using SincappStudio;
using UnityEngine;

public class RemoteController : MonoSingleton<RemoteController>
{
    public float FillAddAmountEachLevel;
    public int LoopStartLevel;
    public bool IsDebugModeEnabled;
    
    protected override void Awake()
    {
        base.Awake();
        
        FillAddAmountEachLevel = RemoteConfig.GetInstance().GetFloat("FillAddAmountEachLevel", 0.1f);
        LoopStartLevel = RemoteConfig.GetInstance().GetInt("LoopStartLevel", 15);
        IsDebugModeEnabled = RemoteConfig.GetInstance().GetBool("IsDebugModeEnabled", false);
    }
}
