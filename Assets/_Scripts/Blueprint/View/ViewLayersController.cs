using TMPro;
using UnityEngine;
using Zenject;

public class ViewLayersController : MonoBehaviour
{
    [Inject] BlueprintVisualConfig _blueprintVisualConfig;
    [SerializeField] TMP_Dropdown _dropdown;

    private void OnEnable()
    {

    }
    private void OnDisable()
    {

    }

    private void ChangeLayers()
    {
        BlueprintViewLayers layers = new();
        _blueprintVisualConfig.SetViewLayers(layers);
    }
}
