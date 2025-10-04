using UnityEngine;

public record SelectedConstructionData(BoxCollider BoxCollider, MeshCollider MeshCollider, MeshFilter MeshFilter, ConstructionObjectData ConstructionObjectData)
{
    public BoxCollider BoxCollider { get; private set; } = BoxCollider;
    public MeshCollider MeshCollider { get; private set; } = MeshCollider;
    public MeshFilter MeshFilter { get; private set; } = MeshFilter;

    public ConstructionObjectData ConstructionObjectData { get; private set; } = ConstructionObjectData;
}
