using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class BlueprintLinesController
{
    BlueprintManager _blueprintManager;

    public List<BlueprintLineHandler> Lines { get; private set; } = new();

    [SerializeField] BlueprintLineHandler _defaultLine;

    [SerializeField] float _height = 5f;
    [SerializeField] bool _looped = true;

    [SerializeField] Color _correctLineColor = Color.green;
    [SerializeField] Color _intersectionLineColor = Color.red; //!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!! Вынести в конфиг

    //public void OnEnable()
    //{
    //    foreach (var line in _lines)
    //        line.PointerClicked += OnPointerClicked;
    //}
    public void OnDisable()
    {
        foreach (var line in Lines)
            line.PointerClicked -= OnPointerClicked;
    }

    public void Awake(BlueprintManager blueprintManager)
    {
        _blueprintManager = blueprintManager;
        _defaultLine.gameObject.SetActive(false);
    }
    public void Update()
    {
        for (int i = 0; i < _blueprintManager.BlueprintPoints.Count - 1; i++)
            ConfigurateLine(Lines[i].SelfImage, _blueprintManager.BlueprintPoints[i], _blueprintManager.BlueprintPoints[i + 1]);
        if (_looped & _blueprintManager.BlueprintPoints.Count > 2)
            ConfigurateLine(Lines[^1].SelfImage, _blueprintManager.BlueprintPoints[^1], _blueprintManager.BlueprintPoints[0]);

        CheckIntersections();
    }

    public void AddLine(int index)
    {
        GameObject lineInstance = GameObject.Instantiate(_defaultLine.gameObject, _defaultLine.transform.parent);
        lineInstance.SetActive(true);

        BlueprintLineHandler blh = lineInstance.GetComponent<BlueprintLineHandler>();
        blh.PointerClicked += OnPointerClicked;

        Lines.Insert(index, blh);
    }
    public void RemoveLine(int index)
    {
        GameObject.Destroy(Lines[index].gameObject);
        Lines.RemoveAt(index);
    }

    private void CheckIntersections()
    {
        for (int i = 0; i < _blueprintManager.BlueprintPoints.Count; i++)
        {
            int nextI = (i + 1) % _blueprintManager.BlueprintPoints.Count;
            Vector2 start1 = _blueprintManager.BlueprintPoints[i];
            Vector2 start2 = _blueprintManager.BlueprintPoints[nextI];

            bool notIntersecting = true;

            for (int j = 0; j < _blueprintManager.BlueprintPoints.Count; j++)
            {
                int nextJ = (j + 1) % _blueprintManager.BlueprintPoints.Count;

                if (i == j || nextI == j || i == nextJ ||
                    (i == 0 && j == _blueprintManager.BlueprintPoints.Count - 1) ||
                    (j == 0 && i == _blueprintManager.BlueprintPoints.Count - 1))
                    continue;

                Vector2 end1 = _blueprintManager.BlueprintPoints[j];
                Vector2 end2 = _blueprintManager.BlueprintPoints[nextJ];

                if (AreSegmentsIntersecting(start1, start2, end1, end2))
                {
                    notIntersecting = false;
                    break;
                }
            }

            if (notIntersecting)
                Lines[i].SelfImage.color = _correctLineColor;
            else
                Lines[i].SelfImage.color = _intersectionLineColor;
        }
    }

    private void ConfigurateLine(Image line, Vector2 startPosition, Vector2 endPosition)
    {
        line.rectTransform.position = (startPosition + endPosition) / 2;

        Vector2 direction = endPosition - startPosition;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        line.rectTransform.rotation = Quaternion.Euler(0, 0, angle);

        line.rectTransform.sizeDelta = new Vector2(direction.magnitude / _blueprintManager.ScaleFactor + _height, _height);
    }
    private void OnPointerClicked(BlueprintLineHandler handler, Vector2 posistion) => _blueprintManager.AddPoint(Lines.IndexOf(handler) + 1, posistion);

    private bool AreSegmentsIntersecting(Vector2 p1, Vector2 p2, Vector2 p3, Vector2 p4)
    {
        // Проверяем взаимное расположение отрезков с помощью векторных произведений
        float d1 = Direction(p3, p4, p1);
        float d2 = Direction(p3, p4, p2);
        float d3 = Direction(p1, p2, p3);
        float d4 = Direction(p1, p2, p4);

        // Отрезки пересекаются если точки находятся по разные стороны друг от друга
        if (((d1 > 0 && d2 < 0) || (d1 < 0 && d2 > 0)) &&
            ((d3 > 0 && d4 < 0) || (d3 < 0 && d4 > 0)))
            return true;

        // Проверка особых случаев (отрезки коллинеарны или endpoints совпадают)
        if (Mathf.Approximately(d1, 0) && OnSegment(p3, p4, p1))
            return true;
        if (Mathf.Approximately(d2, 0) && OnSegment(p3, p4, p2))
            return true;
        if (Mathf.Approximately(d3, 0) && OnSegment(p1, p2, p3))
            return true;
        if (Mathf.Approximately(d4, 0) && OnSegment(p1, p2, p4))
            return true;

        return false;
    }
    // Векторное произведение (определяет ориентацию тройки точек)
    private static float Direction(Vector2 pi, Vector2 pj, Vector2 pk)
    {
        return (pk.x - pi.x) * (pj.y - pi.y) - (pj.x - pi.x) * (pk.y - pi.y);
    }
    // Проверяет, лежит ли точка pk на отрезке pi-pj
    private static bool OnSegment(Vector2 pi, Vector2 pj, Vector2 pk)
    {
        return Mathf.Min(pi.x, pj.x) <= pk.x && pk.x <= Mathf.Max(pi.x, pj.x) &&
               Mathf.Min(pi.y, pj.y) <= pk.y && pk.y <= Mathf.Max(pi.y, pj.y);
    }
}
