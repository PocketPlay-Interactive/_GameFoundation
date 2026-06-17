using System;
using System.Security.Cryptography;

public static class MemoryObfuscation
{
    public const int FixedKey = 0x1A2B3C4D;

    public static int GenerateKey()
    {
        byte[] bytes = new byte[4];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(bytes);
        }

        int key = BitConverter.ToInt32(bytes, 0);
        return key == 0 ? FixedKey : key;
    }

    public static int EncryptInt(int value, int key)
    {
        return ObfuscateInt(value, key);
    }

    public static int DecryptInt(int encryptedValue, int key)
    {
        return DeobfuscateInt(encryptedValue, key);
    }

    public static int ObfuscateInt(int value)
    {
        return ObfuscateInt(value, FixedKey);
    }

    public static int ObfuscateInt(int value, int key)
    {
        return value ^ key;
    }

    public static int DeobfuscateInt(int obfuscatedValue)
    {
        return DeobfuscateInt(obfuscatedValue, FixedKey);
    }

    public static int DeobfuscateInt(int obfuscatedValue, int key)
    {
        return obfuscatedValue ^ key;
    }

    public static int ObfuscateFloat(float value)
    {
        return ObfuscateFloat(value, FixedKey);
    }

    public static int ObfuscateFloat(float value, int key)
    {
        int intValue = BitConverter.ToInt32(BitConverter.GetBytes(value), 0);
        return intValue ^ key;
    }

    public static float DeobfuscateFloat(int obfuscatedValue)
    {
        return DeobfuscateFloat(obfuscatedValue, FixedKey);
    }

    public static float DeobfuscateFloat(int obfuscatedValue, int key)
    {
        int intValue = obfuscatedValue ^ key;
        return BitConverter.ToSingle(BitConverter.GetBytes(intValue), 0);
    }

    public static string ObfuscateString(string value)
    {
        return ObfuscateString(value, FixedKey);
    }

    public static string ObfuscateString(string value, int key)
    {
        if (string.IsNullOrEmpty(value)) return value;

        char[] chars = value.ToCharArray();
        for (int i = 0; i < chars.Length; i++)
        {
            chars[i] = (char)(chars[i] ^ (key + i));
        }

        return new string(chars);
    }

    public static string DeobfuscateString(string obfuscated)
    {
        return DeobfuscateString(obfuscated, FixedKey);
    }

    public static string DeobfuscateString(string obfuscated, int key)
    {
        return ObfuscateString(obfuscated, key);
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
}
