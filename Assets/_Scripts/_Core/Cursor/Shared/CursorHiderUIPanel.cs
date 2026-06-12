using UnityEngine;
using UnityEngine.EventSystems;
using Zenject;

public class CursorHiderUIPanel : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [Inject] CursorController _cursorController;

    
    public void OnPointerDown(PointerEventData eventData) => _cursorController.ChangeCurcorState(false);
    public void OnPointerUp(PointerEventData eventData) => _cursorController.ChangeCurcorState(true);
}
