using System;
using UnityEngine;

[CreateAssetMenu(fileName = "BlueprintVisualConfig", menuName = "Scriptable Objects/Blueprint/Config")]
public class BlueprintVisualConfig : ScriptableObject
{
    public Action<BlueprintViewLayers> ViewLayersChanged;

    [field: Header("<color=yellow>RUNTIME</color>")]
    [field: SerializeField] public BlueprintViewLayers BlueprintViewLayers { get; private set; }

    [field: Header("<color=yellow>CANVAS</color>")]
    [field: SerializeField] public float DefaultBlueprintScaleFactor { get; private set; } = 2f;
    [field: SerializeField] public Vector2 BlueprintScaleFactorMinMax { get; private set; } = new Vector2(1, 10);
    [field: SerializeField] public float DefaultBordersSize { get; private set; } = 3f;

    [field: Header("<color=yellow>POINTS</color>")]
    [field: SerializeField] public PointsVisualConfig PointsData { get; private set; } = new();

    [field: Header("<color=yellow>LINES</color>")]
    [field: SerializeField] public LinesVisualConfig LinesData { get; private set; } = new();

    [field: Header("<color=yellow>HISTORY</color>")]
    [field: SerializeField] public HistoryVisualConfig HistoryData { get; private set; } = new();

    [field: Header("<color=yellow>Text</color>")]
    [field: SerializeField] public TextVisualConfig TextData { get; private set; } = new();

    public void SetViewLayers(BlueprintViewLayers newLayers)
    {
        if (BlueprintViewLayers == newLayers)
            return;

        BlueprintViewLayers = newLayers;
        ViewLayersChanged?.Invoke(newLayers);
    }
}

[Serializable]
public class PointsVisualConfig : VisualConfigBase
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
    //[field: SerializeField] public bool IsVisible { get; set; } = true; //Ctrl+F: SetPointsVisible. метод меняет данное значение. А не должен
}

[Serializable]
public class LinesVisualConfig : VisualConfigBase
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

[Serializable]
public class TextVisualConfig : VisualConfigBase
{
    [field: Header("<u>Font</u>")]
    [field: SerializeField] public float FontSize { get; private set; } = 15f;
    [Tooltip("Кривая, получающая на вход размер ScaleFactor, и возвращающая максимальный width линии")]
    [field: SerializeField] public AnimationCurve ClippingCurve { get; private set; }

    [field: Header("<u>Animation</u>")]
    [field: SerializeField, Space] public float TextFadeDuration { get; private set; } = 0.25f;
}

public abstract class VisualConfigBase
{
    //public event Action<bool> VisibilityChanged;

    //public bool IsVisible
    //{
    //    get => _isVisible;
    //    set
    //    {
    //        if (_isVisible != value)
    //            VisibilityChanged?.Invoke(_isVisible);

    //        _isVisible = value;
    //    }
    //}
    //[Header("<u>VisualConfig</u>")]
    //[SerializeField] private bool _isVisible = true;

    /*
    Нужно переписать логику. Либо открыть свойства (bad move), либо как-то усложнить количество кода, дабы получать данные об обновлении. Возможно стоит создавать новые экземпляры VisualConfigBase-наследников, однако
    тогда надо атомаризировать данные и классы с ивентами (новый экземпляр удалит подписки на ивенты). А возможно, стоит просто сделать глобальный ивент изменения данных, т.к. многие классы берут данные сразу из конфига.
    Но, такие как HistoryView - создают твин в начале.

    public event Action ConfigChanged;
    public void ApplyChanges() => ConfigChanged?.Invoke();
    */
}