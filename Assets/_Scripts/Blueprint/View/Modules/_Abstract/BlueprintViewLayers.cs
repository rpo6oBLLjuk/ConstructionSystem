[System.Flags]
public enum BlueprintViewLayers //Согласно правилам именования Microsoft, enum с флагами именуются во множественном числе: https://learn.microsoft.com/en-us/dotnet/standard/design-guidelines/names-of-classes-structs-and-interfaces#naming-enumerations
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
    //Проверка эквивалентности флага
    public static bool IsEqualLayer(this BlueprintViewLayers layers, BlueprintViewLayers compareLayers, BlueprintViewLayers layer) => layers.HasLayer(layer) == compareLayers.HasLayer(layer);

    //Проверка наличия флага
    public static bool HasLayer(this BlueprintViewLayers layers, BlueprintViewLayers layer) => (layers & layer) == layer;

    //Включение слоя
    public static BlueprintViewLayers EnableLayer(this BlueprintViewLayers layers, BlueprintViewLayers layer) => layers | layer;

    //Отключение слоя
    public static BlueprintViewLayers DisableLayer(this BlueprintViewLayers layers, BlueprintViewLayers layer) => layers & ~layer;

    //Изменение значения слоя
    public static BlueprintViewLayers SetLayer(this BlueprintViewLayers layers, BlueprintViewLayers layer, bool isActive) => isActive ? layers.EnableLayer(layer) : layers.DisableLayer(layer);

    //Переключение слоя
    public static BlueprintViewLayers ToggleLayer(this BlueprintViewLayers layers, BlueprintViewLayers layer) => layers ^ layer;

}
