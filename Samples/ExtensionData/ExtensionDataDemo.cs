using System;
using System.Collections;
using UnityEngine;

public class ExtensionDataDemo : MonoBehaviour
{
    private const string PlayerNameKey = "extension_data_demo_player_name";
    private const string ScoreKey = "extension_data_demo_score";
    private const string MoveSpeedKey = "extension_data_demo_move_speed";
    private const string IsVipKey = "extension_data_demo_is_vip";
    private const string PlayerProfileJsonKey = "extension_data_demo_player_profile_json";

    [Serializable]
    private class PlayerProfileData
    {
        public string playerName;
        public int level;
        public float health;
        public bool hasCompletedTutorial;
    }

    private IEnumerator Start()
    {
        yield return new WaitUntil(() => RuntimeStorageData.IsReady);

        SaveDemoData();
        LoadAndLogDemoData();
        SaveJsonDemoData();
        LoadAndLogJsonDemoData();
    }

    private static void SaveDemoData()
    {
        ExtensionDataClass extensionData = RuntimeStorageData.Player.ExtensionData;

        extensionData.Set(PlayerNameKey, "Player One");
        extensionData.SetInt(ScoreKey, 1234);
        extensionData.SetFloat(MoveSpeedKey, 2.5f);
        extensionData.SetBool(IsVipKey, true);

        RuntimeStorageData.SaveAllData();
        Debug.Log("[ExtensionDataDemo] Saved string, int, float and bool values.");
    }

    private static void LoadAndLogDemoData()
    {
        ExtensionDataClass extensionData = RuntimeStorageData.Player.ExtensionData;

        string playerName = extensionData.Get(PlayerNameKey, "Unknown Player");
        int score = extensionData.GetInt(ScoreKey, 0);
        float moveSpeed = extensionData.GetFloat(MoveSpeedKey, 0f);
        bool isVip = extensionData.GetBool(IsVipKey, false);

        Debug.Log(
            $"[ExtensionDataDemo] Loaded values - PlayerName: {playerName}, " +
            $"Score: {score}, MoveSpeed: {moveSpeed}, IsVip: {isVip}");
    }

    private static void SaveJsonDemoData()
    {
        var playerProfile = new PlayerProfileData
        {
            playerName = "Player One",
            level = 10,
            health = 87.5f,
            hasCompletedTutorial = true
        };

        string json = JsonUtility.ToJson(playerProfile);
        RuntimeStorageData.Player.ExtensionData.Set(PlayerProfileJsonKey, json);
        RuntimeStorageData.SaveAllData();

        Debug.Log($"[ExtensionDataDemo] Saved JSON: {json}");
    }

    private static void LoadAndLogJsonDemoData()
    {
        string json = RuntimeStorageData.Player.ExtensionData.Get(PlayerProfileJsonKey);
        PlayerProfileData playerProfile = string.IsNullOrEmpty(json)
            ? null
            : JsonUtility.FromJson<PlayerProfileData>(json);

        if (playerProfile != null)
        {
            Debug.Log(
                $"[ExtensionDataDemo] Loaded JSON - PlayerName: {playerProfile.playerName}, " +
                $"Level: {playerProfile.level}, Health: {playerProfile.health}, " +
                $"HasCompletedTutorial: {playerProfile.hasCompletedTutorial}");
        }
    }
}
