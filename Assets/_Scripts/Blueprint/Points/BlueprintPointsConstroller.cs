using DG.Tweening;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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

    BlueprintPointHandler _activePoint;
    bool _isDragging = false;
    bool _isShift = false; //!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!! New Input System


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
            if (_isShift)
                inputPosition = new Vector3(CalculateSnappedCoordinate(inputPosition.x), CalculateSnappedCoordinate(inputPosition.y), 0);

            _activePoint.transform.DOMove(inputPosition, _snapDuration);
        }
    }

    public void AddPoint(int index, Vector2 position)
    {
        GameObject pointInstance = GameObject.Instantiate(_defaultPoint.gameObject, _defaultPoint.transform.parent);
        pointInstance.SetActive(true);

        BlueprintPointHandler bph = pointInstance.GetComponent<BlueprintPointHandler>();
        bph.SelfImage.color = _inactivePointColor;

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

    private float CalculateSnappedCoordinate(float coordinate)
    {
        float snapValue = _snapDistance * _blueprintManager.ScaleFactor / _snapSmooth;
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
    }

    private void OnPointLeftClick(BlueprintPointHandler blueprintPointHandler) => _blueprintManager.RemovePoint(Points.IndexOf(blueprintPointHandler));
}
