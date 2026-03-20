using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Zenject;

[RequireComponent(typeof(Image))]
public class BlueprintPointHandler : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerClickHandler
{
    [Inject] CursorController _cursorController;

    public Action<BlueprintPointHandler> PointerDown;
    public Action<BlueprintPointHandler> PointerUp;

    public Action<BlueprintPointHandler> PointerLeftClick;

    public Image SelfImage { get; private set; }


    private void Awake() => SelfImage ??= GetComponent<Image>();

    public void OnPointerDown(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
            PointerDown?.Invoke(this);

        _cursorController.ChangeLockState(CursorLockMode.Confined);
    }
    public void OnPointerUp(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
            PointerUp?.Invoke(this);

        _cursorController.ChangeLockState(CursorLockMode.None);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Right)
            PointerLeftClick?.Invoke(this);
    }
}
