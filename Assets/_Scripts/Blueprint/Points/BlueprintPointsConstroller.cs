using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class BlueprintPointsConstroller
{
    BlueprintManager _blueprintManager;

    [SerializeField] BlueprintPointHandler _defaultPoint;
    
    public List<BlueprintPointHandler> points;
    Image activePoint;

    private bool isActiveDragging = false;


    public void OnEnable()
    {
        foreach (var point in points)
        {
            point.PointerDown += OnPointDown;
            point.PointerUp += OnPointUp;
        }
    }
    public void OnDisable()
    {
        foreach (var point in points)
        {
            point.PointerDown -= OnPointDown;
            point.PointerUp -= OnPointUp;
        }
    }

    public void Awake(BlueprintManager blueprintManager) => _blueprintManager = blueprintManager;
    public void Update()
    {
        if (isActiveDragging)
            activePoint.rectTransform.position = Input.mousePosition;
    }

    public void AddPoint(int index, Vector2 position)
    {
        GameObject pointInstance = GameObject.Instantiate(_defaultPoint.gameObject, _defaultPoint.transform.parent);
        //
        //
        //
    }


    void OnPointDown(Image image)
    {
        isActiveDragging = true;
        activePoint = image;
    }
    void OnPointUp(Image image)
    {
        isActiveDragging = false;
        activePoint = null;
    }
}
