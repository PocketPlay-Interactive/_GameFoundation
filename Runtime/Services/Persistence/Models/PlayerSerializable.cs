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

    // Mã hoá PreferredLanguage
    private string preferredLanguageObfuscated;
    public string PreferredLanguage
    {
        get => MemoryObfuscation.DeobfuscateString(preferredLanguageObfuscated);
        set => preferredLanguageObfuscated = MemoryObfuscation.ObfuscateString(value);
    }

    // Mã hoá OwnedPackages (List<string>)
    private List<string> ownedPackagesObfuscated = new List<string>();
    public List<string> OwnedPackages
    {
        get
        {
            var result = new List<string>();
            if (ownedPackagesObfuscated == null)
                return result;

            foreach (var obf in ownedPackagesObfuscated)
                result.Add(MemoryObfuscation.DeobfuscateString(obf));
            return result;
        }
        set
        {
            ownedPackagesObfuscated = new List<string>();
            if (value != null)
                foreach (var s in value)
                    ownedPackagesObfuscated.Add(MemoryObfuscation.ObfuscateString(s));
        }
    }

    // GoldAmount
    private int GoldAmountObfuscated;
    private int GoldAmountChecksum;
    public int GoldAmount
    {
        get
        {
            int realValue = MemoryObfuscation.DeobfuscateInt(GoldAmountObfuscated);
            if (MemoryChecksum.VerifyChecksum(realValue, GoldAmountChecksum))
                return realValue;
            else
                return 0;
        }
        set
        {
            GoldAmountObfuscated = MemoryObfuscation.ObfuscateInt(value);
            GoldAmountChecksum = MemoryChecksum.GenerateChecksum(value);
        }
    }

    // LastLoginDay
    private int LastLoginDayObfuscated;
    private int LastLoginDayChecksum;
    public int LastLoginDay
    {
        get
        {
            int realValue = MemoryObfuscation.DeobfuscateInt(LastLoginDayObfuscated);
            if (MemoryChecksum.VerifyChecksum(realValue, LastLoginDayChecksum))
                return realValue;
            else
                return 0;
        }
        set
        {
            LastLoginDayObfuscated = MemoryObfuscation.ObfuscateInt(value);
            LastLoginDayChecksum = MemoryChecksum.GenerateChecksum(value);
        }
    }

    // TotalLoginDays
    private int TotalLoginDaysObfuscated;
    private int TotalLoginDaysChecksum;
    public int TotalLoginDays
    {
        get
        {
            int realValue = MemoryObfuscation.DeobfuscateInt(TotalLoginDaysObfuscated);
            if (MemoryChecksum.VerifyChecksum(realValue, TotalLoginDaysChecksum))
                return realValue;
            else
                return 0;
        }
        set
        {
            TotalLoginDaysObfuscated = MemoryObfuscation.ObfuscateInt(value);
            TotalLoginDaysChecksum = MemoryChecksum.GenerateChecksum(value);
        }
    }

    // IsFirstLaunch
    private int IsFirstLaunchObfuscated;
    private int IsFirstLaunchChecksum;
    public bool IsFirstLaunch
    {
        get
        {
            int realValue = MemoryObfuscation.DeobfuscateInt(IsFirstLaunchObfuscated);
            if (MemoryChecksum.VerifyChecksum(realValue, IsFirstLaunchChecksum))
                return realValue != 0;
            else
                return false;
        }
        set
        {
            int intValue = value ? 1 : 0;
            IsFirstLaunchObfuscated = MemoryObfuscation.ObfuscateInt(intValue);
            IsFirstLaunchChecksum = MemoryChecksum.GenerateChecksum(intValue);
        }
    }

    // CurrentLevel
    private int CurrentLevelObfuscated;
    private int CurrentLevelChecksum;
    public int CurrentLevel
    {
        get
        {
            int realValue = MemoryObfuscation.DeobfuscateInt(CurrentLevelObfuscated);
            if (MemoryChecksum.VerifyChecksum(realValue, CurrentLevelChecksum))
                return realValue;
            else
                return 0;
        }
        set
        {
            CurrentLevelObfuscated = MemoryObfuscation.ObfuscateInt(value);
            CurrentLevelChecksum = MemoryChecksum.GenerateChecksum(value);
        }
    }

    public ExtensionDataClass ExtensionData;

    public PlayerSerializable()
    {
        PlayerId = SystemInfo.deviceUniqueIdentifier;
        GoldAmount = 0;
        PreferredLanguage = "English";
        OwnedPackages = new List<string>();
        LastLoginDay = 0;
        TotalLoginDays = 0;
        IsFirstLaunch = false;
        CurrentLevel = 0;
        ExtensionData = new ExtensionDataClass();
    }

    public bool CanShowAds() =>
        OwnedPackages == null || OwnedPackages.Count == 0;

    public bool HasProduct(string productId) =>
        OwnedPackages != null && OwnedPackages.Contains(productId);

    public void AddProduct(string productId)
    {
        var pkgs = OwnedPackages;
        if (pkgs != null && !pkgs.Contains(productId))
        {
            pkgs.Add(productId);
            OwnedPackages = pkgs;
        }
    }

    public void RemoveAllProducts() =>
        OwnedPackages = new List<string>();

    public void LogAllProducts()
    {
        var pkgs = OwnedPackages;
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

