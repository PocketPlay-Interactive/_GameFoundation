using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using UnityEngine;
using UnityEngine.Events;

#if UNITY_EDITOR
using System.Reflection;
#endif

public static class StaticVariable
{
    public static bool isLoaded = false;

    // Persistent keys
    public static readonly string DATA_PLAYER = "player.data";
    public static readonly string DATA_SETTING = "setting.data";

    // Reusable buffer to reduce GC when collecting children
    private static readonly List<Transform> childrenBuffer = new List<Transform>(32);

    // Suffix map for number formatting (K, M, B, ...)
    private static readonly string[] suffixes = { "", "K", "M", "B", "T", "Q", "S", "O", "N", "D" };

    /// <summary>
    /// Simple "try/catch" promise style invoker.
    /// </summary>
    public static void PromiseFunction(UnityAction success, UnityAction error)
    {
        try { success?.Invoke(); }
        catch { error?.Invoke(); }
    }

    /// <summary>
    /// Format very large BigInteger into short form (e.g., 1.23K, 4.56M).
    /// Returns "∞" if exceeds suffix table.
    /// </summary>
    public static string FormatBigNumber(this System.Numerics.BigInteger number)
    {
        if (number < 0) return "-" + FormatBigNumber(System.Numerics.BigInteger.Negate(number));
        if (number < 1000) return number.ToString();

        int index = (int)Math.Floor(System.Numerics.BigInteger.Log(number, 1000));
        if (index >= suffixes.Length) return "∞";
        //Debug.Log(index);

        double scaled = (double)number / Math.Pow(1000, index);
        return scaled.ToString("F2", CultureInfo.InvariantCulture) + suffixes[index];
    }

    /// <summary>
    /// Format int into short form (e.g., 12.3K). Negative-safe.
    /// </summary>
    public static string FormatNumber(this int number)
    {
        if (number < 0) return "-" + FormatNumber(-number);
        if (number < 1000) return number.ToString();

        // index = floor(log10(n)/3)
        int index = (int)Math.Floor(Math.Log10(number) / 3.0);
        if (index <= 0) return number.ToString();
        if (index >= suffixes.Length) return "∞";
        //Debug.Log(index);

        double scaled = number / Math.Pow(1000, index);
        return scaled.ToString("F1", CultureInfo.InvariantCulture) + suffixes[index];
    }

    /// <summary>
    /// Format numeric string using BigInteger (handles very large numbers).
    /// </summary>
    public static string FormatNumber(this string numberString)
    {
        if (!System.Numerics.BigInteger.TryParse(numberString, out var number))
            return numberString;

        if (number < 0) return "-" + FormatNumber(number);
        if (number < 1000) return number.ToString();

        int index = (int)Math.Floor(System.Numerics.BigInteger.Log(number, 1000));
        if (index >= suffixes.Length) return "∞";
        //Debug.Log(index);

        double scaled = (double)number / Math.Pow(1000, index);
        return scaled.ToString("F1", CultureInfo.InvariantCulture) + suffixes[index];
    }

    /// <summary>
    /// Format numeric string using BigInteger (handles very large numbers).
    /// </summary>
    public static string FormatNumber(this System.Numerics.BigInteger number)
    {
        if (number < 1000)
            return number.ToString();

        int index = (int)Math.Floor(System.Numerics.BigInteger.Log(number, 1000));
        if (index >= suffixes.Length)
            return "∞";
        //Debug.Log(index);

        double scaled = (double)number / Math.Pow(1000, index);
        return scaled.ToString("F1", System.Globalization.CultureInfo.InvariantCulture) + suffixes[index];
    }

    public static Quaternion ToRotation(this Vector3 eulerAngles) => Quaternion.Euler(eulerAngles);
    public static Vector3 ToEulerAngles(this Quaternion quaternion) => quaternion.eulerAngles;

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
    public static void ConcurrentSetActive(this GameObject obj, bool value, bool isConcurrent = false) 
    { 
        if (obj == null)
        {
            Debug.LogWarning("ConcurrentSetActive() called on a null GameObject.");
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
    public static void ConcurrentSetActive(this Transform obj, bool value, bool isConcurrent = false) 
    { 
        if (obj == null)
        {
            Debug.LogWarning("ConcurrentSetActive() called on a null Transform.");
            return;
        }
        if (isConcurrent)
            Concurrency.Instance().Enqueue(() => { if (obj != null) obj.gameObject.SetActive(value); });
        else
            obj.gameObject.SetActive(value);
    }

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
    /// Pick a random element from the provided params-array.
    /// </summary>
    public static int RandomChoice(params int[] values)
    {
        if (values == null || values.Length == 0) return 0;
        if (values.Length == 1) return values[0];
        int idx = UnityEngine.Random.Range(0, values.Length);
        return values[idx];
    }

    /// <summary>
    /// In-place Fisher-Yates shuffle.
    /// </summary>
    public static void Shuffle<T>(this IList<T> ts)
    {
        int count = ts.Count;
        for (int i = 0; i < count - 1; ++i)
        {
            int r = UnityEngine.Random.Range(i, count);
            (ts[i], ts[r]) = (ts[r], ts[i]);
        }
    }

    // --- Array helpers (kept API, reduced allocations where sensible) ---
    public static T[] AddElement<T>(this T[] array, T item)
    {
        if (array == null) return null;
        var len = array.Length;
        T[] result = new T[len + 1];
        Array.Copy(array, result, len);
        result[len] = item;
        return result;
    }

    public static T[] AddRange<T>(this T[] target, T[] items)
    {
        if (target == null) return null;
        if (items == null || items.Length == 0) return target;

        int oldLen = target.Length;
        int newLen = oldLen + items.Length;
        T[] result = new T[newLen];
        Array.Copy(target, 0, result, 0, oldLen);
        Array.Copy(items, 0, result, oldLen, items.Length);
        return result;
    }

    public static T[] OnlyAdd<T>(this T[] target, T item)
    {
        if (target == null) return null;
        for (int i = 0; i < target.Length; i++)
            if (Compare(target[i], item)) return target;

        var len = target.Length;
        T[] result = new T[len + 1];
        Array.Copy(target, result, len);
        result[len] = item;
        return result;
    }

    public static bool Compare<T>(T x, T y) => EqualityComparer<T>.Default.Equals(x, y);

    public static bool IsFound<T>(this T[] target, T item)
    {
        for (int i = 0; i < target.Length; i++)
            if (Compare(target[i], item)) return true;
        return false;
    }

    public static bool IsActive(this Transform target) => target != null && target.gameObject.activeInHierarchy;
    public static bool IsActive(this GameObject target) => target != null && target.activeInHierarchy;

    public static T GetOnce<T>(this T[] target)
    {
        int i = UnityEngine.Random.Range(0, target.Length);
        return target[i];
    }

    /// <summary>
    /// Find nearest GameObject from a list (null-safe list items).
    /// </summary>
    public static GameObject FindNearest(this Transform target, List<GameObject> lstObject)
    {
        if (lstObject == null || lstObject.Count == 0) return null;

        GameObject nearest = null;
        float shortest = Mathf.Infinity;
        Vector3 pos = target.position;

        for (int i = 0; i < lstObject.Count; i++)
        {
            var go = lstObject[i];
            if (go == null) continue;

            float dist = Vector3.Distance(pos, go.transform.position);
            if (dist < shortest)
            {
                shortest = dist;
                nearest = go;
            }
        }
        return nearest;
    }

    public static GameObject FindNearest(this GameObject target, List<GameObject> lstObject)
    {
        if (lstObject == null || lstObject.Count == 0) return null;

        GameObject nearest = null;
        float shortest = Mathf.Infinity;
        Vector3 pos = target.transform.position;

        for (int i = 0; i < lstObject.Count; i++)
        {
            var go = lstObject[i];
            if (go == null) continue;

            float dist = Vector3.Distance(pos, go.transform.position);
            if (dist < shortest)
            {
                shortest = dist;
                nearest = go;
            }
        }
        return nearest;
    }

    // --- Recursive & direct child finders ---
    public static Transform FindChildRecursive(this Transform parent, string name)
    {
        if (parent == null) return null;
        var result = parent.Find(name);
        if (result != null) return result;

        foreach (Transform child in parent)
        {
            result = child.FindChildRecursive(name);
            if (result != null) return result;
        }
        return null;
    }

    public static T FindChildByRecursion<T>(this Transform aParent)
    {
        if (aParent == null) return default;
        var result = aParent.GetComponent<T>();
        if (!Equals(result, default(T))) return result;

        foreach (Transform child in aParent)
        {
            result = child.FindChildByRecursion<T>();
            if (!Equals(result, default(T))) return result;
        }
        return default;
    }

    public static Transform FindChildByParent(this Transform aParent, string aName)
    {
        if (aParent == null) return null;
        for (int i = 0; i < aParent.childCount; i++)
        {
            var child = aParent.GetChild(i);
            if (child.name == aName) return child;
        }
        return null;
    }

    public static Transform[] FindChildsByParent(this Transform aParent, string aName)
    {
        childrenBuffer.Clear();
        if (aParent == null) return null;

        for (int i = 0; i < aParent.childCount; i++)
        {
            var child = aParent.GetChild(i);
            if (child.name == aName) childrenBuffer.Add(child);
        }
        return childrenBuffer.ToArray();
    }

    public static Transform FindChildByParent(this GameObject aParent, string aName)
    {
        if (aParent == null) return null;
        var t = aParent.transform;
        for (int i = 0; i < t.childCount; i++)
        {
            var child = t.GetChild(i);
            if (child.name == aName) return child;
        }
        return null;
    }

    public static Transform[] FindChildsByParent(this GameObject aParent, string aName)
    {
        childrenBuffer.Clear();
        if (aParent == null) return null;

        var t = aParent.transform;
        for (int i = 0; i < t.childCount; i++)
        {
            var child = t.GetChild(i);
            if (child.name == aName) childrenBuffer.Add(child);
        }
        return childrenBuffer.ToArray();
    }

    public static Transform Find(this GameObject aParent, int index)
    {
        if (aParent == null) return null;
        var t = aParent.transform;
        if (index < 0 || index >= t.childCount) return null;
        return t.GetChild(index);
    }

    /// <summary>
    /// Safe Enum.TryParse with default fallback.
    /// </summary>
    public static T ToEnum<T>(this string value, T defaultValue) where T : struct
    {
        if (string.IsNullOrEmpty(value)) return defaultValue;
        return Enum.TryParse<T>(value, true, out var result) ? result : defaultValue;
    }

    public static T[] SetToLast<T>(this T[] arr)
    {
        if (arr == null || arr.Length == 0) return null;
        T first = arr[0];
        for (int i = 1; i < arr.Length; i++) arr[i - 1] = arr[i];
        arr[^1] = first;
        return arr;
    }

    public static T[] SetToFirst<T>(this T[] arr)
    {
        if (arr == null || arr.Length == 0) return null;
        T last = arr[^1];
        for (int i = arr.Length - 1; i > 0; i--) arr[i] = arr[i - 1];
        arr[0] = last;
        return arr;
    }

    public static Transform Find(this Transform[] arr, string name)
    {
        for (int i = 0; i < arr.Length; i++)
            if (arr[i].name == name) return arr[i];
        return null;
    }

    public static float Round(this float original)
    {
        return Mathf.Round(original * 100f) / 100f;
    }

    /// <summary>
    /// Convert sign of float to -1/0/1.
    /// </summary>
    public static int ConvertAndRount(this float original)
    {
        if (original < 0) return -1;
        if (original > 0) return 1;
        return 0;
    }

    public static void Log<T>(this T _t) { Debug.Log(_t); }

    public static void LogArray<T>(this T[] arr)
    {
        arr.SimpleForEach(a => Debug.Log(a));
    }

    public static void Log<T>(this T[] arr, string type)
    {
        arr.SimpleForEach(a => Debug.Log($"[{type}] {a}"));
    }

    public static bool IsChild(this Transform aParent, string aName)
    {
        if (aParent == null) return false;
        for (int i = 0; i < aParent.childCount; i++)
            if (aParent.GetChild(i).name == aName) return true;
        return false;
    }

    public static Transform GetChild(this GameObject _object, int index)
    {
        return _object.transform.GetChild(index);
    }

    // Reusable string builder to reduce allocations for AddSpaceBeforeUppercase
    private static readonly StringBuilder output = new StringBuilder(64);

    /// <summary>
    /// Insert spaces before uppercase letters: "NguyenSiHiep" -> "Nguyen Si Hiep".
    /// </summary>
    public static string AddSpaceBeforeUppercase(this string input)
    {
        output.Clear();
        for (int i = 0; i < input.Length; i++)
        {
            if (i > 0 && char.IsUpper(input[i])) output.Append(' ');
            output.Append(input[i]);
        }
        return output.ToString();
    }

    /// <summary>
    /// Move along a circle by arc length "distance".
    /// </summary>
    public static Vector2 travelAlongCircle(this Vector2 pos, Vector2 center, float distance)
    {
        Vector3 axis = Vector3.back;
        Vector2 dir = pos - center;
        float circumference = 2f * Mathf.PI * dir.magnitude;
        float angle = distance / Mathf.Max(circumference, 1e-6f) * 360f;
        dir = Quaternion.AngleAxis(angle, axis) * dir;
        return dir + center;
    }

    /// <summary>
    /// Position on circumference in XZ plane at "degrees".
    /// </summary>
    public static Vector3 PositionInCircumference(Vector3 center, float radius, float degrees)
    {
        float radians = degrees * Mathf.Deg2Rad;
        float x = Mathf.Cos(radians);
        float z = Mathf.Sin(radians);
        Vector3 position = new Vector3(x, center.y, z) * radius + center;
        return position;
    }

    /// <summary>
    /// Random point on a discrete circle ring (integer X/Y). Uses Random.Range properly.
    /// </summary>
    public static Vector3 GetCirclePosition(double radius, Vector3 center)
    {
        int point = UnityEngine.Random.Range(5, 10);
        double slice = 2 * Math.PI / point;
        double angle = slice * UnityEngine.Random.Range(0, point);
        int newX = (int)(center.x + radius * Math.Cos(angle));
        int newY = (int)(center.y + radius * Math.Sin(angle));
        return new Vector3(newX, newY);
    }

    /// <summary>
    /// Random unit direction in XZ plane.
    /// </summary>
    public static Vector3 GetRandomDirection()
    {
        return Quaternion.AngleAxis(UnityEngine.Random.Range(0f, 360f), Vector3.up) * Vector3.forward;
    }

    private static readonly string[] Months = { "Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec" };
    public static string ConvertMonthIntToString(int Month) => Months[Month - 1];
    public static DayOfWeek ConvertDateTimeToDayOfWeek(DateTime dateTime) => dateTime.DayOfWeek;
    public static DayOfWeek ConvertDateTimeToDayOfWeek(int year, int month, int day) => new DateTime(year, month, day).DayOfWeek;
    public static string ConvertMonthIntToString(string Month) => ConvertMonthIntToString(int.Parse(Month));

    /// <summary>
    /// Safe SetPositions for LineRenderer (no extra allocs).
    /// </summary>
    public static void SetPostions(this LineRenderer lineRenderer, params Vector3[] postions)
    {
        if (lineRenderer == null || postions == null) return;
        if (lineRenderer.positionCount != postions.Length)
            lineRenderer.positionCount = postions.Length;
        lineRenderer.SetPositions(postions);
    }

    /// <summary>
    /// Open URL across platforms. (Note: ExternalEval is obsolete; OpenURL is used on all targets.)
    /// </summary>
    public static void OpenUrl(string url)
    {
#if UNITY_EDITOR
        Application.OpenURL(url);
#elif UNITY_WEBGL
        Application.OpenURL(url);
#else
        Application.OpenURL(url);
#endif
    }

    // Remap giá trị từ [a,b] sang [c,d]
    public static float Remap(this float value, float from1, float to1, float from2, float to2)
        => (value - from1) / (to1 - from1) * (to2 - from2) + from2;

    // Lerp 2 giá trị
    public static float Lerp(this float a, float b, float t) => Mathf.Lerp(a, b, t);

    // Kiểm tra null hoặc đã bị destroy (cho GameObject/Component)
    public static bool IsNullOrDestroyed(this UnityEngine.Object obj) => obj == null;

    // SafeInvoke cho event/delegate
    public static void SafeInvoke(this Action action) { if (action != null) action(); }
    public static void SafeInvoke<T>(this Action<T> action, T arg) { if (action != null) action(arg); }

    // Extension cho RectTransform: SetAnchor, SetPivot, SetSize
    public static void SetAnchor(this RectTransform rt, Vector2 min, Vector2 max)
    {
        rt.anchorMin = min;
        rt.anchorMax = max;
    }
    public static void SetPivot(this RectTransform rt, Vector2 pivot) => rt.pivot = pivot;
    public static void SetSize(this RectTransform rt, Vector2 size)
    {
        var oldSize = rt.rect.size;
        var deltaSize = size - oldSize;
        rt.offsetMin -= new Vector2(deltaSize.x * rt.pivot.x, deltaSize.y * rt.pivot.y);
        rt.offsetMax += new Vector2(deltaSize.x * (1 - rt.pivot.x), deltaSize.y * (1 - rt.pivot.y));
    }

    // Extension cho List: RandomElement, RemoveNulls
    public static T RandomElement<T>(this IList<T> list)
        => list.Count == 0 ? default : list[UnityEngine.Random.Range(0, list.Count)];
    public static void RemoveNulls<T>(this List<T> list) where T : class
        => list.RemoveAll(item => item == null);

    // Extension cho string: ToTitleCase
    public static string ToTitleCase(this string str)
        => CultureInfo.CurrentCulture.TextInfo.ToTitleCase(str.ToLower());

    public static string FormatTimeMMSS(this int totalSeconds)
    {
        if (totalSeconds < 0) totalSeconds = 0;
        int m = totalSeconds / 60;
        int s = totalSeconds % 60;
        return string.Format("{0:00}:{1:00}", m, s);
    }

    public static string FormatTimeMMSS(this float seconds)
    {
        if (float.IsNaN(seconds) || float.IsInfinity(seconds)) seconds = 0f;
        return FormatTimeMMSS(Mathf.FloorToInt(seconds));
    }

}
