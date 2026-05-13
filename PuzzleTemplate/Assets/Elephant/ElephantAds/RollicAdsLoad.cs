using ElephantSDK;
using UnityEngine;

namespace RollicGames.Advertisements
{
    public class RollicAdsLoad
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void FirstSceneLoading()
        {
            if (ElephantCore.Instance == null)
            {
                Debug.LogWarning("RollicAds failed to load due to uninitialized ElephantCore. Check scene loading order.");
                return;
            }
            ElephantCore.Instance.AddAdapters(new RollicElephantManager());
        }
    }
}