using System;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

public static class MemoryChecksum
{
    public const int FixedKey = 0xABCDEF;

    private const string Salt = "GameFoundation.Security.MemoryChecksum.v2";

    public static int GenerateChecksum(int value)
    {
        return GenerateHmacChecksum(value.ToString());
    }

    public static int GenerateChecksum(string value)
    {
        return GenerateHmacChecksum(value ?? string.Empty);
    }

    public static bool VerifyChecksum(int value, int checksum)
    {
        return GenerateChecksum(value) == checksum || GenerateLegacyChecksum(value) == checksum;
    }

    public static bool VerifyChecksum(string value, int checksum)
    {
        return GenerateChecksum(value) == checksum;
    }

    private static int GenerateHmacChecksum(string value)
    {
        byte[] keyBytes = Encoding.UTF8.GetBytes(GetDeviceBoundSecret());
        byte[] dataBytes = Encoding.UTF8.GetBytes(value);

        using (var hmac = new HMACSHA256(keyBytes))
        {
            byte[] hash = hmac.ComputeHash(dataBytes);
            return BitConverter.ToInt32(hash, 0);
        }
    }

    private static int GenerateLegacyChecksum(int value)
    {
        return (value ^ FixedKey) + 0xABCDEF;
    }

    private static string GetDeviceBoundSecret()
    {
        string deviceId = SystemInfo.deviceUniqueIdentifier;
        string appId = Application.identifier;
        return Salt + "|" + appId + "|" + deviceId;
    }
}
