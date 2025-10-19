using System.Collections.Generic;
using UnityEngine;

public class BlueprintManager : MonoBehaviour
{
    public List<Vector2> BlueprintPoints = new();

    public BlueprintLinesController LinesController;
    public BlueprintPointsConstroller PointsController;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        PointsController.Update();

        for (int i = 0; i < BlueprintPoints.Count; i++)
        {
            BlueprintPoints[i] = PointsController.points[i].transform.position;
        }
    }
}
