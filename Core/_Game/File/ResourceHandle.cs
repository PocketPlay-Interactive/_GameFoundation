using System.Collections.Generic;
using UnityEngine;

public class ResourceHandle : GenericSingleton<ResourceHandle>
{
    private readonly Dictionary<string, Object> _cache = new();

    public T Load<T>(string path) where T : Object
    {
        if (_cache.TryGetValue(path, out var cached) && cached is T tObj)
            return tObj;

        var asset = Resources.Load<T>(path);
        if (asset != null)
            _cache[path] = asset;
        else
            Debug.LogWarning($"[ResourceHandle] Không tìm thấy asset tại: {path}");
        return asset;
    }

    public T[] LoadAll<T>(string folderPath) where T : Object
    {
        var assets = Resources.LoadAll<T>(folderPath);
        if (assets == null || assets.Length == 0)
            Debug.LogWarning($"[ResourceHandle] Không tìm thấy asset nào trong thư mục: {folderPath}");
        return assets;
    }

    public void Unload(string path)
    {
        if (_cache.TryGetValue(path, out var asset) && asset != null)
        {
            Resources.UnloadAsset(asset);
            _cache.Remove(path);
        }
    }

    public void UnloadAll()
    {
        foreach (var asset in _cache.Values)
            if (asset != null)
                Resources.UnloadAsset(asset);
        _cache.Clear();
    }

    public void UnloadUnused()
    {
        Resources.UnloadUnusedAssets();
    }

    public GameObject LoadPrefab(string prefabName)
    {
        string path = $"Prefabs/{prefabName}";
        var prefab = Load<GameObject>(path);
        if (prefab == null)
            Debug.LogWarning($"[ResourceHandle] Không tìm thấy prefab: {prefabName} trong folder Prefabs");
        return prefab;
    }

    public T LoadData<T>(string dataFileName) where T : Object
    {
        string path = $"Data/{dataFileName}";
        var data = Load<T>(path);
        if (data == null)
            Debug.LogWarning($"[ResourceHandle] Không tìm thấy data: {dataFileName} trong folder Data (type: {typeof(T).Name})");
        return data;
    }
}