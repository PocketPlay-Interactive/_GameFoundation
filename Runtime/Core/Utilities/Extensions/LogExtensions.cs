using UnityEngine;

public static class LogExtensions
{
    public static void Log<T>(this T _t) { Debug.Log(_t); }

    public static void LogArray<T>(this T[] arr)
    {
        if (arr == null) return;
        arr.SimpleForEach(a => Debug.Log(a));
    }

    public static void LogArrayWithPrefix<T>(this T[] arr, string type)
    {
        if (arr == null) return;
        arr.SimpleForEach(a => Debug.Log($"[{type}] {a}"));
    }
}
