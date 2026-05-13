#if !UNITY_EDITOR
using UnityEngine;

namespace ElephantSDK
{
    public class ElephantFacebookLoad
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void FirstSceneLoading()
        {
            if (ElephantCore.Instance == null)
            {
                Debug.LogWarning("Elephant-Facebook failed to load due to uninitialized ElephantCore. Check scene loading order.");
                return;
            }
            ElephantCore.Instance.AddAdapters(new ElephantFacebookManager());
        }    
    }
}
#endif