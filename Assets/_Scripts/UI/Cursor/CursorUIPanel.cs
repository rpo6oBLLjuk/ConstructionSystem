using UnityEngine;
using UnityEngine.EventSystems;

public class CursorUIPanel : MonoBehaviour, IPointerDownHandler
{
    [SerializeField] CursorController _cursorController;

    public void OnPointerDown(PointerEventData eventData) => _cursorController.ChangeCurcorState(false);
}
