using System;
using UnityEngine;
using UnityEngine.Audio;

[CreateAssetMenu(fileName = "AudioConfig", menuName = "Scriptable Objects/Audio/Config")]
public class AudioConfig : ScriptableObject
{
    [field: Header("Master")]
    [field: SerializeField] MixerData Master = new();

    [field: Header("Sound Effects")]
    [field: SerializeField, Range(-80, 20)] public MixerData SFX = new();
    [field: SerializeField, Range(-80, 20)] public MixerData UI = new();

    [field: Header("Music")]
    [field: SerializeField, Range(-80, 20)] public MixerData Music = new();


    public void InitializeMixers()
    {
        Master.Initialize();
        SFX.Initialize();
        UI.Initialize();
        Music.Initialize();
    }
}

[Serializable]
public class MixerData
{
    [field: SerializeField] public AudioMixer Mixer { get; private set; }
    [field: SerializeField, Range(0, 100)] public Reactive<float> Volume { get; set; } = 50f;

    public void Initialize()
    {
        Volume.OnChanged += volume =>
        {
            Mixer.SetFloat("Volume", VolumeToDecibels(volume));
            Debug.Log("Volume Changed");
        };
    }

    public float VolumeToDecibels(float linear) => (linear <= 0) ? -80f : Mathf.Log10(linear / 100f) * 20f; //ѕлавна€ интерпол€ци€, т.к. децибелы нелинейны
    public float DecibelsToVolume(float db) => (db <= -80f) ? 0f : Mathf.Pow(10f, db / 20f) * 100f;
}
