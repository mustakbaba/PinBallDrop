using UnityEngine;

namespace ElephantSDK
{
    public class ElephantPushLoad
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void FirstSceneLoading()
        {
            if (ElephantCore.Instance == null)
            {
                Debug.LogWarning("Elephant-Push failed to load due to uninitialized ElephantCore. Check scene loading order.");
                return;
            }
            ElephantCore.Instance.AddAdapters(new ElephantPushElephantManager());
        }    
    }
}