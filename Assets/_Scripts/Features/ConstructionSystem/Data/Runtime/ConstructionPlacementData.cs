using UnityEngine;

public record ConstructionPlacementData(Vector3 TargetPosition, Quaternion TargetRotation, bool HasCollisions, Collider[] CollidingObjects)
{
    public Vector3 TargetPosition { get; private set; } = TargetPosition;
    public Quaternion TargetRotation { get; private set; } = TargetRotation;
    public bool HasCollisions { get; private set; } = HasCollisions;
    public Collider[] CollidingObjects { get; private set; } = CollidingObjects;
}
