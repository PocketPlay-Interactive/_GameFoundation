using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class PlayerSerializable
{
    // Mã hoá PlayerId
    [SerializeField]
    private string playerIdObfuscated;
    public string PlayerId
    {
        get => MemoryObfuscation.DeobfuscateString(playerIdObfuscated);
        set => playerIdObfuscated = MemoryObfuscation.ObfuscateString(value);
    }

    // Mã hoá SelectedLanguage
    [SerializeField]
    private string selectedLanguageObfuscated;
    public string SelectedLanguage
    {
        get => MemoryObfuscation.DeobfuscateString(selectedLanguageObfuscated);
        set => selectedLanguageObfuscated = MemoryObfuscation.ObfuscateString(value);
    }

    // Mã hoá OwnedProductIds (List<string>)
    [SerializeField]
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
    [SerializeField]
    private int goldBalanceObfuscated;
    [SerializeField]
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
    [SerializeField]
    private int lastLoginDayOfMonthObfuscated;
    [SerializeField]
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
    [SerializeField]
    private int loginDayCountObfuscated;
    [SerializeField]
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
    [SerializeField]
    private int isInitialLaunchObfuscated;
    [SerializeField]
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
    [SerializeField]
    private int currentLevelIndexObfuscated;
    [SerializeField]
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

    public void Set<T>(string key, T value)
    {
        EnsureLists();

        if (typeof(T) == typeof(string))
        {
            SetString(key, (object)value as string);
            return;
        }

        switch (value)
        {
            case int i:
                SetIntValue(key, i);
                break;
            case float f:
                SetFloatValue(key, f);
                break;
            case bool b:
                SetIntValue(key, b ? 1 : 0);
                break;
            default:
                SetString(key, JsonUtility.ToJson(new ValueBox<T> { value = value }));
                break;
        }
    }

    public T Get<T>(string key, T defaultValue = default(T))
    {
        EnsureLists();

        Type type = typeof(T);

        if (type == typeof(string))
            return (T)(object)GetString(key, (object)defaultValue as string);

        if (type == typeof(int))
            return (T)(object)GetIntValue(key, defaultValue is int intValue ? intValue : 0);

        if (type == typeof(float))
            return (T)(object)GetFloatValue(key, defaultValue is float floatValue ? floatValue : 0f);

        if (type == typeof(bool))
            return (T)(object)(GetIntValue(key, defaultValue is bool boolValue && boolValue ? 1 : 0) != 0);

        string json = GetString(key);
        if (string.IsNullOrEmpty(json))
            return defaultValue;

        try
        {
            if (json.Contains("\"value\""))
                return JsonUtility.FromJson<ValueBox<T>>(json).value;

            return JsonUtility.FromJson<T>(json);
        }
        catch
        {
            return defaultValue;
        }
    }

    public Dictionary<string, string> ToDictionary()
    {
        EnsureLists();

        var dict = new Dictionary<string, string>();
        int count = Math.Min(keys.Count, values.Count);
        for (int i = 0; i < count; i++)
            dict[keys[i]] = values[i];
        return dict;
    }

    public bool HasKey(string key)
    {
        EnsureLists();

        int idx = keys.IndexOf(key);
        return idx >= 0 && idx < values.Count;
    }

    private void SetString(string key, string value)
    {
        string obfuscated = MemoryObfuscation.ObfuscateString(value);
        SetRaw(key, obfuscated, MemoryChecksum.GenerateChecksum(obfuscated).ToString());
    }

    private string GetString(string key, string defaultValue = null)
    {
        int idx = keys.IndexOf(key);
        if (idx < 0 || idx >= values.Count)
            return defaultValue;

        if (idx < checksums.Count && !string.IsNullOrEmpty(checksums[idx]))
        {
            if (!int.TryParse(checksums[idx], out int checksum) ||
                !MemoryChecksum.VerifyChecksum(values[idx], checksum))
                return defaultValue;
        }

        return MemoryObfuscation.DeobfuscateString(values[idx]);
    }

    private void SetIntValue(string key, int value)
    {
        int obfuscated = MemoryObfuscation.ObfuscateInt(value);
        SetRaw(key, obfuscated.ToString(), MemoryChecksum.GenerateChecksum(value).ToString());
    }

    private int GetIntValue(string key, int defaultValue = 0)
    {
        int idx = keys.IndexOf(key);
        if (idx < 0 || idx >= values.Count || idx >= checksums.Count)
            return defaultValue;

        if (!int.TryParse(values[idx], out int obfuscated) ||
            !int.TryParse(checksums[idx], out int checksum))
            return defaultValue;

        int realValue = MemoryObfuscation.DeobfuscateInt(obfuscated);
        return MemoryChecksum.VerifyChecksum(realValue, checksum) ? realValue : defaultValue;
    }

    private void SetFloatValue(string key, float value)
    {
        int obfuscated = MemoryObfuscation.ObfuscateFloat(value);
        SetRaw(key, obfuscated.ToString(), MemoryChecksum.GenerateChecksum(obfuscated).ToString());
    }

    private float GetFloatValue(string key, float defaultValue = 0f)
    {
        int idx = keys.IndexOf(key);
        if (idx < 0 || idx >= values.Count || idx >= checksums.Count)
            return defaultValue;

        if (!int.TryParse(values[idx], out int obfuscated) ||
            !int.TryParse(checksums[idx], out int checksum))
            return defaultValue;

        return MemoryChecksum.VerifyChecksum(obfuscated, checksum)
            ? MemoryObfuscation.DeobfuscateFloat(obfuscated)
            : defaultValue;
    }

    private void SetRaw(string key, string value, string checksum)
    {
        int idx = keys.IndexOf(key);
        if (idx >= 0)
        {
            EnsureValueSlots(idx);
            values[idx] = value;
            checksums[idx] = checksum;
            return;
        }

        keys.Add(key);
        values.Add(value);
        checksums.Add(checksum);
    }

    private void EnsureValueSlots(int idx)
    {
        while (values.Count <= idx)
            values.Add("");

        while (checksums.Count <= idx)
            checksums.Add("");
    }

    private void EnsureLists()
    {
        if (keys == null) keys = new List<string>();
        if (values == null) values = new List<string>();
        if (checksums == null) checksums = new List<string>();
    }

    [Serializable]
    private class ValueBox<T>
    {
        public T value;
    }
}


// ExtensionData.Set("username", "player1");
// string name = ExtensionData.Get<string>("username");

// ExtensionData.Set("score", 1234);
// int score = ExtensionData.Get<int>("score");

// ExtensionData.Set("speed", 2.5f);
// float speed = ExtensionData.Get<float>("speed");

// ExtensionData.Set("isVip", true);
// bool isVip = ExtensionData.Get<bool>("isVip");

