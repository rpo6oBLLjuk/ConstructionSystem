using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class ReziseTool : MonoBehaviour
{
    [Inject] BlueprintManager _blueprintManager;
    [Inject] BlueprintVisualConfig _visualConfig;

    [SerializeField] ButtonSpriteSwapper buttonSpriteSwapper;


    private void OnEnable()
    {
        _blueprintManager.OnBlueprintScaleFactorChanged += BlueprintScaleFactorChanged;
        buttonSpriteSwapper.Button.onClick.AddListener(ResizeButtonClick);
    }
    private void OnDisable()
    {
        _blueprintManager.OnBlueprintScaleFactorChanged -= BlueprintScaleFactorChanged;
        buttonSpriteSwapper.Button.onClick.RemoveListener(ResizeButtonClick);
    }

    private void ResizeButtonClick() => _blueprintManager.SetBlueprintScaleFactor(_blueprintManager.ScaleFactor != 1 ? 1 : Mathf.FloorToInt((_visualConfig.BlueprintScaleFactorMinMax.x + _visualConfig.BlueprintScaleFactorMinMax.y) / 2));
    private void BlueprintScaleFactorChanged(float value, float _) => buttonSpriteSwapper.SetActive(value == 1);
}
