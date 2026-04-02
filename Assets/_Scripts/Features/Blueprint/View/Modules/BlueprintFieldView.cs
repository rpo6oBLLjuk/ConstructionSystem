using UnityEngine;
using UnityEngine.UI;

public class BlueprintFieldView : BlueprintView<byte>
{
    protected override BlueprintViewLayers ViewLayer => BlueprintViewLayers.Grid;

    [SerializeField] Image _blueprint;
    [SerializeField] Color _blueprintColor;

    Sprite _blueprintSprite;


    protected override void OnEnable()
    {
        _blueprintSprite = _blueprint.sprite;
        base.OnEnable();
    }

    protected override void SetVisible(bool isVisible, float fadeDuration)
    {
        _blueprint.sprite = isVisible ? _blueprintSprite : null;
        _blueprint.color = isVisible ? Color.white : _blueprintColor;
    }
}
