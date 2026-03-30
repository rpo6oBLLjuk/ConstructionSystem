using UnityEngine;
using Zenject;

public class AudioService : MonoBehaviour
{
    [Inject] DiContainer _container;
    [SerializeField] MusicAudioController _musicAudioController;
    [SerializeField] SoundAudioController _soundAudioController;


    private void Start()
    {
        _container.Inject(_musicAudioController);
        _container.Inject(_soundAudioController);
    }

    public void PlayMusic(AudioClip clip) => _musicAudioController.PlayMusic(clip);
    public void ResumeMusic() => _musicAudioController.ResumeMusic();
    public void PauseMusic() => _musicAudioController.PauseMusic();

    public void PlaySFX(AudioClip clip) => _soundAudioController.PlaySFX(clip);
    public void PlaySFX(AudioClip clip, Vector3 position) => _soundAudioController.PlaySFX(clip, position);

    public void PlayUISound(AudioClip clip) => _soundAudioController.PlayUISound(clip);
}
