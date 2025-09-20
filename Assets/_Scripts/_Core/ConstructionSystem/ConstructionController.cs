using System;
using UnityEngine;

public class ConstructionController : MonoBehaviour
{
    public event Action<ConstructionObject> OnNewObjectSelected;

    public ConstructionObjects Objects;
    public ConstructionObject SelectedObject;


    public void SelectNewObject(ConstructionObject objToSelect)
    {
        SelectedObject = objToSelect;
        OnNewObjectSelected?.Invoke(objToSelect);
    }
}
