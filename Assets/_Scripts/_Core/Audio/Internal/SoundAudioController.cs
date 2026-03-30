using System;
using UnityEngine;
using Zenject;

[Serializable]
public class SoundAudioController
{
    [Inject] AudioConfig _config;
    [SerializeField] AudioSource _sfxSource;
    [SerializeField] AudioSource _uiSource;

    public void PlaySFX(AudioClip clip) => _sfxSource.PlayOneShot(clip);
    public void PlayUISound(AudioClip clip) => _uiSource.PlayOneShot(clip);

    public void PlaySFX(AudioClip clip, Vector3 position)
    {
        GameObject instance = GameObject.Instantiate(_sfxSource.gameObject, position, Quaternion.identity, _sfxSource.transform);
        instance.GetComponent<AudioSource>().PlayOneShot(clip);
        GameObject.Destroy(instance, clip.length);
    }
}
