using UnityEngine;

#if UNITY_EDITOR
using System.Reflection;
#endif

public static class PlatformUtils
{
    /// <summary>
    /// Clear Unity Console (Editor only).
    /// </summary>
    public static void ClearLog()
    {
#if UNITY_EDITOR
        var assembly = Assembly.GetAssembly(typeof(UnityEditor.Editor));
        var type = assembly.GetType("UnityEditor.LogEntries");
        var method = type.GetMethod("Clear");
        method?.Invoke(new object(), null);
#endif
    }

    /// <summary>
    /// Open URL across platforms. (Note: ExternalEval is obsolete; OpenURL is used on all targets.)
    /// </summary>
    public static void OpenURL(string url)
    {
        if (string.IsNullOrEmpty(url)) return;

        Application.OpenURL(url);
    }
}
