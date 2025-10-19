using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class BlueprintLineHandler : BlueprintPointerHandler<Vector2>
{
    public Image SelfImage { get; private set; }

    private void Awake() => SelfImage ??= GetComponent<Image>();

    public override void OnPointerDown(PointerEventData eventData) => PointerDown?.Invoke(eventData.position);
    public override void OnPointerUp(PointerEventData eventData) => PointerUp?.Invoke(eventData.position);
}
