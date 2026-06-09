using UnityEngine;

public class ProjectData
{
    public Vector2[] points = new Vector2[]
        {
            new Vector2(-50f, 50f),
            new Vector2(-50f, -50f),
            new Vector2(50f, -50f),
            new Vector2(50f, 50f)
        };

    public float square;

    public int[] items;             // Items Id
    public Vector3[] positions;     // position
    public Quaternion[] rotations;  // rotation
}
