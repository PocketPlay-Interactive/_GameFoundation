using UnityEngine;

/// <summary>
/// Tiện ích mở rộng cho PlayerPrefs: hỗ trợ bool, int, float, string.
/// Có kiểm tra anti-cheat nếu ENABLE_ANTI_CHEAT = true trong Config.
/// </summary>
public static class PlayerPrefsUtils
{
    private const string ChecksumSuffix = "_checksum";
    private const string ObfuscationKeySuffix = "_obfkey";

    // Bool
    public static void SetBool(string key, bool value) => SetInt(key, value ? 1 : 0);
    public static bool GetBool(string key, bool defaultValue = false)
    {
        if (Config.ENABLE_ANTI_CHEAT &&
            PlayerPrefs.HasKey(key) &&
            !PlayerPrefs.HasKey(key + ChecksumSuffix) &&
            !PlayerPrefs.HasKey(key + ObfuscationKeySuffix))
            return PlayerPrefs.GetInt(key, defaultValue ? 1 : 0) != 0;

        return GetInt(key, defaultValue ? 1 : 0) != 0;
    }

    // Int với anti-cheat
    public static void SetInt(string key, int value)
    {
        if (Config.ENABLE_ANTI_CHEAT)
        {
            int valueKey = MemoryObfuscation.GenerateKey();
            int obfuscated = MemoryObfuscation.ObfuscateInt(value, valueKey);
            int checksum = MemoryChecksum.GenerateChecksum(value);
            PlayerPrefs.SetInt(key, obfuscated);
            PlayerPrefs.SetInt(key + ObfuscationKeySuffix, valueKey);
            PlayerPrefs.SetInt(key + ChecksumSuffix, checksum);
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
            if (!PlayerPrefs.HasKey(key))
                return defaultValue;

            int valueKey = PlayerPrefs.GetInt(key + ObfuscationKeySuffix, MemoryObfuscation.FixedKey);
            int obfuscated = PlayerPrefs.GetInt(key);
            int checksum = PlayerPrefs.GetInt(key + ChecksumSuffix, MemoryChecksum.GenerateChecksum(defaultValue));
            int value = MemoryObfuscation.DeobfuscateInt(obfuscated, valueKey);
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
            int valueKey = MemoryObfuscation.GenerateKey();
            int obfuscated = MemoryObfuscation.ObfuscateFloat(value, valueKey);
            int checksum = MemoryChecksum.GenerateChecksum(obfuscated);
            PlayerPrefs.SetInt(key, obfuscated);
            PlayerPrefs.SetInt(key + ObfuscationKeySuffix, valueKey);
            PlayerPrefs.SetInt(key + ChecksumSuffix, checksum);
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
            if (!PlayerPrefs.HasKey(key))
                return defaultValue;

            int valueKey = PlayerPrefs.GetInt(key + ObfuscationKeySuffix, MemoryObfuscation.FixedKey);
            int obfuscated = PlayerPrefs.GetInt(key);
            int checksum = PlayerPrefs.GetInt(key + ChecksumSuffix, MemoryChecksum.GenerateChecksum(obfuscated));
            bool isValid = MemoryChecksum.VerifyChecksum(obfuscated, checksum);
            if (isValid)
                return MemoryObfuscation.DeobfuscateFloat(obfuscated, valueKey);
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
            int valueKey = MemoryObfuscation.GenerateKey();
            string obfuscated = MemoryObfuscation.ObfuscateString(value, valueKey);
            int checksum = MemoryChecksum.GenerateChecksum(obfuscated);
            PlayerPrefs.SetString(key, obfuscated);
            PlayerPrefs.SetInt(key + ObfuscationKeySuffix, valueKey);
            PlayerPrefs.SetInt(key + ChecksumSuffix, checksum);
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
            if (!PlayerPrefs.HasKey(key))
                return defaultValue;

            int valueKey = PlayerPrefs.GetInt(key + ObfuscationKeySuffix, MemoryObfuscation.FixedKey);
            string obfuscated = PlayerPrefs.GetString(key);
            if (PlayerPrefs.HasKey(key + ChecksumSuffix))
            {
                int checksum = PlayerPrefs.GetInt(key + ChecksumSuffix);
                if (!MemoryChecksum.VerifyChecksum(obfuscated, checksum))
                    return defaultValue;
            }

            return MemoryObfuscation.DeobfuscateString(obfuscated, valueKey);
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
        PlayerPrefs.DeleteKey(key + ChecksumSuffix);
        PlayerPrefs.DeleteKey(key + ObfuscationKeySuffix);
    }

    // Kiểm tra tồn tại
    public static bool HasKey(string key) => PlayerPrefs.HasKey(key);
}
