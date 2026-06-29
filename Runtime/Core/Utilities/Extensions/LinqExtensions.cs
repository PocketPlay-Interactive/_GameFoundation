using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public static class LinqExtensions
{
    private static readonly List<Transform> linqCache = new();

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
        dict.ContainsKey(key) ? dict[key] : default;

    public static T GetRandom<T>(this IEnumerable<T> enumerable,out int index)
    {
        var list = enumerable.ToList();
        index = Random.Range(0, list.Count);
        return list[index];
    }

    public static T GetRandomWithReduceFactor<T>(this IEnumerable<T> enumerable,float factor)
    {
        var list = enumerable.ToList();
    
        var probabilityList = new List<float>();

        var currentProbability = 1f;
        probabilityList.Add(1f);
        for (var i = 1; i < list.Count; i++)
        {
            currentProbability = currentProbability*factor;
            probabilityList.Add(currentProbability);
        }

        var p = Random.Range(0f, probabilityList.Sum());

        for (var i = 0; i < list.Count; i++)
        {
            p -= probabilityList[i];
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
        var list = enumerable.ToList();

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
        var list = enumerable.ToList();

        if (list.Count == 0)
            return default;

        return list.GetRandom();
    }

    public static T GetAndRemove<T>(this IList<T> list, T item)
    {
       return list.GetAndRemove(list.IndexOf(item));
    }

    public static T GetAndRemove<T>(this IList<T> list, int index)
    {
        if (index < 0 || index>=list.Count)
            return default;
        var item = list[index];
        list.RemoveAt(index);
        return item;
    }

    public static int ChildCount(this GameObject _parent)
    {
        return _parent.transform.childCount;
    }

    public static void ForChild(this Transform _parent, Action<Transform> action)
    {
        for(int i = 0; i < _parent.childCount; i++)
        {
            action?.Invoke(_parent.GetChild(i));
        }
    }

    public static void ForChild(this GameObject _parent, Action<Transform> action)
    {
        for (int i = 0; i < _parent.transform.childCount; i++)
        {
            action?.Invoke(_parent.GetChild(i));
        }
    }

    public static void ForChild(this Transform _parent, Action<int, Transform> action)
    {
        for (int i = 0; i < _parent.childCount; i++)
        {
            action?.Invoke(i, _parent.GetChild(i));
        }
    }

    public static void ForChildReverse(this Transform parent, Action<Transform> action)
    {
        int count = parent.childCount;
        for(int i = count - 1; i >= 0; i--)
        {
            action?.Invoke((Transform)parent.GetChild(i));
        }    
    }

    public static void ForChildReverse(this Transform parent, Action<int, Transform> action)
    {
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
    {
        // Lấy số lượng child
        int childCount = parent.childCount;

        // Khởi tạo mảng với số lượng child
        Transform[] children = new Transform[childCount];

        // Lặp qua tất cả các child và thêm vào mảng
        for (int i = 0; i < childCount; i++)
        {
            children[i] = parent.GetChild(i);
        }

        return children;
    }

    public static Transform[] GetChilds(this GameObject parent)
    {
        // Lấy số lượng child
        int childCount = parent.transform.childCount;

        // Khởi tạo mảng với số lượng child
        Transform[] children = new Transform[childCount];

        // Lặp qua tất cả các child và thêm vào mảng
        for (int i = 0; i < childCount; i++)
        {
            children[i] = parent.GetChild(i);
        }

        return children;
    }

    public static List<Transform> GetRandomChildren(this GameObject parent, int count)
    {
        linqCache.Clear();
        if (parent == null) return linqCache;

        foreach (Transform child in parent.transform)
        {
            linqCache.Add(child);
        }

        return linqCache.OrderBy(x => Random.value).Take(count).ToList();
    }

    public static List<Transform> GetRandomChildren(this Transform parent, int count)
    {
        linqCache.Clear();
        if (parent == null) return linqCache;

        foreach (Transform child in parent.transform)
        {
            linqCache.Add(child);
        }

        return linqCache.OrderBy(x => Random.value).Take(count).ToList();
    }

    public static void ForEach<T>(this IEnumerable<T> enumerable, Action<T> action)
    {
        foreach (var item in enumerable)
        {
            action?.Invoke(item);
        }
    }

    public static void ForEachReverse<T>(this T[] arr, Action<T> action)
    {
        int count = arr.Length;
        for (int i = count - 1; i >= 0; i--)
        {
            action?.Invoke(arr[i]);
        }
    }

    public static void ForEachReverse<T>(this List<T> arr, Action<T> action)
    {
        int count = arr.Count;
        for (int i = count - 1; i >= 0; i--)
        {
            action?.Invoke(arr[i]);
        }
    }

    public static void SimpleForEach<T>(this T[] _array, Action<T> action)
    {
        for(int i = 0; i < _array.Length; i++)
        {
            if (_array[i] == null)
                continue;
            action?.Invoke(_array[i]);
        }
    }

    public static void ForEach<T>(this T[] _array, Action<T> action)
    {
        for (int i = 0; i < _array.Length; i++)
        {
            if (_array[i] == null)
                continue;
            action?.Invoke(_array[i]);
        }
    }

    public static void SimpleForEach<T>(this List<T> _array, Action<T> action)
    {
        for (int i = 0; i < _array.Count; i++)
        {
            action?.Invoke(_array[i]);
        }
    }

    public static void ForEach<T>(this IEnumerable<T> enumerable, Action<T, int> action)
    {
        int i = 0;
        foreach (var item in enumerable)
        {
            action?.Invoke(item, i);
            i++;
        }
    }

    public static void SimpleForEach<T>(this T[] _array, Action<T, int> action)
    {
        for (int i = 0; i < _array.Length; i++)
        {
            action?.Invoke(_array[i], i);
        }
    }

    public static void SimpleForEach<T>(this List<T> _array, Action<T, int> action)
    {
        for (int i = 0; i < _array.Count; i++)
        {
            action?.Invoke(_array[i], i);
        }
    }

    public static Transform FindOnce(this List<Transform> _array, bool _isActive)
    {
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
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }
}