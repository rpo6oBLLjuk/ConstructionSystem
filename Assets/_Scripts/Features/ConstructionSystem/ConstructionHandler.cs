using UnityEngine;

public class ConstructionHandler : MonoBehaviour
{
    public ConstructionObjectData Data { get; private set; }

    public BoxCollider BoxCollider { get; private set; }
    public MeshCollider MeshCollider { get; private set; }

    public MeshFilter MeshFilter { get; private set; }
    public MeshRenderer MeshRenderer { get; private set; }


    public void InjectReferences(ConstructionObjectData data, BoxCollider boxCollider, MeshCollider meshCollider, MeshFilter meshFilter, MeshRenderer meshRenderer)
    {
        Data = data;

        BoxCollider = boxCollider;
        MeshCollider = meshCollider;

        MeshFilter = meshFilter;
        MeshRenderer = meshRenderer;
    }
}
