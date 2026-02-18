using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class BlueprintLineHandler : MonoBehaviour, IPointerClickHandler
{
    public event Action<BlueprintLineHandler, Vector2> PointerClicked;
    public Image SelfImage { get; private set; }

    private void Awake() => SelfImage ??= GetComponent<Image>();

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
            PointerClicked?.Invoke(this, eventData.position);
    }
}
