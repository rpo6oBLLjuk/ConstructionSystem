using UnityEngine;

public class ConstructionHandler : MonoBehaviour
{
    public ConstructionObjectData Data { get; private set; }

    //public Bounds Bounds => 

    public BoxCollider BoxCollider { get; private set; }
    public MeshCollider MeshCollider { get; private set; }

    public MeshFilter MeshFilter { get; private set; }
    public MeshRenderer MeshRenderer { get; private set; }

    //private ConstructionObjectsDataContainer _constructionObjectsDataContainer;

    private MeshCombiner _meshCombiner = new();
    private Mesh mesh;

    //Handler не должен знать о глобальном конфиге и т€нуть его зависимость 

    public void InitializeWithMesh(ConstructionObjectData data, Mesh mesh)
    {
        Data = data;
        this.mesh = mesh;

        InitializeComponents();
    }
    public Mesh InitializeWithoutMesh(ConstructionObjectData data) //ѕопытатьс€ удалить второй аргумент
    {
        Data = data;
        mesh = GetCombinedMesh();

        InitializeComponents();

        return mesh;
    }

    private Mesh GetCombinedMesh() => _meshCombiner.GetCombinedMesh(transform);

    private void InitializeComponents()
    {
        (MeshFilter, MeshRenderer) = _meshCombiner.AddMeshToTransform(transform, mesh);
        (BoxCollider, MeshCollider) = _meshCombiner.AddCollidersToTransform(transform, MeshRenderer);
    }

}
