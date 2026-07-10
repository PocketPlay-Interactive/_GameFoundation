using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SettingSerializable
{
    [SerializeField]
    private List<string> keys = new List<string>();
    [SerializeField]
    private List<bool> values = new List<bool>();

    public SettingSerializable()
    {
        EnsureDefaults();
    }

    public void Set(string key, bool value)
    {
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

    public bool Get(string key, bool defaultValue = false)
    {
        key = key.ToLower();
        if (keys.Contains(key))
        {
            int index = keys.IndexOf(key);
            return values[index];
        }

        return defaultValue;
    }

    public void EnsureDefaults()
    {
        AddDefault("MUSIC", true);
        AddDefault("SOUND", true);
        AddDefault("VIBRATE", true);
    }

    public bool HasKey(string key)
    {
        key = key.ToLower();
        return keys.Contains(key);
    }

    public void LogAllSettings()
    {
        for (int i = 0; i < keys.Count && i < values.Count; i++)
        {
            Debug.Log($"Setting - Key: {keys[i]}, Value: {values[i]}");
        }
    }

    private void AddDefault(string key, bool value)
    {
        key = key.ToLower();
        if (!keys.Contains(key))
            Set(key, value);
    }
}
