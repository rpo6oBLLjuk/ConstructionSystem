using UnityEngine;

public static class DebugWrapper
{
    private const string _fastLogColor = "#ff0000";
    private const string _successLogColor = "#00ff00";
    private const string _inactiveLogColor = "#666666";


    public static void Log(this object obj, string message, string sender = null, Object context = null) => Debug.Log($"[{sender ?? obj.GetType().Name}] {message}\n", context);
    public static void LogWarning(this object obj, string message, string sender = null, Object context = null) => Debug.LogWarning($"[{sender ?? obj.GetType().Name}] {message}\n", context);
    public static void LogError(this object obj, string message, string sender = null, Object context = null) => Debug.LogError($"[{sender ?? obj.GetType().Name}] {message}\n", context);

    public static void FastLog(this object obj, string message, string sender = null, Object context = null) => Debug.Log($"<color={_fastLogColor}>[{sender ?? obj.GetType().Name}] {message}\n</color>", context);
    public static void SuccessLog(this object obj, string message, string sender = null, Object context = null) => Debug.Log($"<color={_successLogColor}>[{sender ?? obj.GetType().Name}] {message}\n</color>", context);
    public static void InactiveLog(this object obj, string message, string sender = null, Object context = null) => Debug.Log($"<color={_inactiveLogColor}>[{sender ?? obj.GetType().Name}] {message}\n</color>", context);
}