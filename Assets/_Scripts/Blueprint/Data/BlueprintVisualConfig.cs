using System;
using UnityEngine;

[CreateAssetMenu(fileName = "BlueprintVisualConfig", menuName = "Scriptable Objects/Blueprint/Config")]
public class BlueprintVisualConfig : ScriptableObject
{
    [field: Header("<color=yellow>RUNTIME</color>")]
    [field: SerializeField] public BlueprintViewLayers BlueprintViewLayers;

    [field: Header("<color=yellow>CANVAS</color>")]
    [field: SerializeField] public float DefaultBlueprintScaleFactor { get; private set; } = 2f;
    [field: SerializeField] public Vector2 BlueprintScaleFactorMinMax { get; private set; } = new Vector2(1, 10);

    [field: Header("<color=yellow>POINTS</color>")]
    [field: SerializeField] public PointsVisualConfig PointsData { get; private set; } = new();

    [field: Header("<color=yellow>LINES</color>")]
    [field: SerializeField] public LinesVisualConfig LinesData { get; private set; } = new();

    [field: Header("<color=yellow>HISTORY</color>")]
    [field: SerializeField] public HistoryVisualConfig HistoryData { get; private set; } = new();
}

[Serializable]
public class PointsVisualConfig
{
    [field: Header("<u>Animation</u>")]
    [field: SerializeField] public float MoveDuration { get; private set; } = 0.25f;
    [field: SerializeField] public float DestroyDuration { get; private set; } = 0.1f;
    [field: SerializeField] public float VisibilityFadeDuration { get; private set; } = 0.25f;

    [field: Header("<u>Snapping</u>")]
    [field: SerializeField] public float SnapDuration { get; private set; } = 0.05f;
    [field: SerializeField] public float SnapDistance { get; private set; } = 10;
    [field: SerializeField] public float SnapSmooth { get; private set; } = 5;
    [field: SerializeField] public float TextureTileMultiplyer { get; private set; } = 1;

    [field: Header("<u>Color</u>")]
    [field: SerializeField] public Color InactivePointColor { get; private set; } = Color.white;
    [field: SerializeField] public Color DragPointColor { get; private set; } = Color.yellow;
    [field: SerializeField] public bool IsVisible { get; set; } = true; //Ctrl+F: SetPointsVisible. метод меняет данное значение. А не должен
}

[Serializable]
public class LinesVisualConfig
{
    [field: Header("<u>Size</u>")]
    [field: SerializeField] public float Height { get; private set; } = 5f;
    [field: SerializeField] public float Padding { get; private set; } = 2f; //!!!!!!!!!!!!НУЖНО ИСПОЛЬЗОВАТЬ В LINECONTROLLER (домножать на Scale, чтобы корректно работало при изменении отбражения линий)
    [field: SerializeField] public bool Looped { get; private set; } = true;

    [field: Header("<u>Color</u>")]
    [field: SerializeField] public Color CorrectLineColor { get; private set; } = Color.green;
    [field: SerializeField] public Color IntersectionLineColor { get; private set; } = Color.red;
}

[Serializable]
public class HistoryVisualConfig
{
    [field: Header("<u>Timings</u>")]
    [field: SerializeField] public float UndoRedoDelay { get; private set; } = 0.25f;
    [field: SerializeField] public float HoldThreshold { get; private set; } = 1f;
}