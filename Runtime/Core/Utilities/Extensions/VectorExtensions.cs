using UnityEngine;

public static class VectorExtensions
{
    // Cache các mảng dùng cho tween, tránh new[] mỗi lần
    private static readonly Vector3[] path2 = new Vector3[2];
    private static readonly Vector3[] path3 = new Vector3[3];

    // Tạo vector nhanh
    // private static readonly Vector2 _cache2D = new Vector2(0, 0);
    private static readonly Vector3 _cache = new Vector3(0, 0, 0);
    public static Vector3 Create(float value) => _cache.WithX(value).WithY(value).WithZ(value);
    public static Vector2 Create2D(float x = 0, float y = 0) => _cache.WithX(x).WithY(y);
    public static Vector3 Create3D(float x = 0, float y = 0, float z = 0) => _cache.WithX(x).WithY(y).WithZ(z);

    // Extension cho Vector2/3: Set, Add, Clamp, Remap, Magnitude
    public static Vector2 WithX(this Vector2 v, float x) { v.x = x; return v; }
    public static Vector2 WithY(this Vector2 v, float y) { v.y = y; return v; }
    public static Vector2 AddX(this Vector2 v, float x) { v.x += x; return v; }
    public static Vector2 AddY(this Vector2 v, float y) { v.y += y; return v; }
    public static Vector3 WithX(this Vector3 v, float x) { v.x = x; return v; }
    public static Vector3 WithY(this Vector3 v, float y) { v.y = y; return v; }
    public static Vector3 WithZ(this Vector3 v, float z) { v.z = z; return v; }
    public static Vector3 AddX(this Vector3 v, float x) { v.x += x; return v; }
    public static Vector3 AddY(this Vector3 v, float y) { v.y += y; return v; }
    public static Vector3 AddZ(this Vector3 v, float z) { v.z += z; return v; }
    public static Vector2 To2D(this Vector3 v) => new Vector2(v.x, v.y);
    public static Vector3 To3D(this Vector2 v, float z = 0) => _cache.WithX(v.x).WithY(v.y).WithZ(z);
    public static bool IsZero(this Vector3 v) => v == Vector3.zero;
    public static bool IsZero(this Vector2 v) => v == Vector2.zero;
    public static bool IsNearlyEqual(this Vector3 v, Vector3 other, float epsilon = 0.001f) => (v - other).sqrMagnitude < epsilon * epsilon;
    public static float DistanceTo(this Vector3 v, Vector3 other) => Vector3.Distance(v, other);
    public static float DistanceTo(this Vector2 v, Vector2 other) => Vector2.Distance(v, other);
    public static Vector3 ClampMagnitude(this Vector3 v, float max) => Vector3.ClampMagnitude(v, max);
    public static Vector2 ClampMagnitude(this Vector2 v, float max) => Vector2.ClampMagnitude(v, max);
    public static Vector3 WithMagnitude(this Vector3 v, float mag) => v.normalized * mag;
    public static Vector2 WithMagnitude(this Vector2 v, float mag) => v.normalized * mag;
    private static readonly Vector3 _cacheRemap = new Vector3(0, 0);
    public static Vector3 Remap(this Vector3 v, float from1, float to1, float from2, float to2)
        => _cacheRemap.WithX(Mathf.Lerp(from2, to2, Mathf.InverseLerp(from1, to1, v.x)))
                 .WithY(Mathf.Lerp(from2, to2, Mathf.InverseLerp(from1, to1, v.y)))
                 .WithZ(Mathf.Lerp(from2, to2, Mathf.InverseLerp(from1, to1, v.z)));
    public static Vector2 Remap(this Vector2 v, float from1, float to1, float from2, float to2)
        => _cacheRemap.WithX(Mathf.Lerp(from2, to2, Mathf.InverseLerp(from1, to1, v.x)))
                 .WithY(Mathf.Lerp(from2, to2, Mathf.InverseLerp(from1, to1, v.y)));

    // Random tiện dụng
    public static float RandomWithin(this Vector2 v) => Random.Range(Mathf.Min(v.x, v.y), Mathf.Max(v.x, v.y));
    public static int RandomWithin(this Vector2Int v) => Random.Range(Mathf.Min(v.x, v.y), Mathf.Max(v.x, v.y));
    public static Vector3 RandomInsideUnitCircleXZ(float radius = 1f)
    {
        var v = Random.insideUnitCircle * radius;
        // Tránh new Vector3 mỗi lần, dùng static nếu cần spam
        return _cache.WithX(v.x).WithY(0).WithZ(v.y);
    }

    // Project, Rotate
    public static Vector3 ProjectOnPlane(this Vector3 v, Vector3 planeNormal)
        => Vector3.ProjectOnPlane(v, planeNormal);
    public static Vector3 RotateAroundY(this Vector3 v, float angle)
        => Quaternion.Euler(0, angle, 0) * v;

    // Clamp giá trị trong Vector2 như min/max
    public static float Clamp(this Vector2 v, float val) => Mathf.Clamp(val, Mathf.Min(v.x, v.y), Mathf.Max(v.x, v.y));
    public static float Lerp(this Vector2Int v, float t) => Mathf.Lerp(v.x, v.y, t);

    // Nearest point trên vector
    private static readonly Vector2[] nearestPoints = new Vector2[2];
    public static Vector2 GetNearestPointToVector(this Vector2 vec, Vector2 point, out float distance, out bool endNearest)
    {
        if (Vector2.Angle(vec, point) >= 90)
        {
            distance = point.magnitude;
            endNearest = true;
            nearestPoints[0] = Vector2.zero;
            return nearestPoints[0];
        }
        if (Vector2.Angle((point - vec), -vec) >= 90)
        {
            distance = (point - vec).magnitude;
            endNearest = true;
            nearestPoints[1] = vec;
            return nearestPoints[1];
        }
        endNearest = false;
        distance = Mathf.Sin(Vector2.Angle(vec, point) * Mathf.Deg2Rad) * point.magnitude;
        return point.magnitude * Mathf.Cos(Vector2.Angle(vec, point) * Mathf.Deg2Rad) * vec.normalized;
    }

    // Box/Circle distance
    public static float BoxDistance(Vector2 a, Vector2 b)
        => Mathf.Max(Mathf.Abs(a.x - b.x), Mathf.Abs(a.y - b.y));
    public static float CircleDistance(Vector2 a, Vector2 b)
        => Vector2.Distance(a, b);

    // Quaternion extensions
    public static Quaternion WithX(this Quaternion q, float x)
    {
        var e = q.eulerAngles; e.x = x; return Quaternion.Euler(e);
    }
    public static Quaternion WithY(this Quaternion q, float y)
    {
        var e = q.eulerAngles; e.y = y; return Quaternion.Euler(e);
    }
    public static Quaternion WithZ(this Quaternion q, float z)
    {
        var e = q.eulerAngles; e.z = z; return Quaternion.Euler(e);
    }
    public static Quaternion Add(this Quaternion q, Vector3 offset)
    {
        var e = q.eulerAngles;
        e.x += offset.x; e.y += offset.y; e.z += offset.z;
        return Quaternion.Euler(e);
    }
    public static Quaternion AddX(this Quaternion q, float x)
    {
        var e = q.eulerAngles; e.x += x; return Quaternion.Euler(e);
    }
    public static Quaternion AddY(this Quaternion q, float y)
    {
        var e = q.eulerAngles; e.y += y; return Quaternion.Euler(e);
    }
    public static Quaternion AddZ(this Quaternion q, float z)
    {
        var e = q.eulerAngles; e.z += z; return Quaternion.Euler(e);
    }

    // Các hàm dùng cho tween có thể dùng path2, path3 để tránh new mảng mỗi lần
    public static Vector3[] GetPath2(Vector3 a, Vector3 b)
    {
        path2[0] = a;
        path2[1] = b;
        return path2;
    }
    public static Vector3[] GetPath3(Vector3 a, Vector3 b, Vector3 c)
    {
        path3[0] = a;
        path3[1] = b;
        path3[2] = c;
        return path3;
    }
}

public static class PrimitiveExtensions
{
    public static int FloorTo(this int value, int digit)
    {
        var pow = (int)Mathf.Pow(10, digit);
        return (value / pow) * pow;
    }
    // Clamp tiện cho int
    public static int Clamp(this int value, int min, int max) => Mathf.Clamp(value, min, max);
}

public static class ColorExtensions
{
    public static Color WithAlpha(this Color color, float a)
    {
        color.a = a;
        return color;
    }
    public static string ToHex(this Color color)
        => ColorUtility.ToHtmlStringRGBA(color);

    /// <summary>
    /// Convert hex (e.g., "66331A") to Color. Returns white on failure.
    /// </summary>
    public static Color ToColor(this string hex)
    {
        if (!ColorUtility.TryParseHtmlString("#" + hex, out Color color))
        {
            Debug.LogError("Invalid hexadecimal color value: " + hex);
            color = Color.white;
        }
        return color;
    }
}

// --- Transform property shortcuts (kept naming for backward compatibility) ---
public static class TransformShortcuts
{
    public static Vector3 Position(this GameObject obj) => obj.transform.position;
    public static void Position(this GameObject obj, Vector3 pos) => obj.transform.position = pos;

    public static Quaternion Rotation(this GameObject obj) => obj.transform.rotation;
    public static void Rotation(this GameObject obj, Quaternion rot) => obj.transform.rotation = rot;
    public static Vector3 Scale(this GameObject obj) => obj.transform.localScale;
    public static void Scale(this GameObject obj, float s) => obj.transform.localScale = VectorExtensions.Create(s);
    public static void Scale(this GameObject obj, Vector3 s) => obj.transform.localScale = s;

    public static void Parent(this GameObject obj, Transform parent) => obj.transform.SetParent(parent);
    public static GameObject Parent(this GameObject obj) => obj.transform.parent != null ? obj.transform.parent.gameObject : null;
}