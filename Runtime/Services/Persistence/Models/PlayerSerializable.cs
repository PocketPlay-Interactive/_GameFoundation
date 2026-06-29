using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class PlayerSerializable
{
    // Mã hoá PlayerId
    private string playerIdObfuscated;
    public string PlayerId
    {
        get => MemoryObfuscation.DeobfuscateString(playerIdObfuscated);
        set => playerIdObfuscated = MemoryObfuscation.ObfuscateString(value);
    }

    // Mã hoá SelectedLanguage
    private string selectedLanguageObfuscated;
    public string SelectedLanguage
    {
        get => MemoryObfuscation.DeobfuscateString(selectedLanguageObfuscated);
        set => selectedLanguageObfuscated = MemoryObfuscation.ObfuscateString(value);
    }

    // Mã hoá OwnedProductIds (List<string>)
    private List<string> ownedProductIdsObfuscated = new List<string>();
    public List<string> OwnedProductIds
    {
        get
        {
            var result = new List<string>();
            if (ownedProductIdsObfuscated == null)
                return result;

            foreach (var obf in ownedProductIdsObfuscated)
                result.Add(MemoryObfuscation.DeobfuscateString(obf));
            return result;
        }
        set
        {
            ownedProductIdsObfuscated = new List<string>();
            if (value != null)
                foreach (var s in value)
                    ownedProductIdsObfuscated.Add(MemoryObfuscation.ObfuscateString(s));
        }
    }

    // GoldBalance
    private int goldBalanceObfuscated;
    private int goldBalanceChecksum;
    public int GoldBalance
    {
        get
        {
            int realValue = MemoryObfuscation.DeobfuscateInt(goldBalanceObfuscated);
            if (MemoryChecksum.VerifyChecksum(realValue, goldBalanceChecksum))
                return realValue;
            else
                return 0;
        }
        set
        {
            goldBalanceObfuscated = MemoryObfuscation.ObfuscateInt(value);
            goldBalanceChecksum = MemoryChecksum.GenerateChecksum(value);
        }
    }

    // LastLoginDayOfMonth
    private int lastLoginDayOfMonthObfuscated;
    private int lastLoginDayOfMonthChecksum;
    public int LastLoginDayOfMonth
    {
        get
        {
            int realValue = MemoryObfuscation.DeobfuscateInt(lastLoginDayOfMonthObfuscated);
            if (MemoryChecksum.VerifyChecksum(realValue, lastLoginDayOfMonthChecksum))
                return realValue;
            else
                return 0;
        }
        set
        {
            lastLoginDayOfMonthObfuscated = MemoryObfuscation.ObfuscateInt(value);
            lastLoginDayOfMonthChecksum = MemoryChecksum.GenerateChecksum(value);
        }
    }

    // LoginDayCount
    private int loginDayCountObfuscated;
    private int loginDayCountChecksum;
    public int LoginDayCount
    {
        get
        {
            int realValue = MemoryObfuscation.DeobfuscateInt(loginDayCountObfuscated);
            if (MemoryChecksum.VerifyChecksum(realValue, loginDayCountChecksum))
                return realValue;
            else
                return 0;
        }
        set
        {
            loginDayCountObfuscated = MemoryObfuscation.ObfuscateInt(value);
            loginDayCountChecksum = MemoryChecksum.GenerateChecksum(value);
        }
    }

    // IsInitialLaunch
    private int isInitialLaunchObfuscated;
    private int isInitialLaunchChecksum;
    public bool IsInitialLaunch
    {
        get
        {
            int realValue = MemoryObfuscation.DeobfuscateInt(isInitialLaunchObfuscated);
            if (MemoryChecksum.VerifyChecksum(realValue, isInitialLaunchChecksum))
                return realValue != 0;
            else
                return false;
        }
        set
        {
            int intValue = value ? 1 : 0;
            isInitialLaunchObfuscated = MemoryObfuscation.ObfuscateInt(intValue);
            isInitialLaunchChecksum = MemoryChecksum.GenerateChecksum(intValue);
        }
    }

    // CurrentLevelIndex
    private int currentLevelIndexObfuscated;
    private int currentLevelIndexChecksum;
    public int CurrentLevelIndex
    {
        get
        {
            int realValue = MemoryObfuscation.DeobfuscateInt(currentLevelIndexObfuscated);
            if (MemoryChecksum.VerifyChecksum(realValue, currentLevelIndexChecksum))
                return realValue;
            else
                return 0;
        }
        set
        {
            currentLevelIndexObfuscated = MemoryObfuscation.ObfuscateInt(value);
            currentLevelIndexChecksum = MemoryChecksum.GenerateChecksum(value);
        }
    }

    public ExtensionDataClass ExtensionData;

    public PlayerSerializable()
    {
        PlayerId = SystemInfo.deviceUniqueIdentifier;
        GoldBalance = 0;
        SelectedLanguage = "English";
        OwnedProductIds = new List<string>();
        LastLoginDayOfMonth = 0;
        LoginDayCount = 0;
        IsInitialLaunch = false;
        CurrentLevelIndex = 0;
        ExtensionData = new ExtensionDataClass();
    }

    public bool CanShowAds() =>
        OwnedProductIds == null || OwnedProductIds.Count == 0;

    public bool HasProduct(string productId) =>
        OwnedProductIds != null && OwnedProductIds.Contains(productId);

    public void AddProduct(string productId)
    {
        var pkgs = OwnedProductIds;
        if (pkgs != null && !pkgs.Contains(productId))
        {
            pkgs.Add(productId);
            OwnedProductIds = pkgs;
        }
    }

    public void RemoveAllProducts() =>
        OwnedProductIds = new List<string>();

    public void LogAllProducts()
    {
        var pkgs = OwnedProductIds;
        if (pkgs != null)
        {
            foreach (var p in pkgs)
                Debug.Log($"[InappPurchase] Owned product: {p}");
        }
    }
}

[Serializable]
public class ExtensionDataClass
{
    public List<string> keys = new List<string>();
    public List<string> values = new List<string>();
    public List<string> checksums = new List<string>(); // Lưu checksum cho số

    // Set string (mã hoá)
    public void Set(string key, string value)
    {
        string obfuscated = MemoryObfuscation.ObfuscateString(value);
        string checksum = MemoryChecksum.GenerateChecksum(obfuscated).ToString();
        int idx = keys.IndexOf(key);
        if (idx >= 0)
        {
            EnsureValueSlots(idx);
            values[idx] = obfuscated;
            checksums[idx] = checksum;
        }
        else
        {
            keys.Add(key);
            values.Add(obfuscated);
            checksums.Add(checksum);
        }
    }

    // Get string (giải mã)
    public string Get(string key, string defaultValue = null)
    {
        int idx = keys.IndexOf(key);
        if (idx >= 0 && idx < values.Count)
        {
            if (idx < checksums.Count && !string.IsNullOrEmpty(checksums[idx]))
            {
                if (!int.TryParse(checksums[idx], out int checksum) ||
                    !MemoryChecksum.VerifyChecksum(values[idx], checksum))
                    return defaultValue;
            }

            return MemoryObfuscation.DeobfuscateString(values[idx]);
        }

        return defaultValue;
    }

    // Set int (mã hoá + checksum)
    public void SetInt(string key, int value)
    {
        int obfuscated = MemoryObfuscation.ObfuscateInt(value);
        int checksum = MemoryChecksum.GenerateChecksum(value);
        string obfStr = obfuscated.ToString();
        string checksumStr = checksum.ToString();
        int idx = keys.IndexOf(key);
        if (idx >= 0)
        {
            EnsureValueSlots(idx);
            values[idx] = obfStr;
            checksums[idx] = checksumStr;
        }
        else
        {
            keys.Add(key);
            values.Add(obfStr);
            checksums.Add(checksumStr);
        }
    }

    // Get int (giải mã + kiểm tra)
    public int GetInt(string key, int defaultValue = 0)
    {
        int idx = keys.IndexOf(key);
        if (idx >= 0 && idx < values.Count && idx < checksums.Count)
        {
            if (!int.TryParse(values[idx], out int obfuscated) ||
                !int.TryParse(checksums[idx], out int checksum))
                return defaultValue;

            int realValue = MemoryObfuscation.DeobfuscateInt(obfuscated);
            if (MemoryChecksum.VerifyChecksum(realValue, checksum))
                return realValue;
        }
        return defaultValue;
    }

    // Set float (mã hoá + checksum)
    public void SetFloat(string key, float value)
    {
        int obfuscated = MemoryObfuscation.ObfuscateFloat(value);
        int checksum = MemoryChecksum.GenerateChecksum(obfuscated);
        string obfStr = obfuscated.ToString();
        string checksumStr = checksum.ToString();
        int idx = keys.IndexOf(key);
        if (idx >= 0)
        {
            EnsureValueSlots(idx);
            values[idx] = obfStr;
            checksums[idx] = checksumStr;
        }
        else
        {
            keys.Add(key);
            values.Add(obfStr);
            checksums.Add(checksumStr);
        }
    }

    // Get float (giải mã + kiểm tra)
    public float GetFloat(string key, float defaultValue = 0f)
    {
        int idx = keys.IndexOf(key);
        if (idx >= 0 && idx < values.Count && idx < checksums.Count)
        {
            if (!int.TryParse(values[idx], out int obfuscated) ||
                !int.TryParse(checksums[idx], out int checksum))
                return defaultValue;

            if (MemoryChecksum.VerifyChecksum(obfuscated, checksum))
                return MemoryObfuscation.DeobfuscateFloat(obfuscated);
        }
        return defaultValue;
    }

    // Set bool (mã hoá + checksum)
    public void SetBool(string key, bool value)
    {
        SetInt(key, value ? 1 : 0);
    }

    // Get bool (giải mã + kiểm tra)
    public bool GetBool(string key, bool defaultValue = false)
    {
        return GetInt(key, defaultValue ? 1 : 0) != 0;
    }

    public Dictionary<string, string> ToDictionary()
    {
        var dict = new Dictionary<string, string>();
        int count = Math.Min(keys.Count, values.Count);
        for (int i = 0; i < count; i++)
            dict[keys[i]] = values[i];
        return dict;
    }

    private void EnsureValueSlots(int idx)
    {
        while (values.Count <= idx)
            values.Add("");

        while (checksums.Count <= idx)
            checksums.Add("");
    }
}


// ExtensionData.Set("username", "player1");
// string name = ExtensionData.Get("username");

// ExtensionData.SetInt("score", 1234);
// int score = ExtensionData.GetInt("score");

// ExtensionData.SetFloat("speed", 2.5f);
// float speed = ExtensionData.GetFloat("speed");

// ExtensionData.SetBool("isVip", true);
// bool isVip = ExtensionData.GetBool("isVip");

