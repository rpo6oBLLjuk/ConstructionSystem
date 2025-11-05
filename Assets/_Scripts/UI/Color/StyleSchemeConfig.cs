using System;
using TMPro;
using UnityEngine;
using UnityEngine.TextCore.Text;

[CreateAssetMenu(fileName = "StyleSchemeConfig", menuName = "Scriptable Objects/UI/StyleSchemeConfig")]
public class StyleSchemeConfig : ScriptableObject
{
    public event Action OnColorChanged;
    public event Action OnFontSizeChanged;
    public event Action OnFontStyleChanged;

    [field: Header("Colors")]
    [field: SerializeField] public Color DefaultColor { get; private set; } = new Color32(226, 107, 0, 255);
    [field: SerializeField] public Color BackgroundColor { get; private set; } = new Color32(46, 45, 46, 255);

    [field: SerializeField] public Color SemiTransparentColor { get; private set; } = new Color32(36, 36, 36, 255);
    [field: SerializeField] public Color InteractableColor { get; private set; } = new Color32(55, 55, 55, 255);
    [field: SerializeField] public Color IconColor { get; private set; } = new Color32(203, 203, 203, 255);
    [field: SerializeField] public Color TextColor { get; private set; } = new Color32(203, 203, 203, 255);

    [field: Header("Fonts")]
    [field: SerializeField, Range(14, 50)] public int TitleFontSize { get; private set; } = 30;
    //[field: SerializeField, Range(14, 50)] public int TitleFontSize { get; private set; } = 30;
    [field: SerializeField, Range(14, 50)] public int DefaultFontSize { get; private set; } = 14;


    [field: Header("Font styles")]
    [field: SerializeField] public TMP_FontAsset DefaultFontStyle { get; private set; } 


    public Color GetColorByType(ColorShemeType type)
    {
        return type switch
        {
            ColorShemeType.Default => DefaultColor,
            ColorShemeType.Background => BackgroundColor,
            ColorShemeType.SemiTransparentColor => SemiTransparentColor,
            ColorShemeType.Interactable => InteractableColor,
            ColorShemeType.Icon => IconColor,
            ColorShemeType.Text => TextColor
        };
    }

    private void OnValidate() => OnColorChanged?.Invoke();
}


public enum ColorShemeType
{
    Default,
    Background,
    SemiTransparentColor,
    Interactable,
    Icon,
    Text
}

public enum FontSizeSchemeType
{

}
