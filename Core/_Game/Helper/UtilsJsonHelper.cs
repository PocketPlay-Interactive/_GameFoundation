using System;
using UnityEngine;

public static class UtilsJsonHelper
{
    public static T[] FromJsonArray<T>(string json)
    {
        string newJson = "{ \"array\": " + json + "}";
        var wrapper = JsonUtility.FromJson<ArrayWrapper<T>>(newJson);
        return wrapper.array;
    }

    public static T FromJsonObject<T>(string json)
    {
        string newJson = "{ \"array\": " + json + "}";
        var wrapper = JsonUtility.FromJson<ObjectWrapper<T>>(newJson);
        return wrapper.array;
    }

    [Serializable]
    private class ArrayWrapper<T> { public T[] array; }

    [Serializable]
    private class ObjectWrapper<T> { public T array; }
}
