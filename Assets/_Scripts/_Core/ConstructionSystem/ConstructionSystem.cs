using System;
using UnityEngine;

public class ConstructionSystem : MonoBehaviour
{
    public event Action<ConstructionObject> OnNewConstructionSelected;

    public ConstructionObjects Constructions;
    public ConstructionObject SelectedConstruction;


    public void SelectNewConstruction(ConstructionObject contructionToSelect)
    {
        SelectedConstruction = contructionToSelect;
        OnNewConstructionSelected?.Invoke(contructionToSelect);
    }
}
