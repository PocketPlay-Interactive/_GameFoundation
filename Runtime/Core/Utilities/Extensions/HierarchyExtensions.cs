using System;
using System.Collections.Generic;
using UnityEngine;

public static class HierarchyExtensions
{
    // Reusable buffer to reduce GC when collecting children
    private static readonly List<Transform> childrenBuffer = new List<Transform>(32);


    /// <summary>
    /// Find nearest GameObject from a list (null-safe list items).
    /// </summary>
    public static GameObject FindNearest(this Transform target, List<GameObject> lstObject)
    {
        if (target == null || lstObject == null || lstObject.Count == 0) return null;

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
        if (target == null || lstObject == null || lstObject.Count == 0) return null;

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

    public static GameObject FindNearest(this Component target, List<GameObject> lstObject)
    {
        if (target == null) return null;
        return target.transform.FindNearest(lstObject);
    }

    // --- Recursive & direct child finders ---
    public static Transform FindChildRecursive(this Component component, string name)
    {
        if (component == null)
            return null;

        return component.transform.FindChildRecursive(name);
    }

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

    public static T FindComponentInChildrenRecursive<T>(this Component component)
    {
        if (component == null)
            return default;

        return component.transform.FindComponentInChildrenRecursive<T>();
    }

    public static T FindComponentInChildrenRecursive<T>(this Transform aParent)
    {
        if (aParent == null) return default;
        var result = aParent.GetComponent<T>();
        if (!Equals(result, default(T))) return result;

        foreach (Transform child in aParent)
        {
            result = child.FindComponentInChildrenRecursive<T>();
            if (!Equals(result, default(T))) return result;
        }
        return default;
    }

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

    public static Transform[] FindDirectChildren(this Transform aParent, string aName)
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

    public static Transform FindDirectChild(this Component aParent, string aName)
    {
        if (aParent == null) return null;
        return aParent.transform.FindDirectChild(aName);
    }

    public static Transform[] FindDirectChildren(this Component aParent, string aName)
    {
        if (aParent == null) return null;
        return aParent.transform.FindDirectChildren(aName);
    }

    public static Transform[] FindDirectChildrenStartingWith(this Transform aParent, string namePrefix)
    {
        childrenBuffer.Clear();
        if (aParent == null || string.IsNullOrEmpty(namePrefix)) return null;

        for (int i = 0; i < aParent.childCount; i++)
        {
            var child = aParent.GetChild(i);
            if (child.name.StartsWith(namePrefix, StringComparison.Ordinal)) childrenBuffer.Add(child);
        }
        return childrenBuffer.ToArray();
    }

    public static T[] FindDirectChildrenStartingWith<T>(this Transform aParent, string namePrefix)
    {
        if (aParent == null || string.IsNullOrEmpty(namePrefix)) return null;

        List<T> results = new List<T>();
        for (int i = 0; i < aParent.childCount; i++)
        {
            var child = aParent.GetChild(i);
            if (!child.name.StartsWith(namePrefix, StringComparison.Ordinal)) continue;

            var component = child.GetComponent<T>();
            if (!Equals(component, default(T))) results.Add(component);
        }
        return results.ToArray();
    }

    public static T[] FindChildrenRecursiveStartingWith<T>(this Transform aParent, string namePrefix)
    {
        if (aParent == null || string.IsNullOrEmpty(namePrefix)) return null;

        List<T> results = new List<T>();
        CollectChildrenRecursiveStartingWith(aParent, namePrefix, results);
        return results.ToArray();
    }

    private static void CollectChildrenRecursiveStartingWith<T>(
        Transform parent,
        string namePrefix,
        List<T> results)
    {
        for (int i = 0; i < parent.childCount; i++)
        {
            Transform child = parent.GetChild(i);
            if (child.name.StartsWith(namePrefix, StringComparison.Ordinal))
            {
                T component = child.GetComponent<T>();
                if (!Equals(component, default(T))) results.Add(component);
            }

            CollectChildrenRecursiveStartingWith(child, namePrefix, results);
        }
    }

    public static Transform[] FindDirectChildrenStartingWith(this Component aParent, string namePrefix)
    {
        if (aParent == null) return null;
        return aParent.transform.FindDirectChildrenStartingWith(namePrefix);
    }

    public static T[] FindDirectChildrenStartingWith<T>(this Component aParent, string namePrefix)
    {
        if (aParent == null) return null;
        return aParent.transform.FindDirectChildrenStartingWith<T>(namePrefix);
    }

    public static T[] FindChildrenRecursiveStartingWith<T>(this Component aParent, string namePrefix)
    {
        if (aParent == null) return null;
        return aParent.transform.FindChildrenRecursiveStartingWith<T>(namePrefix);
    }

    public static Transform FindDirectChild(this GameObject aParent, string aName)
    {
        if (aParent == null) return null;
        return aParent.transform.FindDirectChild(aName);
    }

    public static Transform[] FindDirectChildren(this GameObject aParent, string aName)
    {
        if (aParent == null) return null;
        return aParent.transform.FindDirectChildren(aName);
    }

    public static Transform[] FindDirectChildrenStartingWith(this GameObject aParent, string namePrefix)
    {
        if (aParent == null) return null;
        return aParent.transform.FindDirectChildrenStartingWith(namePrefix);
    }

    public static T[] FindDirectChildrenStartingWith<T>(this GameObject aParent, string namePrefix)
    {
        if (aParent == null) return null;
        return aParent.transform.FindDirectChildrenStartingWith<T>(namePrefix);
    }

    public static T[] FindChildrenRecursiveStartingWith<T>(this GameObject aParent, string namePrefix)
    {
        if (aParent == null) return null;
        return aParent.transform.FindChildrenRecursiveStartingWith<T>(namePrefix);
    }

    public static Transform GetChildAt(this GameObject aParent, int index)
    {
        if (aParent == null) return null;
        var t = aParent.transform;
        if (index < 0 || index >= t.childCount) return null;
        return t.GetChild(index);
    }

    public static Transform GetChildAt(this Component aParent, int index)
    {
        if (aParent == null) return null;
        return aParent.gameObject.GetChildAt(index);
    }

    public static Transform FindByName(this Transform[] arr, string name)
    {
        if (arr == null || string.IsNullOrEmpty(name)) return null;

        for (int i = 0; i < arr.Length; i++)
            if (arr[i] != null && arr[i].name == name) return arr[i];
        return null;
    }

    public static bool HasDirectChild(this Transform aParent, string aName)
    {
        if (aParent == null) return false;
        for (int i = 0; i < aParent.childCount; i++)
            if (aParent.GetChild(i).name == aName) return true;
        return false;
    }

    public static bool HasDirectChild(this Component aParent, string aName)
    {
        if (aParent == null) return false;
        return aParent.transform.HasDirectChild(aName);
    }

    public static bool HasDirectChild(this GameObject aParent, string aName)
    {
        if (aParent == null) return false;
        return aParent.transform.HasDirectChild(aName);
    }

    public static Transform GetChildTransform(this GameObject _object, int index)
    {
        if (_object == null || index < 0 || index >= _object.transform.childCount) return null;
        return _object.transform.GetChild(index);
    }

    public static Transform GetChildTransform(this Component component, int index)
    {
        if (component == null) return null;
        return component.gameObject.GetChildTransform(index);
    }
}
