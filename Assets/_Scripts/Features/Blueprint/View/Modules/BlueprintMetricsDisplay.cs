using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using Zenject;

public class BlueprintMetricsDisplay : MonoBehaviour
{
    [Inject] BlueprintManager _blueprintManager;
    [Inject] BlueprintVisualConfig _visualConfig;
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

    private void OnPointMoved(int id, Vector2 _, Vector2 __) => CalculateMetrics().Forget();
    private void OnBlueprintDataChanged(List<Vector2> __) => CalculateMetrics().Forget();

    //Vertex count: 4, Lines count: 4; Total Perimeter: 10m, Total Area: 33.6 m2
    private async UniTask CalculateMetrics()
    {
        await UniTask.WaitForEndOfFrame();
        
        List<Vector2> points = _blueprintManager.BlueprintPoints;
        _metricsText.text = $"Points: {points.Count}, Perimeter: {CalculatePerimeter(points):F3}, Area: {CalculateArea(points):F3}";
    }

    private float CalculatePerimeter(List<Vector2> points)
    {
        float length = 0;
        Vector2 previousPoint = points[^1];

        points.ForEach(p =>
        {
            length += Vector2.Distance(previousPoint, p) / _visualConfig.TextData.TextMetricPerPixel;
            previousPoint = p;
        });

        return length;
    }

    //Метод Гаусса
    private float CalculateArea(List<Vector2> points)
    {
        if (points.Count < 3)
            return 0;

        float area = 0;
        for (int i = 0; i < points.Count; i++)
        {
            Vector2 current = points[i];
            Vector2 next = points[(i + 1) % points.Count];
            area += (current.x * next.y - next.x * current.y);
        }

        area = Mathf.Abs(area) / 2f;
        
        float pixelToMeter = _visualConfig.TextData.TextMetricPerPixel; // Пиксельные единицы в квадратные метры
        return area / (pixelToMeter * pixelToMeter);
    }

}
