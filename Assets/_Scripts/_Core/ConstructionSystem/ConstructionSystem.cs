using System;
using UnityEngine;

public class ConstructionSystem : MonoBehaviour
{
    public event Action<ConstructionObjectData> OnNewConstructionSelected;

    public ConstructionObjectsDataContainer Constructions;
    public ConstructionObjectData CurrentConstructionData;
    //public ConstructionHandler SelectedConstruction;


    private void Awake()
    {
        Constructions.Initialize();
        SelectNewConstruction(CurrentConstructionData);
    }

    public void SelectNewConstruction(ConstructionObjectData contructionToSelect)
    {
        //GameObject construction = Instantiate(contructionToSelect.Prefab, Vector3.zero + Vector3.down * 10, Quaternion.identity);

        //ConstructionHandler SelectedConstruction = construction.GetComponent<ConstructionHandler>();
        //SelectedConstruction.Instantiate(contructionToSelect, Constructions);
        OnNewConstructionSelected?.Invoke(contructionToSelect);
    }
}
