using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

[ExecuteAlways]
[RequireComponent(typeof(Graphic))]
public class StyleSchemeConfigHandler : MonoBehaviour
{
    [Inject] private StyleSchemeConfig _colorSchemeCfg;

    private StyleSchemeConfig ColorSchemeCfg
    {
        get
        {
#if UNITY_EDITOR
            if (_colorSchemeCfg == null && !Application.isPlaying)
            {
                var assets = AssetDatabase.FindAssets($"t:{typeof(StyleSchemeConfig).Name}");
                if (assets.Length > 0)
                {
                    string path = AssetDatabase.GUIDToAssetPath(assets[0]);
                    _colorSchemeCfg = AssetDatabase.LoadAssetAtPath<StyleSchemeConfig>(path);
                }
            }
#endif
            return _colorSchemeCfg;
        }
    }

    private Graphic _graphicSource;
    [SerializeField] private ColorShemeType _colorShemeType = ColorShemeType.Default;

    private void OnEnable()
    {
        if (ColorSchemeCfg != null)
            ColorSchemeCfg.OnColorChanged += ApplyColor;
    }

    private void OnDisable()
    {
        if (_colorSchemeCfg != null)
            _colorSchemeCfg.OnColorChanged -= ApplyColor;

#if UNITY_EDITOR
        EditorApplication.delayCall -= ApplyColor;
#endif
    }

    private void Start()
    {
        if (ColorSchemeCfg == null && Application.isPlaying)
        {
            //DebugWrapper.InactiveLog(this, $"Zenject injection failed on {gameObject.name}. Object might be missing from Context");
            return;
        }
        ApplyColor();
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (this == null)
            return;

        EditorApplication.delayCall -= ApplyColor;
        EditorApplication.delayCall += ApplyColor;
    }

    private void OnDestroy()
    {
        EditorApplication.delayCall -= ApplyColor;
    }

#endif

    private void ApplyColor()
    {
        if (this == null || ColorSchemeCfg == null)
            return;

        if (!GetGhaphicsComponent())
            return;

        if (_graphicSource == null)
        {
            //DebugWrapper.InactiveLog($"[StyleSchemeConfigHandler] _graphicSource is null on {gameObject.name} even after GetGhaphicsComponent!", this);
            return;
        }

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
            _graphicSource.color = color;
        }
    }


    private bool GetGhaphicsComponent()
    {
        if (_graphicSource != null && _graphicSource.gameObject != null)
            return true;

        if (!enabled)
            return false;

        switch (_colorShemeType)
        {
            case ColorShemeType.Button:
                if (TryGetComponent(out Button btn))
                {
                    _graphicSource = GetComponent<Image>() ?? GetComponent<Graphic>();
                }
                break;

            case ColorShemeType.ButtonText:
            case ColorShemeType.Text: 
                _graphicSource = GetComponent<TMP_Text>() ?? GetComponentInChildren<TMP_Text>();
                break;

            case ColorShemeType.Default:
            default:
                _graphicSource = GetComponent<TMP_Text>() as Graphic
                                 ?? GetComponent<Image>()
                                 ?? GetComponent<Graphic>();
                break;
        }


        return false;
    }

}
