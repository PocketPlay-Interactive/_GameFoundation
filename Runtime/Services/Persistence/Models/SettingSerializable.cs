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
        // Khởi tạo mặc định nếu muốn
        Set("music", true);
        Set("sound", true);
        Set("vibrate", true);
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
        Set(key, defaultValue);
        return defaultValue;
    }

    public bool HasKey(string key)
    {
        key = key.ToLower();
        return keys.Contains(key);
    }

    public void LogAllSettings()
    {
        for (int i = 0; i < keys.Count; i++)
        {
            Debug.Log($"Setting - Key: {keys[i]}, Value: {values[i]}");
        }
    }
}
