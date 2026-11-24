using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public static class LinqExtensions
{
    // ---- Dictionary ----------------------------------------------------------------------------

    public static void AddOrUpdate<T, TJ>(this IDictionary<T, TJ> dict, T key, TJ val) => dict[key] = val;

    public static TJ GetOrDefault<T, TJ>(this IDictionary<T, TJ> dict, T key) =>
        dict != null && dict.TryGetValue(key, out var value) ? value : default;

    // ---- Random picking ------------------------------------------------------------------------

    public static T GetRandom<T>(this IEnumerable<T> enumerable, out int index)
    {
        var list = enumerable as IList<T> ?? enumerable?.ToList();
        if (list == null || list.Count == 0)
        {
            index = -1;
            return default;
        }

        index = Random.Range(0, list.Count);
        return list[index];
    }

    public static T GetRandom<T>(this IEnumerable<T> enumerable) => enumerable.GetRandom(out _);

    // Same as GetRandom(): both return default for an empty or null sequence.
    [Obsolete("Use GetRandom().")]
    public static T GetRandomOrDefault<T>(this IEnumerable<T> enumerable) => enumerable.GetRandom(out _);

    /// <summary>
    /// Pick <paramref name="count"/> distinct elements. Throws immediately (not while iterating) when the
    /// sequence has fewer elements than requested.
    /// </summary>
    public static IEnumerable<T> GetRandom<T>(this IEnumerable<T> enumerable, int count)
    {
        if (enumerable == null || count <= 0) return Enumerable.Empty<T>();

        var list = enumerable.ToList();
        if (list.Count < count)
            throw new InvalidOperationException($"Cannot pick {count} elements from a sequence of {list.Count}.");

        var result = new List<T>(count);
        for (int i = 0; i < count; i++)
        {
            int index = Random.Range(0, list.Count);
            result.Add(list[index]);
            list.RemoveAt(index);
        }
        return result;
    }

    /// <summary>Pick an element where earlier items are more likely: item i has weight factor^i.</summary>
    public static T GetRandomWithReduceFactor<T>(this IEnumerable<T> enumerable, float factor)
    {
        var list = enumerable as IList<T> ?? enumerable?.ToList();
        if (list == null || list.Count == 0) return default;

        factor = Mathf.Max(0f, factor);

        float total = 0f, weight = 1f;
        for (int i = 0; i < list.Count; i++)
        {
            total += weight;
            weight *= factor;
        }

        float p = Random.Range(0f, total);
        weight = 1f;
        for (int i = 0; i < list.Count; i++)
        {
            p -= weight;
            if (p <= 0f) return list[i];
            weight *= factor;
        }

        return list[list.Count - 1];
    }

    // ---- List / array edits --------------------------------------------------------------------

    public static T GetAndRemove<T>(this IList<T> list, T item) => list == null ? default : list.GetAndRemove(list.IndexOf(item));

    public static T GetAndRemove<T>(this IList<T> list, int index)
    {
        if (list == null || index < 0 || index >= list.Count) return default;

        var item = list[index];
        list.RemoveAt(index);
        return item;
    }

    public static void Shuffle<T>(this IList<T> list)
    {
        if (list == null) return;

        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }

    // ---- Iteration -----------------------------------------------------------------------------

    public static void ForEach<T>(this IEnumerable<T> enumerable, Action<T> action)
    {
        if (enumerable == null || action == null) return;

        foreach (var item in enumerable)
            action(item);
    }

    public static void ForEach<T>(this IEnumerable<T> enumerable, Action<T, int> action)
    {
        if (enumerable == null || action == null) return;

        int i = 0;
        foreach (var item in enumerable)
            action(item, i++);
    }

    // Array overload kept for compatibility: unlike the IEnumerable version it SKIPS null elements.
    public static void ForEach<T>(this T[] _array, Action<T> action)
    {
        if (_array == null || action == null) return;

        for (int i = 0; i < _array.Length; i++)
        {
            if (_array[i] == null) continue;
            action(_array[i]);
        }
    }

    [Obsolete("Use ForEach on the array; null elements are skipped.")]
    public static void SimpleForEach<T>(this T[] _array, Action<T> action) => _array.ForEach(action);

    [Obsolete("Use ForEach.")]
    public static void SimpleForEach<T>(this List<T> _array, Action<T> action) => ((IEnumerable<T>)_array).ForEach(action);

    [Obsolete("Use ForEach.")]
    public static void SimpleForEach<T>(this T[] _array, Action<T, int> action) => ((IEnumerable<T>)_array).ForEach(action);

    [Obsolete("Use ForEach.")]
    public static void SimpleForEach<T>(this List<T> _array, Action<T, int> action) => ((IEnumerable<T>)_array).ForEach(action);

    public static void ForEachReverse<T>(this IList<T> list, Action<T> action)
    {
        if (list == null || action == null) return;

        for (int i = list.Count - 1; i >= 0; i--)
            action(list[i]);
    }

    // Arrays implement IList<T>; these keep the old array/List signatures binary-compatible.
    public static void ForEachReverse<T>(this T[] array, Action<T> action) => ((IList<T>)array).ForEachReverse(action);

    public static void ForEachReverse<T>(this List<T> list, Action<T> action) => ((IList<T>)list).ForEachReverse(action);

    // ---- Transform children --------------------------------------------------------------------

    public static int ChildCount(this GameObject _parent) => _parent != null ? _parent.transform.childCount : 0;

    public static void ForChild(this Transform _parent, Action<Transform> action)
    {
        if (_parent == null || action == null) return;

        for (int i = 0; i < _parent.childCount; i++)
            action(_parent.GetChild(i));
    }

    public static void ForChild(this Transform _parent, Action<int, Transform> action)
    {
        if (_parent == null || action == null) return;

        for (int i = 0; i < _parent.childCount; i++)
            action(i, _parent.GetChild(i));
    }

    public static void ForChild(this GameObject _parent, Action<Transform> action)
    {
        if (_parent != null) _parent.transform.ForChild(action);
    }

    public static void ForChildReverse(this Transform parent, Action<Transform> action)
    {
        if (parent == null || action == null) return;

        for (int i = parent.childCount - 1; i >= 0; i--)
            action(parent.GetChild(i));
    }

    public static void ForChildReverse(this Transform parent, Action<int, Transform> action)
    {
        if (parent == null || action == null) return;

        for (int i = parent.childCount - 1; i >= 0; i--)
            action(i, parent.GetChild(i));
    }

    public static Transform[] GetChildren(this Transform parent)
    {
        if (parent == null) return Array.Empty<Transform>();

        var arr = new Transform[parent.childCount];
        for (int i = 0; i < arr.Length; i++)
            arr[i] = parent.GetChild(i);
        return arr;
    }

    [Obsolete("Use GetChildren.")]
    public static Transform[] GetChilds(this Transform parent) => parent.GetChildren();

    [Obsolete("Use GetChildren.")]
    public static Transform[] GetChilds(this GameObject parent) => parent == null ? Array.Empty<Transform>() : parent.transform.GetChildren();

    // A fresh list per call (the previous shared static buffer was not safe for nested or threaded calls).
    public static List<Transform> GetRandomChildren(this Transform parent, int count)
    {
        if (parent == null || count <= 0) return new List<Transform>();

        var children = new List<Transform>(parent.GetChildren());
        children.Shuffle();
        return children.GetRange(0, Mathf.Min(count, children.Count));
    }

    public static List<Transform> GetRandomChildren(this GameObject parent, int count) =>
        parent == null ? new List<Transform>() : parent.transform.GetRandomChildren(count);

    /// <summary>
    /// Find the first child whose active state equals <paramref name="_isActive"/> and flip it
    /// (an active one is hidden, an inactive one is shown). Returns it, or null.
    /// </summary>
    public static Transform FindOnce(this List<Transform> _array, bool _isActive)
    {
        if (_array == null) return null;

        for (int i = 0; i < _array.Count; i++)
        {
            var item = _array[i];
            if (item == null || item.IsActive() != _isActive) continue;

            item.SetActiveSafe(!_isActive);
            return item;
        }
        return null;
    }
}
