using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using Zenject;

//Изменение цвета прописано костыльно, если !isVisible, то DOFade. Перенести всё в 1-2 метода.




//Изменение цвета прописано костыльно, если !isVisible, то DOFade. Перенести всё в 1-2 метода.
[Serializable]
public class BlueprintPointsConstroller : MonoBehaviour
{
    [Inject] BlueprintManager _blueprintManager;
    [Inject] BlueprintVisualConfig _visualConfig;

    public List<BlueprintPointHandler> Points { get; private set; } = new();

    [SerializeField] BlueprintPointHandler _defaultPoint;

    BlueprintPointHandler _activePoint;
    Vector2 lastMousePosition = Vector2.zero;
    
    bool _isDragging = false;
    bool _isShift = false; //!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!! New Input System


    public void OnDisable()
    {
        foreach (var point in Points)
        {
            point.PointerDown -= OnPointDown;
            point.PointerUp -= OnPointUp;

            point.PointerLeftClick -= OnPointLeftClick;
        }
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
            

        Points.ForEach(point => point.transform.localScale = Vector3.one / _blueprintManager.BlueprintScaleFactor);
    }

    public void MovePoint(BlueprintPointHandler pointHandler, Vector2 position) => pointHandler.SelfImage.rectTransform.DOAnchorPos(position, _visualConfig.PointsData.SnapDuration);
    public void MovePoint(int index, Vector2 position) => MovePoint(Points[index], position);
    public void AddPoint(int index, Vector2 position)
    {
        GameObject pointInstance = GameObject.Instantiate(_defaultPoint.gameObject, _defaultPoint.transform.parent);
        pointInstance.SetActive(true);


        BlueprintPointHandler bph = pointInstance.GetComponent<BlueprintPointHandler>();
        bph.SelfImage.color = _visualConfig.PointsData.InactivePointColor;

        SetPointVisible(bph, _visualConfig.PointsData.IsVisible);

        Points.Insert(index, bph);
        bph.SelfImage.rectTransform.anchoredPosition = position;

        bph.PointerDown += OnPointDown;
        bph.PointerUp += OnPointUp;

        bph.PointerLeftClick += OnPointLeftClick;
    }
    public void RemovePoint(int index)
    {
        BlueprintPointHandler blueprintPointHandler = Points[index];
        Points.RemoveAt(index);

        Sequence sequence = DOTween.Sequence();

        sequence.Append(blueprintPointHandler.SelfImage.DOFade(0, _visualConfig.PointsData.VisibilityFadeDuration))
            .Join(blueprintPointHandler.transform.DOScale(2, _visualConfig.PointsData.VisibilityFadeDuration)
            .OnComplete(() =>
            {
                GameObject.Destroy(blueprintPointHandler.gameObject);
            }));
    }

    public void SetPointsVisible(bool isVisible)
    {
        _visualConfig.PointsData.IsVisible = isVisible;
        foreach (BlueprintPointHandler bph in Points)
            SetPointVisible(bph, isVisible);
    }
    private void SetPointVisible(BlueprintPointHandler blueprintPointHandler, bool isVisible)
    {
        blueprintPointHandler.SelfImage.DOKill();
        blueprintPointHandler.SelfImage.DOFade(isVisible ? _visualConfig.PointsData.InactivePointColor.a : 0, _visualConfig.PointsData.VisibilityFadeDuration);
    }

    private float CalculateSnappedCoordinate(float coordinate)
    {
        float snapValue = _visualConfig.PointsData.SnapDistance / _visualConfig.PointsData.SnapSmooth / _visualConfig.PointsData.TextureTileMultiplyer;
        coordinate -= (coordinate < 0 ? snapValue / 2 : 0);

        float remainder = coordinate % snapValue;
        return coordinate - remainder + (remainder >= snapValue / 2 ? snapValue : 0);
    }

    private Vector2 CalculateInputPosition(Vector3 mousePosition)
    {
        Vector2 inputPosition = (mousePosition - _activePoint.transform.parent.position) / _blueprintManager.CanvasScaleFactor / _blueprintManager.BlueprintScaleFactor;
        if (_isShift)
            inputPosition = new Vector3(CalculateSnappedCoordinate(inputPosition.x), CalculateSnappedCoordinate(inputPosition.y), 0);

        return inputPosition;
    }

    private void OnPointDown(BlueprintPointHandler blueprintPointHandler)
    {
        _isDragging = true;
        _activePoint = blueprintPointHandler;

        blueprintPointHandler.SelfImage.DOKill();
        _activePoint.SelfImage.color = _visualConfig.PointsData.DragPointColor;
    }
    private void OnPointUp(BlueprintPointHandler blueprintPointHandler)
    {
        _blueprintManager.MovePoint(Points.IndexOf(_activePoint), _activePoint.SelfImage.rectTransform.anchoredPosition);

        lastMousePosition = CalculateInputPosition(Input.mousePosition);

        _activePoint.SelfImage.rectTransform.DOKill();
        _activePoint.SelfImage.rectTransform.DOAnchorPos(lastMousePosition, _visualConfig.PointsData.SnapDuration);

        _isDragging = false;
        _activePoint = null;

        blueprintPointHandler.SelfImage.color = _visualConfig.PointsData.InactivePointColor;
        SetPointVisible(blueprintPointHandler, _visualConfig.PointsData.IsVisible);
    }

    private void OnPointLeftClick(BlueprintPointHandler blueprintPointHandler) => _blueprintManager.RemovePoint(Points.IndexOf(blueprintPointHandler));
}
