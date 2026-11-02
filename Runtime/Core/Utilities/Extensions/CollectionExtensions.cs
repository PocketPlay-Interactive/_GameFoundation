using System;
using System.Collections.Generic;
using UnityEngine;

public static class CollectionExtensions
{
    /// <summary>
    /// Pick a random element from the provided params-array.
    /// </summary>
    public static int GetRandomChoice(params int[] values)
    {
        if (values == null || values.Length == 0) return 0;
        if (values.Length == 1) return values[0];
        int idx = UnityEngine.Random.Range(0, values.Length);
        return values[idx];
    }

    // --- Array helpers (kept API, reduced allocations where sensible) ---
    public static T[] Append<T>(this T[] array, T item)
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

    public static T[] AddIfMissing<T>(this T[] target, T item)
    {
        if (target == null) return null;
        for (int i = 0; i < target.Length; i++)
            if (AreEqual(target[i], item)) return target;

        var len = target.Length;
        T[] result = new T[len + 1];
        Array.Copy(target, result, len);
        result[len] = item;
        return result;
    }

    public static bool AreEqual<T>(T x, T y) => EqualityComparer<T>.Default.Equals(x, y);

    public static bool ContainsItem<T>(this T[] target, T item)
    {
        if (target == null) return false;

        for (int i = 0; i < target.Length; i++)
            if (AreEqual(target[i], item)) return true;
        return false;
    }

    public static T GetRandomElement<T>(this T[] target)
    {
        if (target == null || target.Length == 0) return default;

        int i = UnityEngine.Random.Range(0, target.Length);
        return target[i];
    }

    // Extension cho List: GetRandomElement, RemoveNullItems
    public static T GetRandomElement<T>(this IList<T> list)
        => list == null || list.Count == 0 ? default : list[UnityEngine.Random.Range(0, list.Count)];
    public static void RemoveNullItems<T>(this List<T> list) where T : class
    {
        if (list != null) list.RemoveAll(item => item == null);
    }

    public static T[] MoveFirstToLast<T>(this T[] arr)
    {
        if (arr == null || arr.Length == 0) return null;
        T first = arr[0];
        for (int i = 1; i < arr.Length; i++) arr[i - 1] = arr[i];
        arr[^1] = first;
        return arr;
    }

    public static T[] MoveLastToFirst<T>(this T[] arr)
    {
        if (arr == null || arr.Length == 0) return null;
        T last = arr[^1];
        for (int i = arr.Length - 1; i > 0; i--) arr[i] = arr[i - 1];
        arr[0] = last;
        return arr;
    }
}
