using System;
using UnityEngine;

public class ConstructionController : MonoBehaviour
{
    public event Action<ConstructionObject> OnNewObjectEquiped;

    public ConstructionObjects Objects;
    public ConstructionObject EquipedObject;




    public void EquipNewObject(ConstructionObject objToEquip)
    {
        EquipedObject = objToEquip;
        OnNewObjectEquiped?.Invoke(objToEquip);
    }
}
