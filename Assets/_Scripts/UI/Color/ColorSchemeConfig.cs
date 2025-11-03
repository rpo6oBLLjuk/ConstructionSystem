using System;
using UnityEngine;

[CreateAssetMenu(fileName = "ColorSchemeConfig", menuName = "Scriptable Objects/UI/ColorSchemeConfig")]
public class ColorSchemeConfig : ScriptableObject
{
    public event Action OnColorChanged;

    [field: SerializeField] public Color DefaultColor { get; private set; } = new Color32(226, 107, 0, 255);
    [field: SerializeField] public Color BackgroundColor { get; private set; } = new Color32(36, 36, 36, 255);

    [field: SerializeField] public Color InteractableColor { get; private set; } = Color.white;

    [field: SerializeField] public Color IconColor { get; private set; } = Color.white;

    [field: SerializeField] public Color TextColor { get; private set; } = Color.white;


    public Color GetColorByType(ColorShemeType type)
    {
        return type switch
        {
            ColorShemeType.Default => DefaultColor,
            ColorShemeType.Background => BackgroundColor,
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
    Interactable,
    Icon,
    Text
}