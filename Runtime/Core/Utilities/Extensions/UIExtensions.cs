using UnityEngine;

public static class UIExtensions
{
    /// <summary>
    /// Safe SetPositions for LineRenderer (no extra allocs).
    /// </summary>
    public static void SetPositionsSafe(this LineRenderer lineRenderer, params Vector3[] positions)
    {
        if (lineRenderer == null || positions == null) return;
        if (lineRenderer.positionCount != positions.Length)
            lineRenderer.positionCount = positions.Length;
        lineRenderer.SetPositions(positions);
    }

    // Extension cho RectTransform: SetAnchor, SetPivot, SetSize
    public static void SetAnchor(this RectTransform rt, Vector2 min, Vector2 max)
    {
        if (rt == null) return;

        rt.anchorMin = min;
        rt.anchorMax = max;
    }
    public static void SetPivot(this RectTransform rt, Vector2 pivot)
    {
        if (rt != null) rt.pivot = pivot;
    }
    public static void SetSize(this RectTransform rt, Vector2 size)
    {
        if (rt == null) return;

        var oldSize = rt.rect.size;
        var deltaSize = size - oldSize;
        rt.offsetMin -= new Vector2(deltaSize.x * rt.pivot.x, deltaSize.y * rt.pivot.y);
        rt.offsetMax += new Vector2(deltaSize.x * (1 - rt.pivot.x), deltaSize.y * (1 - rt.pivot.y));
    }
}
