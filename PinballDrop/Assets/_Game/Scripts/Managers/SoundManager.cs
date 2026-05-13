using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    [SerializeField] AudioSource _audioSource;

    public void PlaySource()
    {
        _audioSource.PlayOneShot(_audioSource.clip);
    } 
}
