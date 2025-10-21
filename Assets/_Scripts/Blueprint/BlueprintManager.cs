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

    //void OnEnable()
    //{
    //    LinesController.OnEnable();
    //    PointsController.OnEnable();
    //}
    void OnDisable()
    {
        LinesController.OnDisable();
        PointsController.OnDisable();
    }

    void Awake()
    {
        _canvas ??= transform.root.GetComponent<Canvas>();

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
        HistoryController.MovePointAction(index, BlueprintPoints[index], newPosition); //!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!

        BlueprintPoints[index] = newPosition;
        PointsController.Points[index].transform.position = newPosition;
    }

    public void AddPoint(int index, Vector2 position)
    {
        HistoryController.AddPointAction(index, position); //!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!

        BlueprintPoints.Insert(index, position);

        PointsController.AddPoint(index, position);
        LinesController.AddLine(index);

        Debug.Log($"Point added by position: {position}");
    }
    public void RemovePoint(int index, bool forceDelete = false)
    {
        if (!forceDelete && BlueprintPoints.Count <= 3)
            return;

        HistoryController.RemovePointAction(index, BlueprintPoints[index]); //!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!

        BlueprintPoints.RemoveAt(index);

        PointsController.RemovePoint(index);
        LinesController.RemoveLine(index);
    }

    public void ResetBlueprint()
    {
        HistoryController.ResetBlueprintAction(BlueprintPoints); //!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!

        SetBlueprintData(_defaultPoints);
    }

    public void SetBlueprintData(List<Vector2> points)
    {
        for (int i = 0; i < BlueprintPoints.Count; i++)
            RemovePoint(i, true);

        for (int i = 0; i < points.Count; i++)
            AddPoint(i, new Vector2(transform.position.x, transform.position.y) + points[i]);
    }
}
