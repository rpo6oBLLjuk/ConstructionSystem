using TMPro;
using UnityEngine;
using Zenject;

public class ViewLayersController : MonoBehaviour
{
    [Inject] BlueprintVisualConfig _blueprintVisualConfig;
    [SerializeField] TMP_Dropdown _dropdown;


    private void OnEnable() => _dropdown.onValueChanged.AddListener(ChangeLayers);
    private void OnDisable() => _dropdown.onValueChanged.RemoveListener(ChangeLayers);

    private void Awake() => ApplyLayersToDropdown(_blueprintVisualConfig.BlueprintViewLayers); //Данный вызов активирует включение элементов в Dropdown без "тихого режима", что вызывает ChangeLayers, который вызывает начальную инициализацию

    private void ChangeLayers(int _)
    {
        int mask = _dropdown.value;
        BlueprintViewLayers layers;

        if (mask == 0)
            layers = BlueprintViewLayers.Nothing;
        else
        {
            // Сдвиг на 1, потому что в enum bit0 занят под Everything
            layers = (BlueprintViewLayers)(mask << 1);

            int allMask = (1 << _dropdown.options.Count) - 1;
            if (mask == allMask)
                layers |= BlueprintViewLayers.Everything;
        }

        _blueprintVisualConfig.SetViewLayers(layers);
    }
    private void ApplyLayersToDropdown(BlueprintViewLayers layers)
    {
        int mask = 0;

        if (layers == BlueprintViewLayers.Nothing)
            mask = 0;
        else if (layers == BlueprintViewLayers.Everything)
        {
            mask = (1 << (_dropdown.options.Count - 2)) - 1; // -2 потому что Nothing и Everything не входят в список слоев
        }
        else
        {
            // Проходим по всем слоям (начиная со сдвига 1, т.к. Everything уже использован)
            for (int i = 1; i <= 6; i++) // Points(2) до RoomNames(64)
            {
                BlueprintViewLayers layer = (BlueprintViewLayers)(1 << i);
                if (layers.HasLayer(layer))
                {
                    // Индекс в dropdown: i-1 (потому что Points это i=1 -> индекс 0 в dropdown)
                    mask |= 1 << (i - 1);
                }
            }
        }

        DebugWrapper.InactiveLog(this, $"Layers: {layers}, Mask: {mask}");

        _dropdown.SetValueWithoutNotify(mask);
        _dropdown.RefreshShownValue();

    }
}
