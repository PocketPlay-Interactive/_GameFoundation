using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SettingSerializable
{
    [SerializeField]
    private List<string> keys = new List<string>();
    [SerializeField]
    private List<bool> values = new List<bool>();
    [SerializeField]
    private List<string> objectKeys = new List<string>();
    [SerializeField]
    private List<string> objectValues = new List<string>();

    public SettingSerializable()
    {
        EnsureDefaults();
    }

    public void Set(string key, bool value)
    {
        EnsureLists();
        key = key.ToLower();
        if (keys.Contains(key))
        {
            int index = keys.IndexOf(key);
            values[index] = value;
        }
        else
        {
            keys.Add(key);
            values.Add(value);
        }
    }

    public void Set<T>(string key, T value)
    {
        EnsureLists();
        if (typeof(T) == typeof(string))
        {
            SetObject(key, (object)value as string);
            return;
        }

        if (value is bool boolValue)
        {
            Set(key, boolValue);
            return;
        }

        key = key.ToLower();
        SetObject(key, JsonUtility.ToJson(new ValueBox<T> { value = value }));
    }

    public bool Get(string key, bool defaultValue = false)
    {
        EnsureLists();
        key = key.ToLower();
        if (keys.Contains(key))
        {
            int index = keys.IndexOf(key);
            return index < values.Count ? values[index] : defaultValue;
        }

        return defaultValue;
    }

    public T Get<T>(string key, T defaultValue = default(T))
    {
        EnsureLists();
        if (typeof(T) == typeof(bool))
            return (T)(object)Get(key, defaultValue is bool boolValue && boolValue);

        key = key.ToLower();
        int index = objectKeys.IndexOf(key);
        if (index < 0 || index >= objectValues.Count)
            return defaultValue;

        try
        {
            string json = objectValues[index];
            if (json.Contains("\"value\""))
                return JsonUtility.FromJson<ValueBox<T>>(json).value;

            return JsonUtility.FromJson<T>(json);
        }
        catch
        {
            return defaultValue;
        }
    }

    public void EnsureDefaults()
    {
        AddDefault("MUSIC", true);
        AddDefault("SOUND", true);
        AddDefault("VIBRATE", true);
    }

    public bool HasKey(string key)
    {
        EnsureLists();
        key = key.ToLower();
        return keys.Contains(key) || objectKeys.Contains(key);
    }

    public void LogAllSettings()
    {
        EnsureLists();
        for (int i = 0; i < keys.Count && i < values.Count; i++)
        {
            Debug.Log($"Setting - Key: {keys[i]}, Value: {values[i]}");
        }

        for (int i = 0; i < objectKeys.Count && i < objectValues.Count; i++)
        {
            Debug.Log($"Setting - Key: {objectKeys[i]}, Value: {objectValues[i]}");
        }
    }

    private void AddDefault(string key, bool value)
    {
        EnsureLists();
        key = key.ToLower();
        if (!keys.Contains(key))
            Set(key, value);
    }

    private void SetObject(string key, string value)
    {
        key = key.ToLower();
        int index = objectKeys.IndexOf(key);

        if (index >= 0)
            objectValues[index] = value;
        else
        {
            objectKeys.Add(key);
            objectValues.Add(value);
        }
    }

    private void EnsureLists()
    {
        if (keys == null) keys = new List<string>();
        if (values == null) values = new List<bool>();
        if (objectKeys == null) objectKeys = new List<string>();
        if (objectValues == null) objectValues = new List<string>();
    }

    [System.Serializable]
    private class ValueBox<T>
    {
        public T value;
    }
}
