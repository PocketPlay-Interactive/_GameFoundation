using System;
using System.Collections.Generic;
using UnityEngine;

public static class CollectionExtensions
{
    /// <summary>Pick a random element from the provided params-array.</summary>
    public static int GetRandomChoice(params int[] values)
    {
        if (values == null || values.Length == 0) return 0;
        return values.Length == 1 ? values[0] : values[UnityEngine.Random.Range(0, values.Length)];
    }

    // ---- Array helpers: return a NEW array (the input is left unchanged), or null for a null input --------

    public static T[] Append<T>(this T[] array, T item)
    {
        if (array == null) return null;

        var result = new T[array.Length + 1];
        Array.Copy(array, result, array.Length);
        result[array.Length] = item;
        return result;
    }

    public static T[] AddRange<T>(this T[] target, T[] items)
    {
        if (target == null) return null;
        if (items == null || items.Length == 0) return target;

        var result = new T[target.Length + items.Length];
        Array.Copy(target, 0, result, 0, target.Length);
        Array.Copy(items, 0, result, target.Length, items.Length);
        return result;
    }

    /// <summary>Returns the same array when the item is already present, otherwise a new array with it appended.</summary>
    public static T[] AddIfMissing<T>(this T[] target, T item)
    {
        if (target == null) return null;
        return target.ContainsItem(item) ? target : target.Append(item);
    }

    public static bool AreEqual<T>(T x, T y) => EqualityComparer<T>.Default.Equals(x, y);

    public static bool ContainsItem<T>(this T[] target, T item) => target != null && Array.IndexOf(target, item) >= 0;

    // ---- Random / cleanup (IList covers arrays and lists) ---------------------------------------------

    public static T GetRandomElement<T>(this IList<T> list) =>
        list == null || list.Count == 0 ? default : list[UnityEngine.Random.Range(0, list.Count)];

    public static T GetRandomElement<T>(this T[] target) => ((IList<T>)target).GetRandomElement();

    public static void RemoveNullItems<T>(this List<T> list) where T : class
    {
        list?.RemoveAll(item => item == null);
    }

    // ---- In-place rotation: mutate and return the same array, null for a null or empty array --------------

    public static T[] MoveFirstToLast<T>(this T[] arr)
    {
        if (arr == null || arr.Length == 0) return null;

        T first = arr[0];
        Array.Copy(arr, 1, arr, 0, arr.Length - 1);
        arr[arr.Length - 1] = first;
        return arr;
    }

    public static T[] MoveLastToFirst<T>(this T[] arr)
    {
        if (arr == null || arr.Length == 0) return null;

        T last = arr[arr.Length - 1];
        Array.Copy(arr, 0, arr, 1, arr.Length - 1);
        arr[0] = last;
        return arr;
    }
}
