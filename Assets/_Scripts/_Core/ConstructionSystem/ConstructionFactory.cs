using UnityEngine;

public class ConstructionFactory
{
    private ConstructionObjectsDataContainer _constructionObjectsDataContainer;


    public void Initialize(ConstructionObjectsDataContainer constructionObjectsDataContainer) => _constructionObjectsDataContainer = constructionObjectsDataContainer;

    public ConstructionHandler InstantiateConstruction(ConstructionObjectData constructionObjectData, Vector3 position, Quaternion rotation = default, Transform parent = null)
    {
        GameObject instance;
        ConstructionHandler handler;

        instance = GameObject.Instantiate(constructionObjectData.Prefab, position, Quaternion.identity, parent);
        handler = instance.GetOrAddComponent<ConstructionHandler>();

        if (!_constructionObjectsDataContainer.TryGetCombinedMesh(constructionObjectData.Id, out Mesh mesh))
        {
            mesh = handler.InitializeWithoutMesh(constructionObjectData);
            _constructionObjectsDataContainer.AddCombinedMesh(constructionObjectData.Id, mesh);
        }
        else
        {
            handler.InitializeWithMesh(constructionObjectData, mesh);
        }

        instance.transform.rotation = rotation; //After initialize, MeshRenderer.Bounds work correctly only in default rotation (Quaternion.identity)

        return handler;
    }
}
