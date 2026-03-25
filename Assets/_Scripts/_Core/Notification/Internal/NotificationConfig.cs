using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NotificationConfig", menuName = "ScriptableObjects/Notification/Config")]
public class NotificationConfig : ScriptableObject
{
    [field: Header("Global")]
    [field: SerializeField] public AudioClipData PopupAudio { get; private set; }

    [field: Header("Sprites")]
    [field: SerializeField] public Sprite InfoSprite { get; private set; }
    [field: SerializeField] public Sprite WarningSprite { get; private set; }
    [field: SerializeField] public Sprite ErrorSprite { get; private set; }

    [field: Header("Popup")]
    [field: SerializeField] public float PopupShowDuration { get; private set; } = 0.25f;
    [field: SerializeField] public float PopupAliveDuration { get; private set; } = 3f;
    [field: SerializeField] public float PopupHideDuration { get; private set; } = 0.25f;

    [field: Header("Dialog")]
    [field: SerializeField] public float DialogShowDuration { get; private set; } = 0.25f;
    [field: SerializeField] public float DialogHideDuration { get; private set; } = 0.25f;


}
