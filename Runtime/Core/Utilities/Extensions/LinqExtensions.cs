using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public static class LinqExtensions
{
    private static readonly List<Transform> linqCache = new();
    private static readonly List<float> probabilityCache = new();

    public static void AddOrUpdate<T, TJ>(this IDictionary<T, TJ> dict, T key, TJ val)
    {
        if (dict.ContainsKey(key))
        {
            dict[key] = val;
        }
        else
        {
            dict.Add(key, val);
        }
    }

    public static TJ GetOrDefault<T, TJ>(this IDictionary<T, TJ> dict, T key) =>
        dict != null && dict.TryGetValue(key, out var value) ? value : default;

    public static T GetRandom<T>(this IEnumerable<T> enumerable,out int index)
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

    public static T GetRandomWithReduceFactor<T>(this IEnumerable<T> enumerable,float factor)
    {
        var list = enumerable as IList<T> ?? enumerable?.ToList();
        if (list == null || list.Count == 0) return default;
        factor = Mathf.Max(0f, factor);
    
        probabilityCache.Clear();

        var currentProbability = 1f;
        probabilityCache.Add(1f);
        var probabilitySum = 1f;
        for (var i = 1; i < list.Count; i++)
        {
            currentProbability = currentProbability*factor;
            probabilityCache.Add(currentProbability);
            probabilitySum += currentProbability;
        }

        var p = Random.Range(0f, probabilitySum);

        for (var i = 0; i < list.Count; i++)
        {
            p -= probabilityCache[i];
            if (p <= 0)
            {
//                Debug.Log(i);
                return list[i];
            }

        }

        return list.GetRandom();
    }

    

    public static T GetRandom<T>(this IEnumerable<T> enumerable) => 
        enumerable.GetRandom(out var index);

    public static IEnumerable<T> GetRandom<T>(this IEnumerable<T> enumerable, int count)
    {
        var list = enumerable?.ToList();
        if (list == null || count <= 0)
            yield break;

        if (list.Count<count)
        {
            throw new InvalidOperationException();
        }

        for (var i = 0; i < count; i++)
        {
            var index = Random.Range(0,list.Count);
            yield return list[index];
            list.RemoveAt(index);
        }
    }


    public static T GetRandomOrDefault<T>(this IEnumerable<T> enumerable)
    {
        var list = enumerable as IList<T> ?? enumerable?.ToList();

        if (list == null || list.Count == 0)
            return default;

        return list.GetRandom();
    }

    public static T GetAndRemove<T>(this IList<T> list, T item)
    {
       if (list == null) return default;
       return list.GetAndRemove(list.IndexOf(item));
    }

    public static T GetAndRemove<T>(this IList<T> list, int index)
    {
        if (list == null || index < 0 || index>=list.Count)
            return default;
        var item = list[index];
        list.RemoveAt(index);
        return item;
    }

    public static int ChildCount(this GameObject _parent)
    {
        return _parent != null ? _parent.transform.childCount : 0;
    }

    public static void ForChild(this Transform _parent, Action<Transform> action)
    {
        if (_parent == null) return;

        for(int i = 0; i < _parent.childCount; i++)
        {
            action?.Invoke(_parent.GetChild(i));
        }
    }

    public static void ForChild(this GameObject _parent, Action<Transform> action)
    {
        if (_parent == null) return;

        for (int i = 0; i < _parent.transform.childCount; i++)
        {
            action?.Invoke(_parent.GetChildTransform(i));
        }
    }

    public static void ForChild(this Transform _parent, Action<int, Transform> action)
    {
        if (_parent == null) return;

        for (int i = 0; i < _parent.childCount; i++)
        {
            action?.Invoke(i, _parent.GetChild(i));
        }
    }

    public static void ForChildReverse(this Transform parent, Action<Transform> action)
    {
        if (parent == null) return;

        int count = parent.childCount;
        for(int i = count - 1; i >= 0; i--)
        {
            action?.Invoke((Transform)parent.GetChild(i));
        }    
    }

    public static void ForChildReverse(this Transform parent, Action<int, Transform> action)
    {
        if (parent == null) return;

        int count = parent.childCount;
        for (int i = count - 1; i >= 0; i--)
        {
            action?.Invoke(i, (Transform)parent.GetChild(i));
        }
    }

    public static Transform[] GetChildren(this Transform parent)
    {
        if (parent == null) return Array.Empty<Transform>();
        var arr = new Transform[parent.childCount];
        for (int i = 0; i < arr.Length; i++)
            arr[i] = parent.GetChild(i);
        return arr;
    }

    public static Transform[] GetChilds(this Transform parent)
        => parent.GetChildren();

    public static Transform[] GetChilds(this GameObject parent)
        => parent != null ? parent.transform.GetChildren() : Array.Empty<Transform>();

    public static List<Transform> GetRandomChildren(this GameObject parent, int count)
    {
        if (parent == null || count <= 0) return new List<Transform>();

        linqCache.Clear();
        foreach (Transform child in parent.transform)
        {
            linqCache.Add(child);
        }

        linqCache.Shuffle();
        return linqCache.GetRange(0, Mathf.Min(count, linqCache.Count));
    }

    public static List<Transform> GetRandomChildren(this Transform parent, int count)
    {
        if (parent == null || count <= 0) return new List<Transform>();

        linqCache.Clear();
        foreach (Transform child in parent.transform)
        {
            linqCache.Add(child);
        }

        linqCache.Shuffle();
        return linqCache.GetRange(0, Mathf.Min(count, linqCache.Count));
    }

    public static void ForEach<T>(this IEnumerable<T> enumerable, Action<T> action)
    {
        if (enumerable == null) return;

        foreach (var item in enumerable)
        {
            action?.Invoke(item);
        }
    }

    public static void ForEachReverse<T>(this T[] arr, Action<T> action)
    {
        if (arr == null) return;

        int count = arr.Length;
        for (int i = count - 1; i >= 0; i--)
        {
            action?.Invoke(arr[i]);
        }
    }

    public static void ForEachReverse<T>(this List<T> arr, Action<T> action)
    {
        if (arr == null) return;

        int count = arr.Count;
        for (int i = count - 1; i >= 0; i--)
        {
            action?.Invoke(arr[i]);
        }
    }

    public static void SimpleForEach<T>(this T[] _array, Action<T> action)
        => _array.ForEach(action);

    public static void ForEach<T>(this T[] _array, Action<T> action)
    {
        if (_array == null) return;

        for (int i = 0; i < _array.Length; i++)
        {
            if (_array[i] == null)
                continue;
            action?.Invoke(_array[i]);
        }
    }

    public static void SimpleForEach<T>(this List<T> _array, Action<T> action)
    {
        if (_array == null) return;

        for (int i = 0; i < _array.Count; i++)
        {
            action?.Invoke(_array[i]);
        }
    }

    public static void ForEach<T>(this IEnumerable<T> enumerable, Action<T, int> action)
    {
        if (enumerable == null) return;

        int i = 0;
        foreach (var item in enumerable)
        {
            action?.Invoke(item, i);
            i++;
        }
    }

    public static void SimpleForEach<T>(this T[] _array, Action<T, int> action)
    {
        if (_array == null) return;

        for (int i = 0; i < _array.Length; i++)
        {
            action?.Invoke(_array[i], i);
        }
    }

    public static void SimpleForEach<T>(this List<T> _array, Action<T, int> action)
    {
        if (_array == null) return;

        for (int i = 0; i < _array.Count; i++)
        {
            action?.Invoke(_array[i], i);
        }
    }

    public static Transform FindOnce(this List<Transform> _array, bool _isActive)
    {
        if (_array == null) return null;

        Transform _res = null;
        for (int i = 0; i < _array.Count; i++)
        {
            if(_array[i].IsActive() == _isActive)
            {
                _res = _array[i];
                if(_isActive == true) _res.Hide();
                else _res.Show();

                break;
            }    
        }
        return _res;
    }

    // Shuffle extension
    public static void Shuffle<T>(this IList<T> list)
    {
        if (list == null) return;

        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }
}
