using UnityEngine;

public abstract class AbstractLayoutFactory<TData, TEventsContext> where TData : IDBEntity where TEventsContext : AbstractLayoutEventsContext
{
    public GameObject Instantiate(GameObject defaultObject, Transform parent, TData data, TEventsContext context)
    {
        GameObject instance = GameObject.Instantiate(defaultObject, parent);

        FillData(instance, data);
        FillListeners(instance, data, context);

        return instance;
    }

    public void Destroy(GameObject instance) => GameObject.Destroy(instance);

    public virtual void FillData(GameObject gameObject, TData data) { }
    public virtual void FillListeners(GameObject gameObject, TData data, TEventsContext context) { }
}
