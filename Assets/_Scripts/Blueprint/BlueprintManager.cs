using System.Collections.Generic;
using UnityEngine;

public class BlueprintManager : MonoBehaviour
{
    public List<Vector2> BlueprintPoints { get; private set; } = new();

    public BlueprintLinesController LinesController;
    public BlueprintPointsConstroller PointsController;

    public float ScaleFactor => _canvas.scaleFactor;

    [SerializeField] Canvas _canvas;

    //void OnEnable()
    //{
    //    LinesController.OnEnable();
    //    PointsController.OnEnable();
    //}
    private void OnDisable()
    {
        LinesController.OnDisable();
        PointsController.OnDisable();
    }

    private void Awake()
    {
        _canvas ??= transform.root.GetComponent<Canvas>();

        LinesController.Awake(this);
        PointsController.Awake(this);

        AddPoint(0, transform.position);
        AddPoint(1, transform.position + Vector3.up * 100);
        AddPoint(2, transform.position + Vector3.up * 100 + Vector3.right * 100);
        AddPoint(3, transform.position + Vector3.right * 100);
    }

    void Update()
    {
        PointsController.Update();

        for (int i = 0; i < BlueprintPoints.Count; i++)
            BlueprintPoints[i] = PointsController.Points[i].transform.position;

        LinesController.Update();
    }

    public void AddPoint(int index, Vector2 position)
    {
        BlueprintPoints.Insert(index, position);

        PointsController.AddPoint(index, position);
        LinesController.AddLine(index);

        Debug.Log($"Point added by position: {position}");
    }
}
