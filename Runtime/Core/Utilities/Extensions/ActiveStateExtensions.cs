using UnityEngine;

public static class ActiveStateExtensions
{
    // --- Safe active toggles via Concurrency (kept as original behavior) ---
    public static void Show(this GameObject obj, bool isConcurrent = false) 
    { 
        if (obj == null)
        {
            Debug.LogWarning("Show() called on a null GameObject.");
            return;
        }
        if (isConcurrent)
            Concurrency.Instance().Enqueue(() => { if (obj != null) obj.SetActive(true); });
        else
            obj.SetActive(true);
    }
    public static void Hide(this GameObject obj, bool isConcurrent = false) 
    { 
        if (obj == null)
        {
            Debug.LogWarning("Hide() called on a null GameObject.");
            return;
        }
        if (isConcurrent)
            Concurrency.Instance().Enqueue(() => { if (obj != null) obj.SetActive(false); });
        else
            obj.SetActive(false);
    }
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
    public static void Show(this Transform obj, bool isConcurrent = false) 
    { 
        if (obj == null)
        {
            Debug.LogWarning("Show() called on a null Transform.");
            return;
        }
        if (isConcurrent)
            Concurrency.Instance().Enqueue(() => { if (obj != null) obj.gameObject.SetActive(true); });
        else
            obj.gameObject.SetActive(true);
    }
    public static void Hide(this Transform obj, bool isConcurrent = false) 
    { 
        if (obj == null)
        {
            Debug.LogWarning("Hide() called on a null Transform.");
            return;
        }
        if (isConcurrent)
            Concurrency.Instance().Enqueue(() => { if (obj != null) obj.gameObject.SetActive(false); });
        else
            obj.gameObject.SetActive(false);
    }
    public static void SetActiveSafe(this Transform obj, bool value, bool isConcurrent = false) 
    { 
        if (obj == null)
        {
            Debug.LogWarning("SetActiveSafe() called on a null Transform.");
            return;
        }
        if (isConcurrent)
            Concurrency.Instance().Enqueue(() => { if (obj != null) obj.gameObject.SetActive(value); });
        else
            obj.gameObject.SetActive(value);
    }
    public static void Show(this Component obj, bool isConcurrent = false)
    {
        if (obj == null)
        {
            Debug.LogWarning("Show() called on a null Component.");
            return;
        }
        obj.gameObject.Show(isConcurrent);
    }
    public static void Hide(this Component obj, bool isConcurrent = false)
    {
        if (obj == null)
        {
            Debug.LogWarning("Hide() called on a null Component.");
            return;
        }
        obj.gameObject.Hide(isConcurrent);
    }
    public static void SetActiveSafe(this Component obj, bool value, bool isConcurrent = false)
    {
        if (obj == null)
        {
            Debug.LogWarning("SetActiveSafe() called on a null Component.");
            return;
        }
        obj.gameObject.SetActiveSafe(value, isConcurrent);
    }

    public static bool IsActive(this Transform target) => target != null && target.gameObject.activeInHierarchy;
    public static bool IsActive(this GameObject target) => target != null && target.activeInHierarchy;
    public static bool IsActive(this Component target) => target != null && target.gameObject.activeInHierarchy;

    // Kiểm tra null hoặc đã bị destroy (cho GameObject/Component)
    public static bool IsNullOrDestroyed(this UnityEngine.Object obj) => obj == null;
}
