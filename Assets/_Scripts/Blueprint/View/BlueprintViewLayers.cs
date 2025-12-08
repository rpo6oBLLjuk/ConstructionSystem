[System.Flags]
public enum BlueprintViewLayers
{
    Nothing = 0,
    Everything = 1 << 0,      // 1
    Points = 1 << 1,          // 2
    Lines = 1 << 2,           // 4
    Angles = 1 << 3,          // 8
    Text = 1 << 4,            // 16
    Grid = 1 << 5,            // 32
    RoomNames = 1 << 6        // 64
}

public static class BlueprintViewLayersHelper
{
    // Проверка наличия флага
    public static bool HasLayer(this BlueprintViewLayers layers, BlueprintViewLayers layer) => (layers & layer) == layer;

    // Добавление слоя
    public static BlueprintViewLayers EnableLayer(this BlueprintViewLayers layers, BlueprintViewLayers layer) => layers | layer;

    // Удаление слоя
    public static BlueprintViewLayers DisableLayer(this BlueprintViewLayers layers, BlueprintViewLayers layer) => layers & ~layer;

    // Переключение слоя
    public static BlueprintViewLayers ToggleLayer(this BlueprintViewLayers layers, BlueprintViewLayers layer) => layers ^ layer;
}
