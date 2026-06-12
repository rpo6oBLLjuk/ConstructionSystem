using UnityEngine;

[CreateAssetMenu(fileName = "ConstructionSystemConfigs", menuName = "Scriptable Objects/Construction/SystemConfig")]
public class ConstructionSystemConfig : ScriptableObject
{
    [Header("Scale")]
    [Min(0.001f)] public float UnitsToMeters = 0.01f;

    [Header("Room")]
    [Min(0.1f)] public float WallHeightMeters = 2.5f;
    [Min(0.01f)] public float WallThicknessMeters = 0.15f;

    public Vector3 BlueprintPointToWorld(Vector2 point)
    {
        return new Vector3(point.x * UnitsToMeters, 0f, point.y * UnitsToMeters);
    }

    public float ToWorldSize(float value)
    {
        return value * UnitsToMeters;
    }
}
