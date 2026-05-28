using System;
using UnityEngine;

[Serializable]
public class NavigationDataContainer
{
    [field: SerializeField] public string Title { get; private set; }
    [field: SerializeField] public string Description { get; private set; }
    [field: SerializeField] public Sprite Preview { get; private set; }
}
