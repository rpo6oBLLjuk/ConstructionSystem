using DG.Tweening;
using System;
using System.Collections.Generic;
using UnityEngine;

public class BlueprintManager : MonoBehaviour
{
    public List<Vector2> BlueprintPoints { get; private set; } = new();

    [field: SerializeField] public BlueprintLinesController LinesController { get; private set; }
    [field: SerializeField] public BlueprintPointsConstroller PointsController { get; private set; }
    [field: SerializeField] public BlueprintHistoryController HistoryController { get; private set; } //!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!! Жёсткая зависимость и лишний код, need execution

    public float ScaleFactor => _canvas.scaleFactor;

    [SerializeField] Canvas _canvas;
    [SerializeField] List<Vector2> _defaultPoints;
    [SerializeField] float pointMoveDuration = 0.25f; //Нужно вынести в конфиг

    // События
    public Action<int, Vector2> OnPointAdded;
    public Action<int, Vector2> OnPointRemoved;
    public Action<int, Vector2, Vector2> OnPointMoved;

    public Action<List<Vector2>> OnBlueprintDataChanging;
    public Action<List<Vector2>> OnBlueprintDataChanged;


    private void OnEnable()
    {
        HistoryController.OnEnable();
    }

    void OnDisable()
    {
        LinesController.OnDisable();
        PointsController.OnDisable();
        HistoryController.OnDisable();
    }

    void Awake()
    {
        _canvas ??= transform.root.GetComponent<Canvas>();

        for (int i = 0; i < _defaultPoints.Count; i++)
            _defaultPoints[i] += new Vector2(transform.position.x, transform.position.y);

        LinesController.Awake(this);
        PointsController.Awake(this);
        HistoryController.Awake(this);

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
        PointsController.Points[index].transform.DOMove(newPosition, pointMoveDuration);
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

        PointsController.RemovePoint(index);
        LinesController.RemoveLine(index);
    }

    public void ResetBlueprint() => SetBlueprintData(_defaultPoints);

    public void SetBlueprintData(List<Vector2> points)
    {
        OnBlueprintDataChanging?.Invoke(BlueprintPoints);

        for (int i = BlueprintPoints.Count - 1; i >= 0; i--)
            RemovePoint(i, true);

        for (int i = 0; i < points.Count; i++)
            AddPoint(i, points[i]);

        OnBlueprintDataChanged?.Invoke(points);
    }
}