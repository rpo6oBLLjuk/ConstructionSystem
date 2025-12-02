using DG.Tweening;
using System;
using System.Collections.Generic;
using UnityEngine;

public class BlueprintManager : MonoBehaviour
{
    public List<Vector2> BlueprintPoints { get; private set; } = new();

    [field: SerializeField] public BlueprintLinesController LinesController { get; private set; }
    [field: SerializeField] public BlueprintPointsConstroller PointsController { get; private set; }
    [field: SerializeField] public BlueprintHistoryController HistoryController { get; private set; } //!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!! ∆Єстка€ зависимость и лишний код, need execution

    public float CanvasScaleFactor => Canvas.scaleFactor;
    public float BlueprintScaleFactor { get; private set; }

    public Canvas Canvas;
    [SerializeField] List<Vector2> _defaultPoints;
    [SerializeField] float _pointMoveDuration = 0.25f; //Ќужно вынести в конфиг
    [SerializeField] float _pointDestroyDuration = 0.1f;

    [SerializeField] float _defaultBlueprintScaleFactor = 2f; // Config!!!!!!!!!!!
    [SerializeField] Vector2 _blueprintScaleFactorMinMax = new Vector2(1, 10); 

    // —обыти€
    public Action<int, Vector2> OnPointAdded;
    public Action<int, Vector2> OnPointRemoved;
    public Action<int, Vector2, Vector2> OnPointMoved;

    public Action<List<Vector2>> OnBlueprintDataChanging;
    public Action<List<Vector2>> OnBlueprintDataChanged;

    /// <summary>
    /// First arg: scale, second arg: tween duration
    /// </summary>
    public event Action<float, float> OnBlueprintScaleFactorChanged;


    void OnDisable()
    {
        LinesController.OnDisable();
        PointsController.OnDisable();
        HistoryController.OnDisable();
    }

    void Awake()
    {
        Canvas ??= transform.root.GetComponent<Canvas>();

        LinesController.Awake(this);
        PointsController.Awake(this);

        HistoryController.Awake(this);

        Debug.Log("Blueprint PointsEnabled");
    }

    private void Start()
    {
        BlueprintScaleFactor = (transform.localScale.x + transform.localScale.y) / 2;
        SetBlueprintScaleFactor(_defaultBlueprintScaleFactor);

        ResetBlueprint();
        HistoryController.AddListeners();
    }

    void Update()
    {
        PointsController.Update();
        LinesController.Update();
    }

    public void MovePoint(int index, Vector2 newPosition)
    {
        OnPointMoved?.Invoke(index, BlueprintPoints[index], newPosition);

        BlueprintPoints[index] = newPosition;
        PointsController.Points[index].SelfImage.rectTransform.DOAnchorPos(newPosition, _pointMoveDuration);
    }

    public void AddPoint(int index, Vector2 position)
    {
        OnPointAdded?.Invoke(index, position);

        BlueprintPoints.Insert(index, position);

        PointsController.AddPoint(index, position);
        LinesController.AddLine(index);

        Debug.Log($"Point added by position: {position}");
    }
    public void RemovePoint(int index, bool forceDelete = false)
    {
        if (!forceDelete)
            if (BlueprintPoints.Count <= 3)
                return;

        OnPointRemoved?.Invoke(index, BlueprintPoints[index]);

        BlueprintPoints.RemoveAt(index);

        LinesController.RemoveLine(index, _pointDestroyDuration); //LineController обновл€етс€ первым, чтобы запомнить точку, к которой будет прив€зана лини€ (таким образом обоих можно вывести из ротации индексов)
        PointsController.RemovePoint(index, _pointDestroyDuration);
    }

    public void SetBlueprintScaleFactor(float newScaleFactor)
    {
        newScaleFactor = Mathf.Clamp(newScaleFactor, _blueprintScaleFactorMinMax.x, _blueprintScaleFactorMinMax.y);
        newScaleFactor = Mathf.Floor(newScaleFactor * 10) / 10;

        //BlueprintScaleFactor = newScaleFactor;

        float duration = 0.1f;

        DOVirtual.Float(BlueprintScaleFactor, newScaleFactor, duration, value => {
            BlueprintScaleFactor = value;
            transform.localScale = Vector3.one * BlueprintScaleFactor;
        });
        OnBlueprintScaleFactorChanged?.Invoke(newScaleFactor, duration);
    }

    public void ResetBlueprint() => SetBlueprintData(_defaultPoints);
    public void SetBlueprintData(List<Vector2> points)
    {
        OnBlueprintDataChanging?.Invoke(BlueprintPoints);

        for (int i = BlueprintPoints.Count - 1; i >= 0; i--)
            RemovePoint(i, true);

        for (int i = 0; i < points.Count; i++)
        {
            AddPoint(i, Vector2.zero);
            MovePoint(i, points[i]);
        }

        OnBlueprintDataChanged?.Invoke(points);
    }
}