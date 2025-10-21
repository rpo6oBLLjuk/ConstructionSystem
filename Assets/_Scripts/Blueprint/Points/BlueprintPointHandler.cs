using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class BlueprintPointHandler : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerClickHandler
{
    public Action<Image> PointerDown;
    public Action<Image> PointerUp;

    public Action<BlueprintPointHandler> PointerLeftClick;

    public Image SelfImage { get; private set; }

    private void Awake() => SelfImage ??= GetComponent<Image>();

    public void OnPointerDown(PointerEventData eventData) => PointerDown?.Invoke(SelfImage);
    public void OnPointerUp(PointerEventData eventData) => PointerUp?.Invoke(SelfImage);

    public void OnPointerClick(PointerEventData eventData)
    {
        if(eventData.button == PointerEventData.InputButton.Right)
            PointerLeftClick?.Invoke(this);
    }
}
