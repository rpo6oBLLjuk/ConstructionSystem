using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

 
[Serializable]
public class BlueprintPointsConstroller : BlueprintView<Image>
{
    protected override IEnumerable<Image> ViewList => Points.Select(point => point.SelfImage);
    protected override BlueprintViewLayers ViewLayer => BlueprintViewLayers.Points;

    public List<BlueprintPointHandler> Points { get; private set; } = new();
    [SerializeField] BlueprintPointHandler _defaultPoint;

    BlueprintPointHandler _activePoint;
    Vector2 lastMousePosition = Vector2.zero;

    bool _isDragging = false;
    bool _isShift = false;


    protected override void OnDisable()
    {
        base.OnDisable();
        foreach (var point in Points)
            RemoveListenersToPoint(point);
    }

    public void Awake() => _defaultPoint.gameObject.SetActive(false);
    public void Update()
    {
        _isShift = Input.GetKey(KeyCode.LeftShift); //!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!! New Input System

        if (_isDragging)
        {
            Vector3 inputPosition = CalculateInputPosition(Input.mousePosition);
            MovePoint(_activePoint, inputPosition);
        }


        Points.ForEach(point => point.transform.localScale = Vector3.one / _blueprintManager.ScaleFactor);
    }

    public void MovePoint(BlueprintPointHandler pointHandler, Vector2 position) => pointHandler.SelfImage.rectTransform.DOAnchorPos(position, _blueprintVisualConfig.PointsData.SnapDuration);
    public void MovePoint(int index, Vector2 position) => MovePoint(Points[index], position);

    public void AddPoint(int index, Vector2 position)
    {
        GameObject pointInstance = GameObject.Instantiate(_defaultPoint.gameObject, _defaultPoint.transform.parent);
        pointInstance.SetActive(true);

        BlueprintPointHandler bph = pointInstance.GetComponent<BlueprintPointHandler>();
        SetGraphicVisible(bph.SelfImage, _blueprintVisualConfig.PointsData.InactivePointColor);

        Points.Insert(index, bph);
        bph.SelfImage.rectTransform.anchoredPosition = position;

        AddListenersToPoint(bph);
    }
    public void RemovePoint(int index)
    {
        BlueprintPointHandler blueprintPointHandler = Points[index];
        Points.RemoveAt(index);

        RemoveListenersToPoint(blueprintPointHandler);

        Sequence sequence = DOTween.Sequence();
        sequence.Append(blueprintPointHandler.SelfImage.DOFade(0, _blueprintVisualConfig.PointsData.VisibilityFadeDuration))
            .Join(blueprintPointHandler.transform.DOScale(2, _blueprintVisualConfig.PointsData.VisibilityFadeDuration)
            .OnComplete(() =>
            {
                GameObject.Destroy(blueprintPointHandler.gameObject);
            }));
    }

    private float CalculateSnappedCoordinate(float coordinate)
    {
        float snapValue = _blueprintVisualConfig.PointsData.SnapDistance / _blueprintVisualConfig.PointsData.SnapSmooth / _blueprintVisualConfig.PointsData.TextureTileMultiplyer;
        coordinate -= (coordinate < 0 ? snapValue / 2 : 0);

        float remainder = coordinate % snapValue;
        return coordinate - remainder + (remainder >= snapValue / 2 ? snapValue : 0);
    }
    private Vector2 CalculateInputPosition(Vector3 mousePosition)
    {
        Vector2 inputPosition = (mousePosition - _activePoint.transform.parent.position) / _blueprintManager.CanvasScaleFactor / _blueprintManager.ScaleFactor;
        if (_isShift)
            inputPosition = new Vector3(CalculateSnappedCoordinate(inputPosition.x), CalculateSnappedCoordinate(inputPosition.y), 0);

        return inputPosition;
    }

    private void OnPointDown(BlueprintPointHandler blueprintPointHandler)
    {
        _isDragging = true;
        _activePoint = blueprintPointHandler;

        blueprintPointHandler.SelfImage.DOKill();
        _activePoint.SelfImage.color = _blueprintVisualConfig.PointsData.DragPointColor;
    }
    private void OnPointUp(BlueprintPointHandler blueprintPointHandler)
    {
        _blueprintManager.MovePoint(Points.IndexOf(_activePoint), _activePoint.SelfImage.rectTransform.anchoredPosition);

        lastMousePosition = CalculateInputPosition(Input.mousePosition);

        _activePoint.SelfImage.rectTransform.DOKill();
        _activePoint.SelfImage.rectTransform.DOAnchorPos(lastMousePosition, _blueprintVisualConfig.PointsData.SnapDuration);

        _isDragging = false;
        _activePoint = null;

        SetGraphicVisible(blueprintPointHandler.SelfImage, _blueprintVisualConfig.PointsData.InactivePointColor);
    }

    private void OnPointLeftClick(BlueprintPointHandler blueprintPointHandler) => _blueprintManager.RemovePoint(Points.IndexOf(blueprintPointHandler));

    private void AddListenersToPoint(BlueprintPointHandler blueprintPointHandler)
    {
        blueprintPointHandler.PointerDown += OnPointDown;
        blueprintPointHandler.PointerUp += OnPointUp;

        blueprintPointHandler.PointerLeftClick += OnPointLeftClick;
    }
    private void RemoveListenersToPoint(BlueprintPointHandler blueprintPointHandler)
    {
        blueprintPointHandler.PointerDown -= OnPointDown;
        blueprintPointHandler.PointerUp -= OnPointUp;

        blueprintPointHandler.PointerLeftClick -= OnPointLeftClick;
    }
}
