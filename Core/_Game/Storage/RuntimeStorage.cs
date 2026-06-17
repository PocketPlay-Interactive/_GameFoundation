using System.IO;
using UnityEngine;

public static class RuntimeStorageData
{
    public enum DATATYPE
    {
        NULL,
        SETTING,
        PLAYER
    }

    public enum StatusGame
    {
        Playing,
        Pause
    }

    public static StatusGame _StatusGame = RuntimeStorageData.StatusGame.Playing;
    public static SettingSerializable Setting;
    public static PlayerSerializable Player;

    private static string _path = OptimizeComponent.GetStringOptimize(Application.persistentDataPath, "/");
    private static string _dataSetting = OptimizeComponent.GetStringOptimize(Application.persistentDataPath, "/", HashLib.GetHashStringAndDeviceID(StaticVariable.DATA_SETTING));
    private static string _dataPlayer = OptimizeComponent.GetStringOptimize(Application.persistentDataPath, "/", HashLib.GetHashStringAndDeviceID(StaticVariable.DATA_PLAYER));

    public static bool IsReady
    {
        get
        {
            if (Setting == null || Player == null)
                return false;
            return true;
        }
    }

    public static void ReadData()
    {
        Setting = ReadData<SettingSerializable>(DATATYPE.SETTING) as SettingSerializable;
        if (Setting == null)
        {
            Setting = ReadNew<SettingSerializable>(DATATYPE.SETTING) as SettingSerializable;
            Debug.LogWarning("Setting data was null, created new default setting.");
        }
        Player = ReadData<PlayerSerializable>(DATATYPE.PLAYER) as PlayerSerializable;
        if (Player == null)
        {
            Player = ReadNew<PlayerSerializable>(DATATYPE.PLAYER) as PlayerSerializable;
            Debug.LogWarning("Player data was null, created new default player data.");
        }
        LogSystem.LogSuccess("LOAD ALL DATA IN GAME SUCCESS");
    }

    public static void SaveAllData()
    {
        SaveData(_dataSetting, Setting);
        SaveData(_dataPlayer, Player);
        LogSystem.LogSuccess("SAVE ALL DATA IN GAME SUCCESS");
    }

    public static bool CanShowAds()
    {
        if (Player == null) return true;
        return Player.CanShowAds();
    }

    public static T ReadData<T>(DATATYPE dataType) where T : class, new()
    {
        var dataPath = GetPath(dataType);
        LogSystem.LogSuccess(dataPath);

        if (File.Exists(dataPath))
        {
            try
            {
                var data = ReadDataExist<T>(dataPath);
                return data;
            }
            catch (System.Exception error)
            {
                LogSystem.LogError($"[RuntimeStorage] ReadData Error: {error.Message}");
                var data = GetDataDefault<T>(dataType);
                return data;
            }
        }
        else
        {
            var data = GetDataDefault<T>(dataType);
            return data;
        }
    }

    public static T ReadNew<T>(DATATYPE dataType) where T : class, new()
    {
        var data = GetDataDefault<T>(dataType);
        return data;
    }

    public static void DeleteData(DATATYPE dataType)
    {
        var dataPath = GetPath(dataType);

        if (File.Exists(dataPath))
        {
            File.Delete(dataPath);
            LogSystem.LogSuccess($"Delete {dataPath} success!");
        }
        else
        {
            LogSystem.LogError($"Can't delete {dataPath} because it's not found!");
        }
    }

    public static void DeleteAllData()
    {
        string[] paths = Directory.GetFiles(_path);
        foreach (var path in paths)
        {
            File.Delete(path);
            LogSystem.LogSuccess($"Deleted {path} successfully!");
        }
    }

    private static T GetDataDefault<T>(DATATYPE dataType) where T : class
    {
        try
        {
            // Debug.Log($"Khởi tạo data {dataType} mới");
            switch (dataType)
            {
                case DATATYPE.SETTING:
                    var settingData = new SettingSerializable();
                    return settingData as T;
                case DATATYPE.PLAYER:
                    var playerData = new PlayerSerializable();
                    return playerData as T;
                default:
                    return null;
            }
        }
        catch (System.Exception ex)
        {
            // Debug.Log(ex.Message);
            LogSystem.LogError($"[RuntimeStorage] GetDataDefault Error: {ex.Message}");
        }
        return null;
    }

    private static void SaveData<T>(string path, T data)
    {
        if (data == null) return;
        string _data = JsonUtility.ToJson(data);
        if (_data == null || _data == "" || _data == "{}") return;

        // LogSystem.LogSuccess(_data);

        _data = HashLib.Base64Encode(_data);
        var encodeMD5 = HashLib.EncryptAndDeviceID(_data);
        File.WriteAllText(path, encodeMD5);
    }

    private static T ReadDataExist<T>(string path) where T : class
    {
        try
        {
            // Debug.Log($"Đọc data cũ");
            FileStream fs = new FileStream(path, FileMode.Open);
            StreamReader sr = new StreamReader(fs);
            string _data = sr.ReadToEnd();
            var decodeMD5 = HashLib.DecryptAndDeviceID(_data);
            _data = HashLib.Base64Decode(decodeMD5);

            var data = JsonUtility.FromJson<T>(_data);
            fs.Flush();
            fs.Close();
            return data;
        }
        catch (System.Exception ex)
        {
            LogSystem.LogError($"[RuntimeStorage] ReadDataExist Error: {ex.Message}");
            // Debug.Log(ex.Message);
        }
        return null;
    }

    private static string GetPath(DATATYPE dataType)
    {
        string dataPath = "";

        switch (dataType)
        {
            case DATATYPE.SETTING:
                dataPath = _dataSetting;
                break;
            case DATATYPE.PLAYER:
                dataPath = _dataPlayer;
                break;
            default:
                break;
        }

        //LogSystem.LogSuccess(OptimizeComponent.GetStringOptimize("Load ", dataPath));

        return dataPath;
    }
}
