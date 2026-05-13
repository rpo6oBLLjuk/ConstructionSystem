using UnityEngine;

public class BlueprintData
{
    public Vector2[] points;

    public string name;
    public string editTime;
    public float square;

    public BlueprintData()
    {
        points = new Vector2[]
        {
            new Vector2(-50f, 50f),
            new Vector2(-50f, -50f),
            new Vector2(50f, -50f),
            new Vector2(50f, 50f)
        };
    }
}
