using UnityEngine;
using UnityEngine.SceneManagement;

namespace Scripts.Managers
{
    public class AdditiveManager : MonoBehaviour
    {
        private void Awake()
        {
            SceneManager.LoadScene("Additive Scene", LoadSceneMode.Additive);
        }
    }
}
