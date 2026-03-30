using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Zenject;

public class BlueprintMetricsDisplay : MonoBehaviour
{
    [Inject] BlueprintManager _blueprintManager;
    [SerializeField] TMP_Text _metricsText;


    private void OnEnable()
    {
        _blueprintManager.OnBlueprintDataChanged += OnBlueprintDataChanged;
        _blueprintManager.OnPointMoved += OnPointMoved;
    }

    private void OnDisable()
    {
        _blueprintManager.OnBlueprintDataChanged -= OnBlueprintDataChanged;
        _blueprintManager.OnPointMoved -= OnPointMoved;
    }

    private void OnPointMoved(int id, Vector2 _, Vector2 __) => CalculateMetrics(_blueprintManager.BlueprintPoints);
    private void OnBlueprintDataChanged(List<Vector2> points) => CalculateMetrics(points);

    //Vertex count: 4, Lines count: 4; Total Perimeter: 10m, Total Square: 33.6 m2
    private void CalculateMetrics(List<Vector2> points)
    {
        int pointsCount = points.Count;

        float length = 0;
        Vector2 previousPoint = points[0];
        points.ForEach(p =>
        {
            length += Vector2.Distance(previousPoint, p);
            previousPoint = p;
        });


        _metricsText.text = $"Points: {pointsCount}, Perimeter: {length:F2}";
    }
}
