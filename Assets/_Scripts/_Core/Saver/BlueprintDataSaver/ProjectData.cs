using System;
using UnityEngine;

public class ProjectData
{
    public Vector2[] Points = new Vector2[]
    {
        new (-50f, 50f),
        new (-50f, -50f),
        new (50f, -50f),
        new (50f, 50f)
    };

    public float Perimeter;
    public float Square;

    public PlacedFurnitureData[] Furniture;
}

[Serializable]
public class PlacedFurnitureData
{
    public int itemId;
    public Vector3 position;
    public Vector3 rotation;
}