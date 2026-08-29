using System;
using UnityEngine;

public static class UtilsJsonHelper
{
    public static string ToJsonArray<T>(T[] array, bool prettyPrint = false)
    {
        var wrapper = new DataArrayWrapper<T> { Data = array };
        return JsonUtility.ToJson(wrapper, prettyPrint);
    }

    public static T[] FromJsonDataArray<T>(string json)
    {
        if (string.IsNullOrEmpty(json)) return Array.Empty<T>();

        var wrapper = JsonUtility.FromJson<DataArrayWrapper<T>>(json);
        return wrapper?.Data ?? Array.Empty<T>();
    }

    public static T[] FromJsonRawDataArray<T>(string json)
    {
        if (string.IsNullOrEmpty(json)) return Array.Empty<T>();

        string newJson = "{ \"Data\": " + json + "}";
        return FromJsonDataArray<T>(newJson);
    }

    public static T FromJsonRawDataObject<T>(string json)
    {
        if (string.IsNullOrEmpty(json)) return default;

        string newJson = "{ \"Data\": " + json + "}";
        var wrapper = JsonUtility.FromJson<DataObjectWrapper<T>>(newJson);
        return wrapper != null ? wrapper.Data : default;
    }

    public static T[] FromJsonArray<T>(string json)
    {
        if (string.IsNullOrEmpty(json)) return Array.Empty<T>();

        string newJson = "{ \"array\": " + json + "}";
        var wrapper = JsonUtility.FromJson<ArrayWrapper<T>>(newJson);
        return wrapper?.array ?? Array.Empty<T>();
    }

    public static T FromJsonObject<T>(string json)
    {
        if (string.IsNullOrEmpty(json)) return default;

        string newJson = "{ \"array\": " + json + "}";
        var wrapper = JsonUtility.FromJson<ObjectWrapper<T>>(newJson);
        return wrapper != null ? wrapper.array : default;
    }

    [Serializable]
    private class ObjectWrapper<T> { public T array; }

    [Serializable]
    private class ArrayWrapper<T> { public T[] array; }

    [Serializable]
    private class DataArrayWrapper<T> { public T[] Data; }

    [Serializable]
    private class DataObjectWrapper<T> { public T Data; }
}
