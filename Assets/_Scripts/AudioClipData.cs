using UnityEngine;

public class AudioClipData
{
    [field: SerializeField] public AudioClip Clip { get; private set; }
    [field: SerializeField, Range(0, 100)] public float Volume {  get; private set; }
}