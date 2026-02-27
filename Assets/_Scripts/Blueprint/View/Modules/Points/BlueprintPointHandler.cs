using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class BlueprintPointHandler : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerClickHandler
{
    public Action<BlueprintPointHandler> PointerDown;
    public Action<BlueprintPointHandler> PointerUp;

    public Action<BlueprintPointHandler> PointerLeftClick;

    public Image SelfImage { get; private set; }


    private void Awake() => SelfImage ??= GetComponent<Image>();

    public void OnPointerDown(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
            PointerDown?.Invoke(this);
    }
    public void OnPointerUp(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
            PointerUp?.Invoke(this);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Right)
            PointerLeftClick?.Invoke(this);
    }
}
