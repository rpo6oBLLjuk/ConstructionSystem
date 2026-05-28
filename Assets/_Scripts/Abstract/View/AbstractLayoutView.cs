using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public class AbstractLayoutView<TData, TLayoutFactory, TEventsContext> : MonoBehaviour
    where TData : IDBEntity, new()
    where TLayoutFactory : AbstractLayoutFactory<TData, TEventsContext>, new()
    where TEventsContext : AbstractLayoutEventsContext, new()
{
    public TEventsContext EventsContext = new();

    [SerializeField] protected Transform _defaultObject;

    protected TLayoutFactory layoutFactory = new();
    protected Dictionary<int, (TData, GameObject)> objectsList = new();


    private void Awake() => _defaultObject.gameObject.SetActive(false);

    public void UpdateDataContext(List<TData> data)
    {
        ClearLayout();
        data.ForEach(currentData => CreateUIElement(currentData));
    }

    public void CreateUIElement(TData data)
    {
        if (objectsList.ContainsKey(data.Id))
        {
            RefreshUIElement(data);
            return;
        }

        GameObject instance = layoutFactory.Instantiate(_defaultObject.gameObject, _defaultObject.parent, data, EventsContext);
        objectsList.Add(data.Id, (data, instance));
    }
    public void RefreshUIElement(TData data) => layoutFactory.FillData(objectsList[data.Id].Item2, data);
    public void RemoveUIElement(TData data) => RemoveUIElement(data.Id);
    public void RemoveUIElement(int id)
    {
        if (!objectsList.ContainsKey(id))
            return;

        layoutFactory.Destroy(objectsList[id].Item2);
        objectsList.Remove(id);
    }

    public void ClearLayout()
    {
        objectsList.Values.ToList().ForEach(tuple => layoutFactory.Destroy(tuple.Item2));
        objectsList.Clear();
    }
}
