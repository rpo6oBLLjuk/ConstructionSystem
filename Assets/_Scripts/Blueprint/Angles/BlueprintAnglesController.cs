using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BlueprintAnglesController : BlueprintView
{
    [SerializeField] GameObject _defaultAnglePrefab;

    List<BlueprintPointHandler> _points => _blueprintManager.PointsController.Points;
    List<BlueprintLineHandler> _lines => _blueprintManager.LinesController.Lines;
    List<AngleInstance> _anglesInstances = new();

    private float _defaultAnglePrefabSize;


    protected override void OnEnable()
    {
        base.OnEnable();
        _blueprintManager.OnPointAdded += AddAngle;
        _blueprintManager.OnPointRemoved += RemovePoint;
    }
    private void OnDisable()
    {
        _blueprintManager.OnPointAdded -= AddAngle;
        _blueprintManager.OnPointRemoved -= RemovePoint;
    }

    public void Start()
    {
        _defaultAnglePrefab.SetActive(false);
        _defaultAnglePrefabSize = (_defaultAnglePrefab.transform as RectTransform).sizeDelta.x;
    }
    public void Update()
    {
        for (int i = 0; i < _anglesInstances.Count; i++)
            UpdateAngleInstance(i, _anglesInstances[i]);
    }

    private void AddAngle(int index, Vector2 _) => InstantiateNewAngle(index);
    private void RemovePoint(int index, Vector2 _) => RemoveAngle(index);

    private void InstantiateNewAngle(int index)
    {
        GameObject instance = GameObject.Instantiate(_defaultAnglePrefab, _defaultAnglePrefab.transform.parent);
        instance.SetActive(true);

        _anglesInstances.Insert(index, new AngleInstance(
            instance.GetComponent<RectTransform>(),
            instance.GetComponent<CanvasGroup>(),
            instance.GetComponentInChildren<Image>(),
            instance.transform.Find("Text_Container") as RectTransform,
            instance.GetComponentInChildren<TMP_Text>(),
            instance.GetComponentInChildren<HorizontalLayoutGroup>())
            );
    }
    private void RemoveAngle(int index)
    {
        Destroy(_anglesInstances[index].Root.gameObject);
        _anglesInstances.RemoveAt(index);
    }

    private void UpdateAngleInstance(int index, AngleInstance instance)
    {
        UpdatePosition(index, instance);
        UpdateRotation(index, instance);
        UpdateScale(index, instance);
        UpdateVisuals(index, instance);
    }

    private void UpdatePosition(int index, AngleInstance instance) => instance.Root.position = _points[index].SelfImage.rectTransform.position;
    private void UpdateRotation(int index, AngleInstance instance)
    {
        int previousIndex = GetPreviousIndex(index);
        int nextIndex = GetNextIndex(index);

        float angle = CalculateAngle(
            _points[previousIndex].SelfImage.rectTransform.anchoredPosition,
            _points[index].SelfImage.rectTransform.anchoredPosition,
            _points[nextIndex].SelfImage.rectTransform.anchoredPosition
        );

        instance.Root.rotation = _lines[previousIndex].transform.rotation;
        instance.TextContainer.localRotation = Quaternion.Euler(0, 0, -angle / 2);
        instance.Text.transform.parent.rotation = Quaternion.identity;
    }

    private void UpdateScale(int index, AngleInstance instance)
    {
        int previousIndex = GetPreviousIndex(index);
        float lineSize = CalculateLineSize(previousIndex, index);

        Vector3 localScale = lineSize > 1 / _blueprintManager.BlueprintScaleFactor
            ? Vector3.one
            : Vector3.one * lineSize * _blueprintManager.BlueprintScaleFactor;

        instance.Root.localScale = localScale / _blueprintManager.BlueprintScaleFactor;
    }

    private void UpdateVisuals(int index, AngleInstance instance)
    {
        int previousIndex = GetPreviousIndex(index);
        int nextIndex = GetNextIndex(index);

        float angle = CalculateAngle(
            _points[previousIndex].SelfImage.rectTransform.anchoredPosition,
            _points[index].SelfImage.rectTransform.anchoredPosition,
            _points[nextIndex].SelfImage.rectTransform.anchoredPosition
        );

        instance.Filler.transform.localScale = Mathf.Sign(angle) == 1 ? Vector3.one : new Vector3(1, -1, 1);
        angle = Mathf.Abs(angle);

        instance.Text.text = $"{angle:F2}°";
        instance.Filler.fillAmount = angle / 360;
    }

    private int GetPreviousIndex(int index) => index == 0 ? _anglesInstances.Count - 1 : index - 1;
    private int GetNextIndex(int index) => index == _anglesInstances.Count - 1 ? 0 : index + 1;

    private float CalculateLineSize(int previousIndex, int currentIndex)
    {
        float previousLineSize = (_lines[previousIndex].SelfImage.rectTransform.sizeDelta.x -
                                _lines[previousIndex].SelfImage.rectTransform.sizeDelta.y) / _defaultAnglePrefabSize;

        float currentLineSize = (_lines[currentIndex].SelfImage.rectTransform.sizeDelta.x -
                               _lines[currentIndex].SelfImage.rectTransform.sizeDelta.y) / _defaultAnglePrefabSize;

        return Mathf.Min(previousLineSize, currentLineSize);
    }
    private float CalculateAngle(Vector3 a, Vector3 b, Vector3 c)
    {
        // Векторы от точки B к A и от точки B к C
        Vector3 ba = a - b;
        Vector3 bc = c - b;

        // Нормализуем векторы (делаем длиной 1)
        ba.Normalize();
        bc.Normalize();

        float baseAngle = Vector3.Angle(ba, bc);

        // Определяем направление с помощью векторного произведения
        Vector3 cross = Vector3.Cross(ba, bc);

        // Если векторное произведение смотрит "вверх" - угол положительный (0-180)
        // Если "вниз" - угол отрицательный (180-360)
        return (cross.z > 0) ? -baseAngle : baseAngle;
    }

    [Serializable]
    private record AngleInstance(RectTransform Root, CanvasGroup CanvasGroup, Image Filler, RectTransform TextContainer, TMP_Text Text, HorizontalLayoutGroup HorizontalLayoutGroup)
    {
        public RectTransform Root { get; private set; } = Root;
        public CanvasGroup CanvasGroup { get; private set; } = CanvasGroup;
        public Image Filler { get; private set; } = Filler;
        public RectTransform TextContainer { get; private set; } = TextContainer;
        public TMP_Text Text { get; private set; } = Text;
        public HorizontalLayoutGroup HorizontalLayoutGroup { get; private set; } = HorizontalLayoutGroup; //TextGameObject
    }
}
