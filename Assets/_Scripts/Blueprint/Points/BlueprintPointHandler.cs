using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class BlueprintPointHandler : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public Action<Image> PointerDown;
    public Action<Image> PointerUp;

    public Image SelfImage { get; private set; }

    private void Awake() => SelfImage ??= GetComponent<Image>();

    public void OnPointerDown(PointerEventData eventData) => PointerDown?.Invoke(SelfImage);
    public void OnPointerUp(PointerEventData eventData) => PointerUp?.Invoke(SelfImage);
}
