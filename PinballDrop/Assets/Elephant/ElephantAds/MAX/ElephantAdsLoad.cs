using UnityEngine;

namespace ElephantSDK
{
    public class ElephantAdsLoad : MonoBehaviour
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void FirstSceneLoading()
        {
            if (ElephantCore.Instance == null)
            {
                Debug.LogWarning("Elephant-Ads failed to load due to uninitialized ElephantCore. Check scene loading order.");
                return;
            }
            ElephantCore.Instance.AddAdapters(new ElephantAdsManager());
        }
    }
}