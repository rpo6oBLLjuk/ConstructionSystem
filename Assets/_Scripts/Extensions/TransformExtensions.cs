using UnityEngine;

public static class TransformExtensions
{
    public static T GetOrAddComponent<T>(this Transform transform) where T : Component => transform.TryGetComponent<T>(out T component) ? component : transform.gameObject.AddComponent<T>();
}