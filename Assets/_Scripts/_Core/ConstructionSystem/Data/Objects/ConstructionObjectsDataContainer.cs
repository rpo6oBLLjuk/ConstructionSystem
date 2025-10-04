using CustomInspector;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ConstructionObjectsDataContainer", menuName = "Scriptable Objects/Construction/ObjectsDataContainer")]
public class ConstructionObjectsDataContainer : ScriptableObject
{
    public IReadOnlyList<ConstructionObjectData> AllObjects => _allObjects.AsReadOnly();
    public IReadOnlyList<ConstructionObjectData> BuiltInObjects => _builtInObjects.AsReadOnly();
    public IReadOnlyList<ConstructionObjectData> ImportedObjects => _importedObjects.AsReadOnly();

    private List<ConstructionObjectData> _allObjects = new();
    [SerializeField] private List<ConstructionObjectData> _builtInObjects = new();
    private List<ConstructionObjectData> _importedObjects = new();

    [SerializeField] private ReorderableDictionary<int, Mesh> _meshCombines = new();


    public void Initialize()
    {
        _allObjects.Clear();
        _importedObjects.Clear();

        _allObjects = _builtInObjects.GetRange(0, _builtInObjects.Count);
        _meshCombines.Clear();
    }

    public void ImportObject(ConstructionObjectData newObject)
    {
        if (!newObject || _importedObjects.Contains(newObject))
            return;

        _importedObjects.Add(newObject);
        _allObjects.Add(newObject);

        Debug.Log($"Object {newObject.Name} imported");
    }
    public void ImportObjects(List<ConstructionObjectData> newObjects)
    {
        foreach (ConstructionObjectData newObject in newObjects)
            ImportObject(newObject);
    }

    public void RemoveImportedObject(ConstructionObjectData objectToRemove)
    {
        if (!objectToRemove || !_importedObjects.Contains(objectToRemove))
            return;

        _importedObjects.Remove(objectToRemove);
        _allObjects.Remove(objectToRemove);

        Debug.Log($"Object {objectToRemove.Name} removed");
    }
    public void RemoveImportedObjects(List<ConstructionObjectData> objectsToRemove)
    {
        foreach (ConstructionObjectData newObject in objectsToRemove)
            RemoveImportedObject(newObject);
    }
    public void ClearImportedObjects() => RemoveImportedObjects(_importedObjects);

    public void AddCombinedMesh(int id, Mesh combinedMesh)
    {
        if (!_meshCombines.ContainsKey(id))
            _meshCombines.Add(id, combinedMesh);
    }
    public bool TryGetCombinedMesh(int id, out Mesh combinedMesh)
    {
        if (_meshCombines.ContainsKey(id))
        {
            combinedMesh = _meshCombines[id];
            return true;
        }
        combinedMesh = null;
        return false;
    }

    
}