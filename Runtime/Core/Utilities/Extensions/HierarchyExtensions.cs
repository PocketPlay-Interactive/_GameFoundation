using System;
using System.Collections.Generic;
using UnityEngine;

// Core logic lives on Transform; the GameObject and Component overloads only forward to it.
public static class HierarchyExtensions
{
    // ---- Nearest -------------------------------------------------------------------------------

    /// <summary>Find the nearest GameObject from a list (null items are skipped).</summary>
    public static GameObject FindNearest(this Transform target, List<GameObject> lstObject)
    {
        if (target == null || lstObject == null || lstObject.Count == 0) return null;

        GameObject nearest = null;
        float shortest = float.PositiveInfinity;
        Vector3 pos = target.position;

        for (int i = 0; i < lstObject.Count; i++)
        {
            var go = lstObject[i];
            if (go == null) continue;

            float dist = (pos - go.transform.position).sqrMagnitude;
            if (dist < shortest)
            {
                shortest = dist;
                nearest = go;
            }
        }
        return nearest;
    }

    public static GameObject FindNearest(this GameObject target, List<GameObject> lstObject) =>
        target == null ? null : target.transform.FindNearest(lstObject);

    public static GameObject FindNearest(this Component target, List<GameObject> lstObject) =>
        target == null ? null : target.transform.FindNearest(lstObject);

    // ---- Recursive finders ---------------------------------------------------------------------

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

    public static Transform FindChildRecursive(this GameObject gameObject, string name) =>
        gameObject == null ? null : gameObject.transform.FindChildRecursive(name);

    public static Transform FindChildRecursive(this Component component, string name) =>
        component == null ? null : component.transform.FindChildRecursive(name);

    public static T FindComponentInChildrenRecursive<T>(this Transform aParent)
    {
        if (aParent == null) return default;
        if (aParent.TryGetComponent(out T result)) return result;

        foreach (Transform child in aParent)
        {
            result = child.FindComponentInChildrenRecursive<T>();
            if (!Equals(result, default(T))) return result;
        }
        return default;
    }

    public static T FindComponentInChildrenRecursive<T>(this Component component) =>
        component == null ? default : component.transform.FindComponentInChildrenRecursive<T>();

    // ---- Direct children -----------------------------------------------------------------------

    public static Transform FindDirectChild(this Transform aParent, string aName)
    {
        if (aParent == null) return null;

        for (int i = 0; i < aParent.childCount; i++)
        {
            var child = aParent.GetChild(i);
            if (child.name == aName) return child;
        }
        return null;
    }

    public static Transform FindDirectChild(this GameObject aParent, string aName) =>
        aParent == null ? null : aParent.transform.FindDirectChild(aName);

    public static Transform FindDirectChild(this Component aParent, string aName) =>
        aParent == null ? null : aParent.transform.FindDirectChild(aName);

    // Returns null for a null parent, otherwise a new array (a fresh list per call: safe when called re-entrantly).
    public static Transform[] FindDirectChildren(this Transform aParent, string aName)
    {
        if (aParent == null) return null;

        var result = new List<Transform>();
        for (int i = 0; i < aParent.childCount; i++)
        {
            var child = aParent.GetChild(i);
            if (child.name == aName) result.Add(child);
        }
        return result.ToArray();
    }

    public static Transform[] FindDirectChildren(this GameObject aParent, string aName) =>
        aParent == null ? null : aParent.transform.FindDirectChildren(aName);

    public static Transform[] FindDirectChildren(this Component aParent, string aName) =>
        aParent == null ? null : aParent.transform.FindDirectChildren(aName);

    public static Transform[] FindDirectChildrenStartingWith(this Transform aParent, string namePrefix)
    {
        if (aParent == null || string.IsNullOrEmpty(namePrefix)) return null;

        var result = new List<Transform>();
        for (int i = 0; i < aParent.childCount; i++)
        {
            var child = aParent.GetChild(i);
            if (child.name.StartsWith(namePrefix, StringComparison.Ordinal)) result.Add(child);
        }
        return result.ToArray();
    }

    public static Transform[] FindDirectChildrenStartingWith(this GameObject aParent, string namePrefix) =>
        aParent == null ? null : aParent.transform.FindDirectChildrenStartingWith(namePrefix);

    public static Transform[] FindDirectChildrenStartingWith(this Component aParent, string namePrefix) =>
        aParent == null ? null : aParent.transform.FindDirectChildrenStartingWith(namePrefix);

    public static T[] FindDirectChildrenStartingWith<T>(this Transform aParent, string namePrefix)
    {
        if (aParent == null || string.IsNullOrEmpty(namePrefix)) return null;

        var results = new List<T>();
        for (int i = 0; i < aParent.childCount; i++)
        {
            var child = aParent.GetChild(i);
            if (child.name.StartsWith(namePrefix, StringComparison.Ordinal) && child.TryGetComponent(out T component))
                results.Add(component);
        }
        return results.ToArray();
    }

    public static T[] FindDirectChildrenStartingWith<T>(this GameObject aParent, string namePrefix) =>
        aParent == null ? null : aParent.transform.FindDirectChildrenStartingWith<T>(namePrefix);

    public static T[] FindDirectChildrenStartingWith<T>(this Component aParent, string namePrefix) =>
        aParent == null ? null : aParent.transform.FindDirectChildrenStartingWith<T>(namePrefix);

    public static T[] FindChildrenRecursiveStartingWith<T>(this Transform aParent, string namePrefix)
    {
        if (aParent == null || string.IsNullOrEmpty(namePrefix)) return null;

        var results = new List<T>();
        CollectStartingWith(aParent, namePrefix, results);
        return results.ToArray();
    }

    public static T[] FindChildrenRecursiveStartingWith<T>(this GameObject aParent, string namePrefix) =>
        aParent == null ? null : aParent.transform.FindChildrenRecursiveStartingWith<T>(namePrefix);

    public static T[] FindChildrenRecursiveStartingWith<T>(this Component aParent, string namePrefix) =>
        aParent == null ? null : aParent.transform.FindChildrenRecursiveStartingWith<T>(namePrefix);

    private static void CollectStartingWith<T>(Transform parent, string namePrefix, List<T> results)
    {
        for (int i = 0; i < parent.childCount; i++)
        {
            Transform child = parent.GetChild(i);
            if (child.name.StartsWith(namePrefix, StringComparison.Ordinal) && child.TryGetComponent(out T component))
                results.Add(component);

            CollectStartingWith(child, namePrefix, results);
        }
    }

    public static bool HasDirectChild(this Transform aParent, string aName) => aParent.FindDirectChild(aName) != null;

    public static bool HasDirectChild(this GameObject aParent, string aName) => aParent.FindDirectChild(aName) != null;

    public static bool HasDirectChild(this Component aParent, string aName) => aParent.FindDirectChild(aName) != null;

    // ---- By index / by name in an array --------------------------------------------------------

    public static Transform GetChildAt(this GameObject aParent, int index)
    {
        if (aParent == null) return null;

        var t = aParent.transform;
        return index < 0 || index >= t.childCount ? null : t.GetChild(index);
    }

    public static Transform GetChildAt(this Component aParent, int index) =>
        aParent == null ? null : aParent.gameObject.GetChildAt(index);

    [Obsolete("Use GetChildAt.")]
    public static Transform GetChildTransform(this GameObject _object, int index) => _object.GetChildAt(index);

    [Obsolete("Use GetChildAt.")]
    public static Transform GetChildTransform(this Component component, int index) => component.GetChildAt(index);

    public static Transform FindByName(this Transform[] arr, string name)
    {
        if (arr == null || string.IsNullOrEmpty(name)) return null;

        for (int i = 0; i < arr.Length; i++)
            if (arr[i] != null && arr[i].name == name) return arr[i];
        return null;
    }
}
