using UnityEngine;

public static class ActiveStateExtensions
{
    // Core: every Show/Hide/SetActiveSafe overload ends here. With isConcurrent the change is queued to the main
    // thread (needs a Concurrency object in the scene); a destroyed target is skipped when the queue runs.
    public static void SetActiveSafe(this GameObject obj, bool value, bool isConcurrent = false)
    {
        if (obj == null)
        {
            Debug.LogWarning("SetActiveSafe() called on a null GameObject.");
            return;
        }

        if (isConcurrent)
            Concurrency.Instance().Enqueue(() => { if (obj != null) obj.SetActive(value); });
        else
            obj.SetActive(value);
    }

    public static void Show(this GameObject obj, bool isConcurrent = false)
    {
        if (obj == null) { Debug.LogWarning("Show() called on a null GameObject."); return; }
        obj.SetActiveSafe(true, isConcurrent);
    }

    public static void Hide(this GameObject obj, bool isConcurrent = false)
    {
        if (obj == null) { Debug.LogWarning("Hide() called on a null GameObject."); return; }
        obj.SetActiveSafe(false, isConcurrent);
    }

    // Keep exact Transform overloads for source and binary compatibility.
    public static void SetActiveSafe(this Transform obj, bool value, bool isConcurrent = false)
    {
        if (obj == null) { Debug.LogWarning("SetActiveSafe() called on a null Transform."); return; }
        obj.gameObject.SetActiveSafe(value, isConcurrent);
    }

    public static void Show(this Transform obj, bool isConcurrent = false)
    {
        if (obj == null) { Debug.LogWarning("Show() called on a null Transform."); return; }
        obj.gameObject.Show(isConcurrent);
    }

    public static void Hide(this Transform obj, bool isConcurrent = false)
    {
        if (obj == null) { Debug.LogWarning("Hide() called on a null Transform."); return; }
        obj.gameObject.Hide(isConcurrent);
    }

    public static void SetActiveSafe(this Component obj, bool value, bool isConcurrent = false)
    {
        if (obj == null) { Debug.LogWarning("SetActiveSafe() called on a null Component."); return; }
        obj.gameObject.SetActiveSafe(value, isConcurrent);
    }

    public static void Show(this Component obj, bool isConcurrent = false)
    {
        if (obj == null) { Debug.LogWarning("Show() called on a null Component."); return; }
        obj.gameObject.Show(isConcurrent);
    }

    public static void Hide(this Component obj, bool isConcurrent = false)
    {
        if (obj == null) { Debug.LogWarning("Hide() called on a null Component."); return; }
        obj.gameObject.Hide(isConcurrent);
    }

    // True when the object exists and is active in the hierarchy.
    public static bool IsActive(this GameObject target) => target != null && target.activeInHierarchy;
    public static bool IsActive(this Transform target) => target != null && target.gameObject.activeInHierarchy;
    public static bool IsActive(this Component target) => target != null && target.gameObject.activeInHierarchy;

    // Unity overloads == so a destroyed object compares equal to null.
    public static bool IsNullOrDestroyed(this UnityEngine.Object obj) => obj == null;
}
