using UnityEngine;

public static class TransformExtensions
{
    public static T GetOrAddComponent<T>(this Transform transform) where T : Component => transform.TryGetComponent<T>(out T component) ? component : transform.gameObject.AddComponent<T>();
}

public static class GameobjectExtensions
{
    public static T GetOrAddComponent<T>(this GameObject gameObject) where T : Component => gameObject.TryGetComponent<T>(out T component) ? component : gameObject.AddComponent<T>();
}