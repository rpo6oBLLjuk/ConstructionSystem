using UnityEngine;

public class ConstructionFactory
{
    private ConstructionObjectsDataContainer _constructionObjectsDataContainer;
    private MeshCombiner _meshCombiner = new();


    public void Initialize(ConstructionObjectsDataContainer constructionObjectsDataContainer) => _constructionObjectsDataContainer = constructionObjectsDataContainer;

    public ConstructionHandler InstantiateConstruction(ConstructionObjectData constructionObjectData, Vector3 position, Quaternion rotation = default, Transform parent = null)
    {
        GameObject instance;
        ConstructionHandler handler;

        if (!_constructionObjectsDataContainer.TryGetCombinedMesh(constructionObjectData.Id, out Mesh mesh))
        {
            mesh = _meshCombiner.GetCombinedMesh(constructionObjectData.Prefab.transform);
            _constructionObjectsDataContainer.AddCombinedMesh(constructionObjectData.Id, mesh);
        }

        instance = GameObject.Instantiate(new GameObject(constructionObjectData.Prefab.name), position, Quaternion.identity, parent);
        handler = instance.GetOrAddComponent<ConstructionHandler>();

        (MeshFilter meshFilter, MeshRenderer meshRenderer) = _meshCombiner.AddMeshToTransform(instance.transform, mesh, constructionObjectData.Prefab.GetComponentInChildren<MeshRenderer>(true).sharedMaterial);
        (BoxCollider boxCollider, MeshCollider meshCollider) = _meshCombiner.AddCollidersToTransform(instance.transform, meshRenderer);

        handler.InjectReferences(constructionObjectData, boxCollider, meshCollider, meshFilter, meshRenderer);

        //throw new System.Exception("WWWWWWWWWW");

        instance.transform.rotation = rotation; //After initialize, MeshRenderer.Bounds work correctly only in default rotation (Quaternion.identity)

        return handler;
    }
}
