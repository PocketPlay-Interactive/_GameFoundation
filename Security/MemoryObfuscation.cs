using UnityEngine;

public static class MemoryObfuscation
{
    public const int FixedKey = 0x1A2B3C4D;

    public static int ObfuscateInt(int value)
    {
        return value ^ FixedKey;
    }

    public static int DeobfuscateInt(int obfuscatedValue)
    {
        return obfuscatedValue ^ FixedKey;
    }

    public static int ObfuscateFloat(float value)
    {
        int intValue = System.BitConverter.ToInt32(System.BitConverter.GetBytes(value), 0);
        return intValue ^ FixedKey;
    }

    public static float DeobfuscateFloat(int obfuscatedValue)
    {
        int intValue = obfuscatedValue ^ FixedKey;
        return System.BitConverter.ToSingle(System.BitConverter.GetBytes(intValue), 0);
    }

    public static string ObfuscateString(string value)
    {
        if (string.IsNullOrEmpty(value)) return value;
        char[] chars = value.ToCharArray();
        for (int i = 0; i < chars.Length; i++)
        {
            chars[i] = (char)(chars[i] ^ (FixedKey + i));
        }
        return new string(chars);
    }

    public static string DeobfuscateString(string obfuscated)
    {
        // XOR lại với cùng key sẽ trả về chuỗi gốc
        return ObfuscateString(obfuscated);
    }

    public static bool CompareObfuscatedString(string obfuscatedA, string obfuscatedB)
    {
        string a = DeobfuscateString(obfuscatedA);
        string b = DeobfuscateString(obfuscatedB);
        return a == b;
    }

    public static bool CompareStringWithObfuscated(string real, string obfuscated)
    {
        string decoded = DeobfuscateString(obfuscated);
        return real == decoded;
    }

    /*
    ===========================
    HƯỚNG DẪN SỬ DỤNG
    ===========================
    1. Che giấu giá trị int:
        int realValue = 100;
        int obfuscated = MemoryObfuscation.ObfuscateInt(realValue);
        // Lưu obfuscated

    2. Lấy lại giá trị int:
        int restored = MemoryObfuscation.DeobfuscateInt(obfuscated);

    3. Che giấu giá trị float:
        float realFloat = 123.45f;
        int obfuscatedFloat = MemoryObfuscation.ObfuscateFloat(realFloat);
        // Lưu obfuscatedFloat

    4. Lấy lại giá trị float:
        float restoredFloat = MemoryObfuscation.DeobfuscateFloat(obfuscatedFloat);
    */

    // string original = "Hello123";
    // string obfuscated = MemoryObfuscation.ObfuscateString(original, MemoryObfuscation.FixedKey);
    // // Lưu obfuscated

    // string restored = MemoryObfuscation.DeobfuscateString(obfuscated, MemoryObfuscation.FixedKey);
    // // restored == original
}