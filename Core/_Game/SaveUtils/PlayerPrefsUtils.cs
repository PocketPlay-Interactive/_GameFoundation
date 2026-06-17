using UnityEngine;

/// <summary>
/// Tiện ích mở rộng cho PlayerPrefs: hỗ trợ bool, int, float, string.
/// Có kiểm tra anti-cheat nếu ENABLE_ANTI_CHEAT = true trong Config.
/// </summary>
public static class PlayerPrefsUtils
{
    // Bool
    public static void SetBool(string key, bool value) => PlayerPrefs.SetInt(key, value ? 1 : 0);
    public static bool GetBool(string key, bool defaultValue = false) => PlayerPrefs.GetInt(key, defaultValue ? 1 : 0) != 0;

    // Int với anti-cheat
    public static void SetInt(string key, int value)
    {
        if (Config.ENABLE_ANTI_CHEAT)
        {
            int obfuscated = MemoryObfuscation.ObfuscateInt(value);
            int checksum = MemoryChecksum.GenerateChecksum(value);
            PlayerPrefs.SetInt(key, obfuscated);
            PlayerPrefs.SetInt(key + "_checksum", checksum);
        }
        else
        {
            PlayerPrefs.SetInt(key, value);
        }
    }

    public static int GetInt(string key, int defaultValue = 0)
    {
        if (Config.ENABLE_ANTI_CHEAT)
        {
            int obfuscated = PlayerPrefs.GetInt(key, MemoryObfuscation.ObfuscateInt(defaultValue));
            int checksum = PlayerPrefs.GetInt(key + "_checksum", MemoryChecksum.GenerateChecksum(defaultValue));
            int value = MemoryObfuscation.DeobfuscateInt(obfuscated);
            bool isValid = MemoryChecksum.VerifyChecksum(value, checksum);
            if (isValid)
                return value;
            else
                return defaultValue; // hoặc xử lý khác nếu phát hiện gian lận
        }
        else
        {
            return PlayerPrefs.GetInt(key, defaultValue);
        }
    }

    // Float với anti-cheat
    public static void SetFloat(string key, float value)
    {
        if (Config.ENABLE_ANTI_CHEAT)
        {
            int obfuscated = MemoryObfuscation.ObfuscateFloat(value);
            int checksum = MemoryChecksum.GenerateChecksum(obfuscated);
            PlayerPrefs.SetInt(key, obfuscated);
            PlayerPrefs.SetInt(key + "_checksum", checksum);
        }
        else
        {
            PlayerPrefs.SetFloat(key, value);
        }
    }

    public static float GetFloat(string key, float defaultValue = 0f)
    {
        if (Config.ENABLE_ANTI_CHEAT)
        {
            int obfuscated = PlayerPrefs.GetInt(key, MemoryObfuscation.ObfuscateFloat(defaultValue));
            int checksum = PlayerPrefs.GetInt(key + "_checksum", MemoryChecksum.GenerateChecksum(obfuscated));
            bool isValid = MemoryChecksum.VerifyChecksum(obfuscated, checksum);
            if (isValid)
                return MemoryObfuscation.DeobfuscateFloat(obfuscated);
            else
                return defaultValue;
        }
        else
        {
            return PlayerPrefs.GetFloat(key, defaultValue);
        }
    }

    // String (không anti-cheat)
    public static void SetString(string key, string value)
    {
        if (Config.ENABLE_ANTI_CHEAT)
        {
            string obfuscated = MemoryObfuscation.ObfuscateString(value);
            PlayerPrefs.SetString(key, obfuscated);
        }
        else
        {
            PlayerPrefs.SetString(key, value);
        }
    }
    
    public static string GetString(string key, string defaultValue = "")
    {
        if (Config.ENABLE_ANTI_CHEAT)
        {
            string obfuscated = PlayerPrefs.GetString(key, MemoryObfuscation.ObfuscateString(defaultValue));
            return MemoryObfuscation.DeobfuscateString(obfuscated);
        }
        else
        {
            return PlayerPrefs.GetString(key, defaultValue);
        }
    }
    // Xóa key
    public static void DeleteKey(string key)
    {
        PlayerPrefs.DeleteKey(key);
        PlayerPrefs.DeleteKey(key + "_checksum");
    }

    // Kiểm tra tồn tại
    public static bool HasKey(string key) => PlayerPrefs.HasKey(key);
}