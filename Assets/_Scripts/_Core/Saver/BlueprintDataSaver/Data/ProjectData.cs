using UnityEngine;

public class ProjectData
{
    public int userId;

    public Vector2[] points;

    public float square;

    public int[] items;             // Id
    public Vector3[] positions;     // position
    public Quaternion[] rotations;  // rotation

    public ProjectData()
    {
        points = new Vector2[]
        {
            new Vector2(-50f, 50f),
            new Vector2(-50f, -50f),
            new Vector2(50f, -50f),
            new Vector2(50f, 50f)
        };
    }

    public ProjectData(int userId)
    {
        this.userId = userId;
    }
}
