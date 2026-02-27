using DG.Tweening;
using System;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class BlueprintManager : MonoBehaviour
{
    public List<Vector2> BlueprintPoints { get; private set; } = new();

    [field: SerializeField] public BlueprintLinesController LinesController { get; private set; }
    [field: SerializeField] public BlueprintPointsConstroller PointsController { get; private set; }

    public float CanvasScaleFactor => Canvas.scaleFactor;
    public float ScaleFactor { get; private set; }

    public Canvas Canvas;
    [SerializeField] List<Vector2> _defaultPoints;
    // События
    public Action<int, Vector2> OnPointAdded;
    public Action<int, Vector2> OnPointRemoved;
    public Action<int, Vector2, Vector2> OnPointMoved;

    public Action<List<Vector2>> OnBlueprintDataChanging;
    public Action<List<Vector2>> OnBlueprintDataChanged;

    /// <summary>
    /// First arg: scale, second arg: tween duration
    /// </summary>
    public event Action<float, float> OnBlueprintScaleFactorChanged;

    [Inject] BlueprintVisualConfig _visualConfig;
    private float _targetScaleFactor;


    
    void Awake()
    {
        Canvas ??= transform.root.GetComponent<Canvas>();
        Debug.Log("Blueprint PointsEnabled");
    }

    private void Start()
    {
        ScaleFactor = (transform.localScale.x + transform.localScale.y) / 2;
        SetBlueprintScaleFactor(_visualConfig.DefaultBlueprintScaleFactor);

        ResetBlueprint();
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
        PointsController.MovePoint(index, newPosition);
    }

    public void AddPoint(int index, Vector2 position)
    {
        BlueprintPoints.Insert(index, position);

        PointsController.AddPoint(index, position);
        LinesController.AddLine(index);

        OnPointAdded?.Invoke(index, position);

        Debug.Log($"Point added by position: {position}");
    }
    public void RemovePoint(int index, bool forceDelete = false)
    {
        if (!forceDelete)
            if (BlueprintPoints.Count <= 3)
            {
                DebugWrapper.LogWarning(this, "Need info 'Чертёж не может содержать меньше 3х точек'");
                return;
            }

        OnPointRemoved?.Invoke(index, BlueprintPoints[index]);

        BlueprintPoints.RemoveAt(index);

        LinesController.RemoveLine(index); //LineController обновляется первым, чтобы запомнить точку, к которой будет привязана линия (таким образом обоих можно вывести из ротации индексов)
        PointsController.RemovePoint(index);
    }

    public void SetBlueprintScaleFactor(float newScaleFactor)
    {
        newScaleFactor = Mathf.Clamp(newScaleFactor, _visualConfig.BlueprintScaleFactorMinMax.x, _visualConfig.BlueprintScaleFactorMinMax.y);
        newScaleFactor = Mathf.Floor(newScaleFactor * 10) / 10;

        _targetScaleFactor = newScaleFactor;

        //BlueprintScaleFactor = newScaleFactor;

        float duration = 0.1f;

       DOVirtual.Float(ScaleFactor, newScaleFactor, duration, value => {
                ScaleFactor = value;
                transform.localScale = Vector3.one * ScaleFactor;
            });

        
        OnBlueprintScaleFactorChanged?.Invoke(newScaleFactor, duration);
    }
    public float AddBlueprintScaleFactor(float addedScaleFactor)
    {
        float result = _targetScaleFactor + addedScaleFactor;
        SetBlueprintScaleFactor(_targetScaleFactor + addedScaleFactor);

        return result;
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