using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoSingleton<SoundManager>
{
    [SerializeField] AudioSource _audioSource;
    [SerializeField] AudioSource _bumperSource;
    [SerializeField] AudioClip _balloonPop;
    [SerializeField] AudioClip _bumperPop;
    [SerializeField] AudioClip _slotClick;

    private float _lastPopTime;
    private float _currentPitch = .6f;
    private const float PitchIncrement = 0.075f;
    private const float MaxPitch = 1.5f;
    private const float PitchResetTime = 0.25f; // bu süre geçerse pitch sıfırla
    public void PlaySource()
    {
        _audioSource.PlayOneShot(_audioSource.clip);
    }
    public void BalloonPopSound()
    {
        _audioSource.PlayOneShot(_balloonPop);
    }   
    public void SlotClickSound()
    {
        _audioSource.PlayOneShot(_slotClick);
    }  
    public void BumperPopSound()
    {
        float timeSinceLast = Time.time - _lastPopTime;
    
        if (timeSinceLast > PitchResetTime)
            _currentPitch = 1f;
        else
            _currentPitch = Mathf.Min(_currentPitch + PitchIncrement, MaxPitch);

        _lastPopTime = Time.time;
        _bumperSource.pitch = _currentPitch;
        _bumperSource.PlayOneShot(_bumperPop);
    }
}

