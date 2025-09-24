using System;
using UnityEngine;

public class ConstructionSystem : MonoBehaviour
{
    public event Action<ConstructionObjectData> OnNewConstructionSelected;

    public ConstructionObjectsDataContainer Constructions;
    public ConstructionObjectData CurrentConstructionData;

    private ConstructionFactory _constructionFactory = new();
    //public ConstructionHandler SelectedConstruction;


    private void Awake()
    {
        Constructions.Initialize();
        _constructionFactory.Initialize(Constructions);

        SelectNewConstruction(CurrentConstructionData);
    }

    public void SelectNewConstruction(ConstructionObjectData contructionToSelect)
    {
        //GameObject construction = Instantiate(contructionToSelect.Prefab, Vector3.zero + Vector3.down * 10, Quaternion.identity);

        //ConstructionHandler SelectedConstruction = construction.GetComponent<ConstructionHandler>();
        //SelectedConstruction.Instantiate(contructionToSelect, Constructions);
        OnNewConstructionSelected?.Invoke(contructionToSelect);
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
            contructionToInstantiate = CurrentConstructionData;

        return _constructionFactory.InstantiateConstruction(contructionToInstantiate, position, rotation, parent);
    }

    public ConstructionHandler InstantiateCurrentConstruction(Vector3 position = default, Quaternion rotation = default, Transform parent = null) => _constructionFactory.InstantiateConstruction(CurrentConstructionData, position, rotation, parent);
}
