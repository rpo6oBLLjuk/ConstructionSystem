using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class BlueprintPointsConstroller : MonoBehaviour
{
    [SerializeField] List<EventTrigger> points;
    [SerializeField] Transform activePoint;

    private bool isActiveDragging = false;


    void Start()
    {
        foreach (var point in points)
        {
            //EventTrigger.E  ntry pointerUp = new EventTrigger.Entry();
            //pointerUp.callback
        }
    }

    void Update()
    {
        
    }
}
