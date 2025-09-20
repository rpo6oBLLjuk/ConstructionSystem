using UnityEngine;
using System.Collections.Generic;
using System;

[CreateAssetMenu(fileName = "ConstructionObjectsContainer", menuName = "Scriptable Objects/Construction/ObjectsContainer")]
public class ConstructionObjects : ScriptableObject
{
    public IReadOnlyList<ConstructionObject> AllObjects => _allObjects.AsReadOnly();
    public IReadOnlyList<ConstructionObject> BuiltInObjects => _builtInObjects.AsReadOnly();
    public IReadOnlyList<ConstructionObject> ImportedObjects => _importedObjects.AsReadOnly();

    private List<ConstructionObject> _allObjects = new();
    [SerializeField] private List<ConstructionObject> _builtInObjects = new();
    private List<ConstructionObject> _importedObjects = new();


    public void ImportObject(ConstructionObject newObject)
    {
        if (!newObject || _importedObjects.Contains(newObject))
            return;

        _importedObjects.Add(newObject);
        _allObjects.Add(newObject);

        Debug.Log($"Object {newObject.Name} imported");
    }
    public void ImportObjects(List<ConstructionObject> newObjects)
    {
        foreach (ConstructionObject newObject in newObjects)
            ImportObject(newObject);
    }

    public void RemoveImportedObject(ConstructionObject objectToRemove)
    {
        if (!objectToRemove || !_importedObjects.Contains(objectToRemove))
            return;

        _importedObjects.Remove(objectToRemove);
        _allObjects.Remove(objectToRemove);

        Debug.Log($"Object {objectToRemove.Name} removed");
    }
    public void RemoveImportedObjects(List<ConstructionObject> objectsToRemove)
    {
        foreach (ConstructionObject newObject in objectsToRemove)
            RemoveImportedObject(newObject);
    }
    public void ClearImportedObjects() => RemoveImportedObjects(_importedObjects);

    private void Awake()
    {
        _allObjects.Clear();
        _importedObjects.Clear();

        _allObjects = _builtInObjects;
    }
}