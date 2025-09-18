using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "ConstructionObjects", menuName = "Scriptable Objects/ConstructionObjects")]
public class ConstructionObjects : ScriptableObject
{
    public IReadOnlyList<GameObject> AllObjects => _allObjects.AsReadOnly();
    public IReadOnlyList<GameObject> BuiltInObjects => _builtInObjects.AsReadOnly();
    public IReadOnlyList<GameObject> ImportedObjects => _importedObjects.AsReadOnly();

    [SerializeField] private List<GameObject> _allObjects = new();
    [SerializeField] private List<GameObject> _builtInObjects = new();
    [SerializeField] private List<GameObject> _importedObjects = new();


    public void ImportObject(GameObject newObject)
    {
        if (!newObject || _importedObjects.Contains(newObject))
            return;

        _importedObjects.Add(newObject);
        _allObjects.Add(newObject);

        Debug.Log($"Object {newObject.name} imported");
    }
    public void ImportObjects(List<GameObject> newObjects)
    {
        foreach (GameObject newObject in newObjects)
            ImportObject(newObject);
    }

    public void RemoveImportedObject(GameObject objectToRemove)
    {
        if (!objectToRemove || !_importedObjects.Contains(objectToRemove))
            return;

        _importedObjects.Remove(objectToRemove);
        _allObjects.Remove(objectToRemove);

        Debug.Log($"Object {objectToRemove.name} removed");
    }
    public void RemoveImportedObjects(List<GameObject> objectsToRemove)
    {
        foreach (GameObject newObject in objectsToRemove)
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
