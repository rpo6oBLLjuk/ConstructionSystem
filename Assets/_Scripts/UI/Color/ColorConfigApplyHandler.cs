using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

[RequireComponent(typeof(Graphics))]
public class ColorConfigApplyHandler : MonoBehaviour
{
    [Inject] ColorSchemeConfig _colorSchemeCfg;

    ColorSchemeConfig ColorSchemeCfg
    {
        get
        {
#if UNITY_EDITOR
#region Hand-made inject config from project files for editor use
            if (_colorSchemeCfg == null)
            {
                if (!Application.isPlaying)
                {
                    try
                    {
                        _colorSchemeCfg = AssetDatabase.LoadAssetAtPath<ColorSchemeConfig>(AssetDatabase.GUIDToAssetPath(AssetDatabase.FindAssets($"t:{typeof(ColorSchemeConfig).Name}")[0]));
                    }
                    catch
                    {
                        Debug.LogError("Check ColorSchemeConfig instance in project files!");
                    }
                }
                else
                {
                    ProjectContext.Instance.Container.Inject(this);
                }
            }
#endregion
#endif
            return _colorSchemeCfg;
        }
    }
    Graphic _graphicSource;

    [SerializeField] ColorShemeType _colorShemeType = ColorShemeType.Default;


    private void OnEnable() => ColorSchemeCfg.OnColorChanged += ApplyColor;
    private void OnDisable() => ColorSchemeCfg.OnColorChanged -= ApplyColor;

    private void Start() => ApplyColor();

    private void Reset()
    {
        GetGhaphicsComponent();
        ApplyColor();
    }
    private void OnValidate() => ApplyColor();

    private void ApplyColor()
    {
        GetGhaphicsComponent();
        Color color = ColorSchemeCfg.GetColorByType(_colorShemeType);

        if (_graphicSource != null)
            _graphicSource.color = color;
    }
    private void GetGhaphicsComponent()
    {
        if (_graphicSource)
            return;

        if (TryGetComponent(out Image image))
            _graphicSource = image;
        else if (TryGetComponent(out TMP_Text text))
        {
            _graphicSource = text;
            _colorShemeType = ColorShemeType.Text;
        }
    }
}
