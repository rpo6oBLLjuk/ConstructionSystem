using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class BlueprintPointHandler : BlueprintPointerHandler<Image>
{
    public Image SelfImage { get; private set; }

    private void Awake() => SelfImage ??= GetComponent<Image>();

    public override void OnPointerDown(PointerEventData eventData) => PointerDown?.Invoke(SelfImage);
    public override void OnPointerUp(PointerEventData eventData) => PointerUp?.Invoke(SelfImage);
}
