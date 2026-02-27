using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class BlueprintLinesController : BlueprintView<CanvasGroup>
{
    protected override IEnumerable<CanvasGroup> ViewList => Lines.Select(BlueprintLineHandler => BlueprintLineHandler.CanvasGroup);
    protected override BlueprintViewLayers ViewLayer => BlueprintViewLayers.Lines;

    public List<LineData> _Lines = new();
    public List<BlueprintLineHandler> Lines { get; private set; } = new();

    [SerializeField] BlueprintLineHandler _defaultLine;

    //public void OnEnable()
    //{
    //    foreach (var line in _lines)
    //        line.PointerClicked += OnPointerClicked;
    //}
    protected override void OnDisable()
    {
        base.OnDisable();
        foreach (var line in Lines)
            line.PointerClicked -= OnPointerClicked;
    }

    public void Awake() => _defaultLine.gameObject.SetActive(false);
    public void Update()
    {
        SetLinesPosition();
        CheckIntersections();
    }

    public void AddLine(int index)
    {
        _Lines.Insert(index, new LineData());

        GameObject lineInstance = GameObject.Instantiate(_defaultLine.gameObject, _defaultLine.transform.parent);
        lineInstance.SetActive(true);

        BlueprintLineHandler blh = lineInstance.GetComponent<BlueprintLineHandler>();
        SetCanvasGroupVisible(blh.CanvasGroup);
        SetGraphicVisible(blh.Text, blh.Text.color);

        blh.PointerClicked += OnPointerClicked;

        Lines.Insert(index, blh);

        SetLinesPosition();
    }
    public void RemoveLine(int index)
    {
        GameObject.Destroy(Lines[index].gameObject);
        _Lines.RemoveAt(index);
        Lines.RemoveAt(index);
    }

    private void SetLinesPosition()
    {
        for (int i = 0; i < _blueprintManager.PointsController.Points.Count - 1; i++)
            ConfigurateLine(Lines[i].SelfImage, _blueprintManager.PointsController.Points[i].SelfImage.rectTransform.anchoredPosition, _blueprintManager.PointsController.Points[i + 1].SelfImage.rectTransform.anchoredPosition);
        if (_blueprintVisualConfig.LinesData.Looped & _blueprintManager.PointsController.Points.Count > 2)
            ConfigurateLine(Lines[^1].SelfImage, _blueprintManager.PointsController.Points[^1].SelfImage.rectTransform.anchoredPosition, _blueprintManager.PointsController.Points[0].SelfImage.rectTransform.anchoredPosition);
    }
    private void CheckIntersections()
    {
        for (int i = 0; i < _blueprintManager.PointsController.Points.Count; i++)
        {
            int nextI = (i + 1) % _blueprintManager.PointsController.Points.Count;
            Vector2 start1 = _blueprintManager.PointsController.Points[i].SelfImage.rectTransform.anchoredPosition;
            Vector2 start2 = _blueprintManager.PointsController.Points[nextI].SelfImage.rectTransform.anchoredPosition;

            bool notIntersecting = true;

            for (int j = 0; j < _blueprintManager.PointsController.Points.Count; j++)
            {
                int nextJ = (j + 1) % _blueprintManager.PointsController.Points.Count;

                if (i == j || nextI == j || i == nextJ ||
                    (i == 0 && j == _blueprintManager.PointsController.Points.Count - 1) ||
                    (j == 0 && i == _blueprintManager.PointsController.Points.Count - 1))
                    continue;

                Vector2 end1 = _blueprintManager.PointsController.Points[j].SelfImage.rectTransform.anchoredPosition;
                Vector2 end2 = _blueprintManager.PointsController.Points[nextJ].SelfImage.rectTransform.anchoredPosition;

                if (AreSegmentsIntersecting(start1, start2, end1, end2))
                {
                    notIntersecting = false;
                    break;
                }
            }

            _Lines[i].Intersecting = !notIntersecting;
            if (notIntersecting)
                Lines[i].SelfImage.color = _blueprintVisualConfig.LinesData.CorrectLineColor;
            else
                Lines[i].SelfImage.color = _blueprintVisualConfig.LinesData.IntersectionLineColor;
        }
    }

    private void ConfigurateLine(Image line, Vector2 startPosition, Vector2 endPosition)
    {
        line.rectTransform.anchoredPosition = (startPosition + endPosition) / 2;

        Vector2 direction = endPosition - startPosition;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        line.rectTransform.rotation = Quaternion.Euler(0, 0, angle);

        float height = _blueprintVisualConfig.LinesData.Height / _blueprintManager.ScaleFactor;
        line.rectTransform.sizeDelta = new Vector2(direction.magnitude + height, height);
        line.raycastPadding = new Vector4(0, -_blueprintVisualConfig.LinesData.Padding / _blueprintManager.ScaleFactor, 0, -_blueprintVisualConfig.LinesData.Padding / _blueprintManager.ScaleFactor);
    }
    private void OnPointerClicked(BlueprintLineHandler handler, Vector2 posistion) => _blueprintManager.AddPoint(Lines.IndexOf(handler) + 1, GetPositionForCenterAnchor(_defaultLine.transform as RectTransform, posistion));

    private Vector2 GetPositionForCenterAnchor(RectTransform rectTransform, Vector3 screenPosition)
    {
        Vector2 localPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            rectTransform.parent as RectTransform,
            screenPosition,
            null,
            out localPoint
        );

        return localPoint;
    }

    private bool AreSegmentsIntersecting(Vector2 p1, Vector2 p2, Vector2 p3, Vector2 p4)
    {
        //ѕровер€ем взаимное расположение отрезков с помощью векторных произведений
        float d1 = Direction(p3, p4, p1);
        float d2 = Direction(p3, p4, p2);
        float d3 = Direction(p1, p2, p3);
        float d4 = Direction(p1, p2, p4);

        //ќтрезки пересекаютс€ если точки наход€тс€ по разные стороны друг от друга
        if (((d1 > 0 && d2 < 0) || (d1 < 0 && d2 > 0)) &&
            ((d3 > 0 && d4 < 0) || (d3 < 0 && d4 > 0)))
            return true;

        //ѕроверка особых случаев (отрезки коллинеарны или endpoints совпадают)
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
    //¬екторное произведение (определ€ет ориентацию тройки точек)
    private static float Direction(Vector2 pi, Vector2 pj, Vector2 pk)
    {
        return (pk.x - pi.x) * (pj.y - pi.y) - (pj.x - pi.x) * (pk.y - pi.y);
    }
    //ѕровер€ет, лежит ли точка pk на отрезке pi-pj
    private static bool OnSegment(Vector2 pi, Vector2 pj, Vector2 pk)
    {
        return Mathf.Min(pi.x, pj.x) <= pk.x && pk.x <= Mathf.Max(pi.x, pj.x) &&
               Mathf.Min(pi.y, pj.y) <= pk.y && pk.y <= Mathf.Max(pi.y, pj.y);
    }
}

public class LineData
{
    public Vector2 StartPoint;
    public Vector2 EndPoint;

    public bool Intersecting = false;
}