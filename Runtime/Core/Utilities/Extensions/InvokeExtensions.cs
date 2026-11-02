using System;
using UnityEngine.Events;

public static class InvokeExtensions
{
    /// <summary>
    /// Simple "try/catch" promise style invoker.
    /// </summary>
    public static void TryInvoke(UnityAction success, UnityAction error)
    {
        try { success?.Invoke(); }
        catch { error?.Invoke(); }
    }

    // SafeInvoke cho event/delegate
    public static void SafeInvoke(this Action action) { if (action != null) action(); }
    public static void SafeInvoke<T>(this Action<T> action, T arg) { if (action != null) action(arg); }
}
