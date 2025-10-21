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

            _activePoint.transform.position = inputPosition;
        }
    }

    public void AddPoint(int index, Vector2 position)
    {
        GameObject pointInstance = GameObject.Instantiate(_defaultPoint.gameObject, _defaultPoint.transform.parent);
        pointInstance.SetActive(true);

        BlueprintPointHandler bph = pointInstance.GetComponent<BlueprintPointHandler>();
        bph.SelfImage.color = _inactivePointColor;

        Points.Insert(index, bph);
        bph.transform.position = position;

        bph.PointerDown += OnPointDown;
        bph.PointerUp += OnPointUp;

        bph.PointerLeftClick += OnPointLeftClick;
    }

    public void RemovePoint(int index)
    {
        GameObject.Destroy(Points[index].gameObject);
        Points.RemoveAt(index);
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
        _blueprintManager.MovePoint(Points.IndexOf(_activePoint), _activePoint.transform.position);

        _isDragging = false;
        _activePoint = null;

        blueprintPointHandler.SelfImage.color = _inactivePointColor;
    }

    private void OnPointLeftClick(BlueprintPointHandler blueprintPointHandler) => _blueprintManager.RemovePoint(Points.IndexOf(blueprintPointHandler));
}
