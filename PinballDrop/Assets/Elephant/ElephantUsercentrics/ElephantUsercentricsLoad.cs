using UnityEngine;

namespace ElephantSDK
{
    public class ElephantUsercentricsLoad : MonoBehaviour
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void FirstSceneLoading()
        {
            if (ElephantCore.Instance == null)
            {
                Debug.LogWarning("Elephant-Usercentrics failed to load due to uninitialized ElephantCore. Check scene loading order.");
                return;
            }
            ElephantCore.Instance.AddAdapters(new ElephantUsercentricsManager());
        }
    }
}