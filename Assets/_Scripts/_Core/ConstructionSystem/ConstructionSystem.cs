using System;
using UnityEngine;
using Zenject;

public class ConstructionSystem : MonoBehaviour
{
    [Inject] DiContainer _container;

    public event Action<SelectedConstructionData> OnNewConstructionSelected;
    public event Action<ConstructionPlacementData> OnTargetPlacementDataUpdated;

    public ConstructionObjectsDataContainer Constructions;
    public SelectedConstructionData CurrentConstructionData;

    public ConstructionPlacementData PlacementData;

    private ConstructionBuilder _constructionBuilder;
    private ConstructionRaycaster _constructionRaycaster;
    private ConstructionRotator _constructionRotator;

    private ConstructionFactory _constructionFactory = new();


    private void Awake()
    {
        Constructions.Initialize();
        _constructionFactory.Initialize(Constructions);

        _constructionBuilder = _container.Instantiate<ConstructionBuilder>();
        _constructionRaycaster = _container.Instantiate<ConstructionRaycaster>();
        _constructionRotator = _container.Instantiate<ConstructionRotator>();

        SelectNewConstruction(Constructions.AllObjects[0]);
    }

    //Single Update point for modules
    private void Update()
    {
        Vector3 pos = _constructionRaycaster.Update();
        Quaternion rot = _constructionRotator.Update();
        (bool hasCollision, Collider[] colliders) = _constructionBuilder.Update(pos, rot);

        PlacementData = new(
            pos,
            rot,
            hasCollision, colliders);

        OnTargetPlacementDataUpdated?.Invoke(PlacementData);
    }

    public void SelectNewConstruction(ConstructionObjectData contructionToSelect)
    {
        ConstructionHandler constructionHandler = _constructionFactory.InstantiateConstruction(contructionToSelect, Vector3.zero + Vector3.down * 10, Quaternion.identity);

        CurrentConstructionData = new(
            constructionHandler.BoxCollider,
            constructionHandler.MeshCollider,
            constructionHandler.MeshFilter,
            contructionToSelect);

        OnNewConstructionSelected?.Invoke(CurrentConstructionData);
    }

    /// <summary>
    /// </summary>
    /// <param name="contructionToInstantiate">Default: CurrentConstructionData</param>
    /// <param name="position">Default: Vector3.zero</param>
    /// <param name="rotation">Default: Quaternion.identity</param>
    /// <param name="parent">Default: null</param>
    public ConstructionHandler InstantiateCustomConstruction(ConstructionObjectData contructionToInstantiate, Vector3 position = default, Quaternion rotation = default, Transform parent = null)
    {
        if (!contructionToInstantiate)
            contructionToInstantiate = CurrentConstructionData.ConstructionObjectData;

        return _constructionFactory.InstantiateConstruction(contructionToInstantiate, position, rotation, parent);
    }
    public ConstructionHandler InstantiateCurrentConstruction(Vector3 position = default, Quaternion rotation = default, Transform parent = null) => _constructionFactory.InstantiateConstruction(CurrentConstructionData.ConstructionObjectData, position, rotation, parent);
}
