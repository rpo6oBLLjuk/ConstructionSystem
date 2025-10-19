using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class BlueprintPointerHandler<T> : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public Action<T> PointerDown;
    public Action<T> PointerUp;

    public virtual void OnPointerDown(PointerEventData eventData) { }
    public virtual void OnPointerUp(PointerEventData eventData) { }
}
