using UnityEngine;
using UnityEngine.EventSystems;

public class CursorHiderUIPanel : MonoBehaviour, IPointerDownHandler
{
    [SerializeField] CursorController _cursorController;

    public void OnPointerDown(PointerEventData eventData) => _cursorController.ChangeCurcorState(false);
}
