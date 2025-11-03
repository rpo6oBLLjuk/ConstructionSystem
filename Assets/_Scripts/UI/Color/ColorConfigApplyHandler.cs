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
//#if UNITY_EDITOR
//            //Hand-made inject config from project files for editor use
//            if (_colorSchemeCfg == null)
//            {
//                if (!Application.isPlaying)
//                {
//                    try
//                    {
//                        _colorSchemeCfg = AssetDatabase.LoadAssetAtPath<ColorSchemeConfig>(AssetDatabase.GUIDToAssetPath(AssetDatabase.FindAssets($"t:{typeof(ColorSchemeConfig).Name}")[0]));
//                    }
//                    catch
//                    {
//                        Debug.LogError("Check ColorSchemeConfig instance in project files!");
//                    }
//                }
//                else
//                {
//                    ProjectContext.Instance.Container.Inject(this);
//                }
//            }
//#endif
            return _colorSchemeCfg;
        }
    }

    [SerializeField] Image _image;
    [SerializeField] TMP_Text _text;

    [SerializeField] ColorShemeType _colorShemeType = ColorShemeType.Default;


    private void OnEnable() => ColorSchemeCfg.OnColorChanged += ApplyColor;
    private void OnDisable() => ColorSchemeCfg.OnColorChanged -= ApplyColor;

    private void Awake() => GetGhaphicsComponent();
    private void Start() => ApplyColor();

    private void Reset()
    {
        GetGhaphicsComponent();
        ApplyColor();
    }
    private void OnValidate() => ApplyColor();

    private void ApplyColor()
    {
        Color color = ColorSchemeCfg.GetColorByType(_colorShemeType);

        if (_image != null)
        {
            _image.color = color;
            return;
        }

        if (_text != null)
        {
            _text.color = color;
            Debug.Log($"Text color changed to: {color}", this);
            return;
        }
    }
    private void GetGhaphicsComponent()
    {
        if (!_image && TryGetComponent(out _image))
            return;

        if (!_text && TryGetComponent(out _text))
        {
            _colorShemeType = ColorShemeType.Text;
            return;
        }
    }
}
