using UnityEngine;

public class ConstructionHandler : MonoBehaviour
{
    public ConstructionObjectData Data { get; private set; }

    //public Bounds Bounds => 

    public BoxCollider BoxCollider { get; private set; }
    public MeshCollider MeshCollider { get; private set; }

    public MeshFilter MeshFilter { get; private set; }
    public MeshRenderer MeshRenderer { get; private set; }

    private ConstructionObjectsDataContainer _constructionObjectsDataContainer;

    private MeshCombiner _meshCombiner = new();


    public void Initialize(ConstructionObjectData data, ConstructionObjectsDataContainer constructionObjectsDataContainer)
    {
        Data = data;
        _constructionObjectsDataContainer = constructionObjectsDataContainer;
        InitializeComponents();
    }

    private void InitializeComponents()
    {
        if (!_constructionObjectsDataContainer.TryGetCombinedMesh(Data.Id, out Mesh mesh))
        {
            mesh = _meshCombiner.GetCombinedMesh(transform);
            _constructionObjectsDataContainer.AddCombinedMesh(Data.Id, mesh);
        }

        (MeshFilter, MeshRenderer) = _meshCombiner.AddMeshToTransform(transform, mesh);
        (BoxCollider, MeshCollider) = _meshCombiner.AddCollidersToTransform(transform, MeshRenderer);
    }

}
