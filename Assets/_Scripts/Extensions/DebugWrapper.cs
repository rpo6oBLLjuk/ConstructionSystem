using UnityEngine;

public static class DebugWrapper
{
    public static void Log(this object obj, string message, string sender = null, Object context = null) => Debug.Log($"[{sender ?? obj.GetType().Name}] {message}", context);
    public static void LogWarning(this object obj, string message, string sender = null, Object context = null) => Debug.LogWarning($"[{sender ?? obj.GetType().Name}] {message}", context);
    public static void LogError(this object obj, string message, string sender = null, Object context = null) => Debug.LogError($"[{sender ?? obj.GetType().Name}] {message}", context);

    public static void FastLog(this object obj, string message, string sender = null, Object context = null) => Debug.Log($"<color=#ff0000>[{sender ?? obj.GetType().Name}] {message}</color>", context);
    public static void InactiveLog(this object obj, string message, string sender = null, Object context = null) => Debug.Log($"<color=#666666>[{sender ?? obj.GetType().Name}] {message}</color>", context);
}