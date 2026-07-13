using System;
using System.Collections.Generic;
using System.Globalization;
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
        get => ReadSecureInt(goldBalanceObfuscated, goldBalanceChecksum);
        set
        {
            WriteSecureInt(value, out goldBalanceObfuscated, out goldBalanceChecksum);
        }
    }

    // LastLogin
    [SerializeField]
    private string lastLoginObfuscated;
    [SerializeField]
    private int lastLoginChecksum;
    public DateTime LastLogin
    {
        get => ReadSecureDateTime(lastLoginObfuscated, lastLoginChecksum);
        set
        {
            WriteSecureDateTime(value, out lastLoginObfuscated, out lastLoginChecksum);
        }
    }

    // LoginDayCount
    [SerializeField]
    private int loginDayCountObfuscated;
    [SerializeField]
    private int loginDayCountChecksum;
    public int LoginDayCount
    {
        get => ReadSecureInt(loginDayCountObfuscated, loginDayCountChecksum);
        set
        {
            WriteSecureInt(value, out loginDayCountObfuscated, out loginDayCountChecksum);
        }
    }

    // IsInitialLaunch
    [SerializeField]
    private int isInitialLaunchObfuscated;
    [SerializeField]
    private int isInitialLaunchChecksum;
    public bool IsInitialLaunch
    {
        get => ReadSecureInt(isInitialLaunchObfuscated, isInitialLaunchChecksum) != 0;
        set => WriteSecureInt(value ? 1 : 0, out isInitialLaunchObfuscated, out isInitialLaunchChecksum);
    }

    // CurrentLevelIndex
    [SerializeField]
    private int currentLevelIndexObfuscated;
    [SerializeField]
    private int currentLevelIndexChecksum;
    public int CurrentLevelIndex
    {
        get => ReadSecureInt(currentLevelIndexObfuscated, currentLevelIndexChecksum);
        set
        {
            WriteSecureInt(value, out currentLevelIndexObfuscated, out currentLevelIndexChecksum);
        }
    }

    public ExtensionDataClass ExtensionData;

    public PlayerSerializable()
    {
        PlayerId = SystemInfo.deviceUniqueIdentifier;
        GoldBalance = 0;
        SelectedLanguage = "English";
        OwnedProductIds = new List<string>();
        LastLogin = DateTime.MinValue;
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

    private static int ReadSecureInt(int obfuscated, int checksum)
    {
        int value = MemoryObfuscation.DeobfuscateInt(obfuscated);
        return MemoryChecksum.VerifyChecksum(value, checksum) ? value : 0;
    }

    private static void WriteSecureInt(int value, out int obfuscated, out int checksum)
    {
        obfuscated = MemoryObfuscation.ObfuscateInt(value);
        checksum = MemoryChecksum.GenerateChecksum(value);
    }

    private static DateTime ReadSecureDateTime(string obfuscated, int checksum)
    {
        if (string.IsNullOrEmpty(obfuscated) || !MemoryChecksum.VerifyChecksum(obfuscated, checksum))
            return DateTime.MinValue;

        string value = MemoryObfuscation.DeobfuscateString(obfuscated);
        return DateTime.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var dateTime)
            ? dateTime
            : DateTime.MinValue;
    }

    private static void WriteSecureDateTime(DateTime value, out string obfuscated, out int checksum)
    {
        obfuscated = MemoryObfuscation.ObfuscateString(value.ToString("O", CultureInfo.InvariantCulture));
        checksum = MemoryChecksum.GenerateChecksum(obfuscated);
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
        return TryGetIndex(key, out _);
    }

    private void SetString(string key, string value)
    {
        string obfuscated = MemoryObfuscation.ObfuscateString(value);
        SetRaw(key, obfuscated, MemoryChecksum.GenerateChecksum(obfuscated).ToString());
    }

    private string GetString(string key, string defaultValue = null)
    {
        if (!TryGetIndex(key, out int idx))
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
        if (!TryGetSecureInt(key, out int obfuscated, out int checksum))
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
        if (!TryGetSecureInt(key, out int obfuscated, out int checksum))
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

    private bool TryGetIndex(string key, out int idx)
    {
        idx = keys.IndexOf(key);
        return idx >= 0 && idx < values.Count;
    }

    private bool TryGetSecureInt(string key, out int obfuscated, out int checksum)
    {
        obfuscated = 0;
        checksum = 0;

        return TryGetIndex(key, out int idx) &&
            idx < checksums.Count &&
            int.TryParse(values[idx], out obfuscated) &&
            int.TryParse(checksums[idx], out checksum);
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

