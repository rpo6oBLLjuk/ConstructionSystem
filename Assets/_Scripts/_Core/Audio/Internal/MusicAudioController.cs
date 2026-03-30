using System;
using UnityEngine;
using Zenject;

[Serializable]
public class MusicAudioController
{
    [Inject] AudioConfig _config;
    [SerializeField] AudioSource _audioSource;

    public void PlayMusic(AudioClip clip)
    {
        _audioSource.clip = clip;
        _audioSource.Play();
    }

    public void ResumeMusic() => _audioSource.Play();
    public void PauseMusic() => _audioSource.Pause();

}
