using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;

public class Pooling : GenericSingleton<Pooling>
{
    private readonly Dictionary<int, List<GameObject>> pools = new();
    // Rental counter per object: lets a delayed return detect the object was reused meanwhile.
    // ponytail: entries for destroyed objects are never pruned; negligible for pooled objects.
    private readonly Dictionary<int, int> rentals = new();

    private int Rental(GameObject go) => rentals.TryGetValue(go.GetInstanceID(), out var r) ? r : 0;
    private void Rent(GameObject go) => rentals[go.GetInstanceID()] = Rental(go) + 1;

    public GameObject GetPrefab(string prefabName)
    {
        var prefab = ResourceHandle.Get().LoadPrefab(prefabName);
        if (prefab == null)
        {
            Debug.LogError($"Pooling: Prefab not found in Resources/Prefabs/{prefabName}");
            return null;
        }
        return GetPrefab(prefab);
    }

    public GameObject GetPrefab(GameObject prefab)
    {
        if (prefab == null) return null;

        var list = GetPool(prefab);
        var inactive = GetInactive(list);
        if (inactive != null) return Activate(inactive);

        var obj = Object.Instantiate(prefab);
        obj.name = prefab.name;
        list.Add(obj);
        return Activate(obj);
    }

    public GameObject GetPrefab(GameObject prefab, Vector3 pos, Quaternion rot, Transform parent = null)
    {
        if (prefab == null) return null;

        var list = GetPool(prefab);
        var inactive = GetInactive(list);
        if (inactive != null) return Activate(inactive, pos, rot, parent);

        var obj = Object.Instantiate(prefab, pos, rot, parent);
        obj.name = prefab.name;
        list.Add(obj);
        return Activate(obj, pos, rot, parent);
    }

    public GameObject GetPrefab(GameObject prefab, Transform parent)
        => prefab == null ? null : GetPrefab(prefab, prefab.transform.position, prefab.transform.rotation, parent);

    public T GetPrefab<T>(GameObject prefab) where T : Component
        => GetPrefab(prefab)?.GetComponent<T>();

    public T GetPrefab<T>(GameObject prefab, Vector3 pos, Quaternion rot) where T : Component
        => GetPrefab(prefab, pos, rot)?.GetComponent<T>();

    public T GetPrefab<T>(GameObject prefab, Transform parent) where T : Component
        => GetPrefab(prefab, parent)?.GetComponent<T>();

    public void PushToPool(GameObject obj, bool keepParent = false)
    {
        if (obj == null) return;
        obj.SetActive(false);
        if (!keepParent) obj.transform.SetParent(null);
    }

    public void PushToPool(GameObject obj, float time)
    {
        if (obj == null) return;
        int rental = Rental(obj);
        DOVirtual.DelayedCall(time, () =>
        {
            if (obj != null && Rental(obj) == rental) PushToPool(obj); // skip if reused since
        });
    }

    private List<GameObject> GetPool(GameObject prefab)
    {
        int id = prefab.GetInstanceID();
        if (!pools.TryGetValue(id, out var list))
            pools[id] = list = new List<GameObject>();

        return list;
    }

    private GameObject GetInactive(List<GameObject> list)
    {
        for (int i = list.Count - 1; i >= 0; i--)
        {
            var go = list[i];
            if (go == null)
            {
                list.RemoveAt(i);
                continue;
            }

            if (!go.activeSelf)
                return go;
        }

        return null;
    }

    private GameObject Activate(GameObject go)
    {
        Rent(go);
        go.SetActive(true);
        return go;
    }
    private GameObject Activate(GameObject go, Vector3 pos, Quaternion rot, Transform parent)
    {
        Rent(go);
        go.transform.SetParent(parent);
        go.transform.SetPositionAndRotation(pos, rot);
        go.SetActive(true);
        return go;
    }
}
