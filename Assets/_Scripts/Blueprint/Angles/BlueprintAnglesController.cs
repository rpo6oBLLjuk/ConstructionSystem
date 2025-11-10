using DG.Tweening;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BlueprintAnglesController : MonoBehaviour
{
    [SerializeField] BlueprintManager _blueprintManager;

    [SerializeField] List<BlueprintPointHandler> _points => _blueprintManager.PointsController.Points;
    [SerializeField] List<BlueprintLineHandler> _lines => _blueprintManager.LinesController.Lines;
    [SerializeField] GameObject _defaultAnglePrefab;

    [SerializeField] List<AngleInstance> _anglesInstances = new();


    private void OnEnable()
    {
        _blueprintManager.OnPointAdded += AddAngle;
        _blueprintManager.OnPointRemoved += RemovePoint;

        _blueprintManager.OnBlueprintScaleFactorChanged += TweenAnglesInstancesScale;
    }
    private void OnDisable()
    {
        _blueprintManager.OnPointAdded -= AddAngle;
        _blueprintManager.OnPointRemoved -= RemovePoint;

        _blueprintManager.OnBlueprintScaleFactorChanged -= TweenAnglesInstancesScale;
    }

    public void Start() => _defaultAnglePrefab.SetActive(false);
    public void Update() => _anglesInstances.ForEach(instance => UpdateAngleInstance(instance));

    private void AddAngle(int index, Vector2 _) => InstantiateNewAngle(index);
    private void RemovePoint(int index, Vector2 _) => RemoveAngle(index);

    private void InstantiateNewAngle(int index)
    {
        GameObject instance = GameObject.Instantiate(_defaultAnglePrefab, _defaultAnglePrefab.transform.parent);
        instance.SetActive(true);

        _anglesInstances.Insert(index, new AngleInstance(
            index,
            instance.GetComponent<RectTransform>(),
            instance.GetComponent<CanvasGroup>(),
            instance.GetComponentInChildren<Image>(),
            instance.GetComponentInChildren<TMP_Text>())
            );
    }
    private void RemoveAngle(int index)
    {
        GameObject.Destroy(_anglesInstances[index].Root);
        _anglesInstances.RemoveAt(index);
    }

    private void UpdateAngleInstance(AngleInstance instance)
    {
        instance.Root.position = _points[instance.Index].SelfImage.rectTransform.position /** instance.Root.transform.localScale.z*/;

        int previousIndex = instance.Index == 0 ? _anglesInstances.Count - 1 : instance.Index - 1;
        int nextIndex = instance.Index == _anglesInstances.Count - 1 ? 0 : instance.Index + 1;

        float angle = CalculateAngle(_points[previousIndex].SelfImage.rectTransform.anchoredPosition,
            _points[instance.Index].SelfImage.rectTransform.anchoredPosition,
            _points[nextIndex].SelfImage.rectTransform.anchoredPosition
            );

        instance.Root.rotation = _lines[previousIndex].transform.rotation;


        float midAngle = angle / 2 * Mathf.Deg2Rad + instance.Root.rotation.z;
        float radius = 40f;
        float x = Mathf.Cos(midAngle) * radius;
        float y = Mathf.Sin(midAngle) * radius;

        // Устанавливаем позицию текста относительно родителя
        instance.Text.transform.localPosition = new Vector3(x, y, 0);
        instance.Text.rectTransform.rotation = Quaternion.identity;

        instance.Text.text = $"{angle:F2}°";
        instance.Filler.fillAmount = angle / 360;
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
        return (cross.z > 0) ? 360 - baseAngle : baseAngle;
    }

    private void TweenAnglesInstancesScale(float scale, float duration) => _anglesInstances.ForEach(instance => instance.Root.DOScale(scale, duration));

    private record AngleInstance(int Index, RectTransform Root, CanvasGroup CanvasGroup, Image Filler, TMP_Text Text)
    {
        public int Index { get; private set; } = Index;
        public RectTransform Root { get; private set; } = Root;
        public CanvasGroup CanvasGroup { get; private set; } = CanvasGroup;
        public Image Filler { get; private set; } = Filler;
        public TMP_Text Text { get; private set; } = Text;
    }
}
