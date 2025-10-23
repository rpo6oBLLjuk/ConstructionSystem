using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

//Изменение цвета прописано костыльно, если !isVisible, то DOFade. Перенести всё в 1-2 метода.
[Serializable]
public class BlueprintPointsConstroller
{
    BlueprintManager _blueprintManager;

    public List<BlueprintPointHandler> Points { get; private set; } = new();

    [SerializeField] BlueprintPointHandler _defaultPoint;

    [SerializeField] float _snapDistance = 10;
    [SerializeField] float _snapSmooth = 5;

    [SerializeField] Color _inactivePointColor = Color.white; //!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!! Вынести в конфиг
    [SerializeField] Color _dragPointColor = Color.yellow;

    [SerializeField] float _snapDuration = 0.05f; //!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!! Вынести в конфиг

    [SerializeField] float _visibilityFadeDuration = 0.25f; //!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!! Вынести в конфиг

    BlueprintPointHandler _activePoint;
    bool _isDragging = false;
    bool _isShift = false; //!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!! New Input System

    bool _isVisible = true;


    //public void OnEnable()
    //{
    //    foreach (var point in Points)
    //    {
    //        point.PointerDown += OnPointDown;
    //        point.PointerUp += OnPointUp;
    //    }
    //}
    public void OnDisable()
    {
        foreach (var point in Points)
        {
            point.PointerDown -= OnPointDown;
            point.PointerUp -= OnPointUp;

            point.PointerLeftClick -= OnPointLeftClick;
        }
    }

    public void Awake(BlueprintManager blueprintManager)
    {
        _blueprintManager = blueprintManager;
        _defaultPoint.gameObject.SetActive(false);
    }
    public void Update()
    {
        if (_isDragging)
        {
            _isShift = Input.GetKey(KeyCode.LeftShift); //!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!! New Input System

            Vector3 inputPosition = Input.mousePosition;
            inputPosition = (inputPosition - _activePoint.transform.parent.position) / _blueprintManager.CanvasScaleFactor / _blueprintManager.BlueprintScaleFactor;
            if (_isShift)
                inputPosition = new Vector3(CalculateSnappedCoordinate(inputPosition.x), CalculateSnappedCoordinate(inputPosition.y), 0);

            _activePoint.SelfImage.rectTransform.DOAnchorPos(inputPosition, _snapDuration);
        }
    }

    public void AddPoint(int index, Vector2 position)
    {
        GameObject pointInstance = GameObject.Instantiate(_defaultPoint.gameObject, _defaultPoint.transform.parent);
        pointInstance.SetActive(true);

        BlueprintPointHandler bph = pointInstance.GetComponent<BlueprintPointHandler>();
        bph.SelfImage.color = _inactivePointColor;
        if (!_isVisible)
            bph.SelfImage.DOFade(0, _visibilityFadeDuration);

        Points.Insert(index, bph);
        bph.SelfImage.rectTransform.anchoredPosition = position;

        bph.PointerDown += OnPointDown;
        bph.PointerUp += OnPointUp;

        bph.PointerLeftClick += OnPointLeftClick;
    }
    public void RemovePoint(int index, float durationTime)
    {
        BlueprintPointHandler bph = Points[index];
        Points.RemoveAt(index);

        Sequence sequence = DOTween.Sequence();

        sequence.Append(bph.SelfImage.DOFade(0, durationTime))
            .Join(bph.transform.DOScale(2, durationTime)
            .OnComplete(() =>
            {
                GameObject.Destroy(bph.gameObject);
            }));
    }

    public void SetPointsVisible(bool isVisible)
    {
        _isVisible = isVisible;
        foreach (BlueprintPointHandler bph in Points)
            bph.SelfImage.DOFade(isVisible ? _inactivePointColor.a : 0, _visibilityFadeDuration);
    }

    private float CalculateSnappedCoordinate(float coordinate)
    {
        float snapValue = _snapDistance / _snapSmooth;
        float remainder = coordinate % snapValue;
        return coordinate - remainder + (remainder >= snapValue / 2 ? snapValue : 0);
    }

    private void OnPointDown(BlueprintPointHandler blueprintPointHandler)
    {
        _isDragging = true;
        _activePoint = blueprintPointHandler;

        _activePoint.SelfImage.color = _dragPointColor;
    }
    private void OnPointUp(BlueprintPointHandler blueprintPointHandler)
    {
        _blueprintManager.MovePoint(Points.IndexOf(_activePoint), _activePoint.SelfImage.rectTransform.anchoredPosition);

        _isDragging = false;
        _activePoint = null;

        blueprintPointHandler.SelfImage.color = _inactivePointColor;
        if (!_isVisible)
            blueprintPointHandler.SelfImage.DOFade(0, _visibilityFadeDuration);
    }

    private void OnPointLeftClick(BlueprintPointHandler blueprintPointHandler) => _blueprintManager.RemovePoint(Points.IndexOf(blueprintPointHandler));
}
