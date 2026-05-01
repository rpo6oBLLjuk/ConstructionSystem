using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

[RequireComponent(typeof(Graphics))]
public class StyleSchemeConfigHandler : MonoBehaviour
{
    [Inject] StyleSchemeConfig _colorSchemeCfg;

    StyleSchemeConfig ColorSchemeCfg
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
                        _colorSchemeCfg = AssetDatabase.LoadAssetAtPath<StyleSchemeConfig>(AssetDatabase.GUIDToAssetPath(AssetDatabase.FindAssets($"t:{typeof(StyleSchemeConfig).Name}")[0]));
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

#if UNITY_EDITOR
    private void Reset()
    {
        ApplyColor();
    }
    private void OnValidate()
    {
        EditorApplication.delayCall += ApplyColor;
    }
#endif
    private void ApplyColor()
    {
        if (!GetGhaphicsComponent())
            return;

        if (_graphicSource.TryGetComponent(out Button btn))
        {
            ColorBlock colorBlock = btn.colors;

            colorBlock.normalColor = ColorSchemeCfg.ButtonBackgroundColor;
            colorBlock.selectedColor = ColorSchemeCfg.ButtonBackgroundColor;

            colorBlock.highlightedColor = ColorSchemeCfg.ButtonHighlightColor;
            colorBlock.pressedColor = ColorSchemeCfg.ButtonPressColor;

            btn.colors = colorBlock;

            _colorShemeType = ColorShemeType.Button;
        }
        else
        {
            Color color = ColorSchemeCfg.GetColorByType(_colorShemeType);
            if (_graphicSource != null)
                _graphicSource.color = color;
        }
    }
    private bool GetGhaphicsComponent()
    {
        if (_graphicSource)
            return true;

        if(enabled == false)
            return false;

        if (TryGetComponent(out Button btn))
        {
            _graphicSource = GetComponent<Image>();
            _colorShemeType = ColorShemeType.Button;

            return true;
        }
        else if (TryGetComponent(out TMP_Text text))
        {
            _graphicSource = text;

            if (GetComponentInParent<Button>())
                _colorShemeType = ColorShemeType.ButtonText;


            return true;
        }
        else if (TryGetComponent(out Image image))
        {
            _graphicSource = image;
            return true;
        }
        else
            return false;

    }
}
