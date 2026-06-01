using UnityEngine;

namespace ElephantSDK
{
    public class RollicAnimationSound : MonoBehaviour
    {
        [SerializeField] private AudioSource _audioSource;

        public void PlaySound()
        { 
            if (_audioSource == null)
            {
                ElephantLog.LogError("Audio", "AudioSource missing on " + gameObject.name);
                return;
            }
        
            var isAnimationSoundEnabled = RemoteConfig.GetInstance().GetBool("animation_sound_enabled", true);
            var isInGameSoundOn = Elephant.IsInGameSoundEnabled();
            if (isAnimationSoundEnabled && isInGameSoundOn)
            {
                _audioSource.Play();
            }
        }
    }
}
