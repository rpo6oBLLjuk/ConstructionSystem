using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class BlueprintPointHandler : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public Action<Image> onPointerDown;
    public Action<Image> onPointerUp;

    [SerializeField] Image image;

    public void OnPointerDown(PointerEventData eventData) => onPointerDown?.Invoke(image);
    public void OnPointerUp(PointerEventData eventData) => onPointerUp?.Invoke(image);
}
