using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using UnityEditor;
using UnityEngine;

public sealed class RuntimeStorageDataWindow : EditorWindow
{
    private const string MenuPath = "GameFoundation/Data Tables/Runtime Storage";

    private Vector2 scrollPosition;
    private StorageView settingView;
    private StorageView playerView;
    private static int tableRowIndex;

    private static readonly Color AccentColor = new Color(0.25f, 0.58f, 0.96f);
    private static readonly Color SuccessColor = new Color(0.25f, 0.72f, 0.46f);

    [MenuItem(MenuPath)]
    private static void Open()
    {
        var window = GetWindow<RuntimeStorageDataWindow>();
        window.titleContent = new GUIContent("Runtime Storage");
        window.minSize = new Vector2(620f, 520f);
        window.Show();
    }

    private void OnEnable()
    {
        Refresh();
    }

    private void OnGUI()
    {
        DrawPageHeader();

        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
        EditorGUILayout.Space(12f);
        DrawSettingStorage(settingView);
        EditorGUILayout.Space(12f);
        DrawPlayerStorage(playerView);
        EditorGUILayout.Space(12f);
        EditorGUILayout.EndScrollView();
    }

    private void DrawPageHeader()
    {
        Rect rect = GUILayoutUtility.GetRect(0f, 92f, GUILayout.ExpandWidth(true));
        EditorGUI.DrawRect(rect, EditorGUIUtility.isProSkin
            ? new Color(0.09f, 0.13f, 0.19f)
            : new Color(0.13f, 0.27f, 0.44f));
        EditorGUI.DrawRect(new Rect(rect.x, rect.yMax - 3f, rect.width, 3f), AccentColor);

        var titleStyle = new GUIStyle(EditorStyles.boldLabel)
        {
            fontSize = 20,
            normal = { textColor = Color.white }
        };
        var subtitleStyle = new GUIStyle(EditorStyles.miniLabel)
        {
            fontSize = 11,
            normal = { textColor = new Color(0.74f, 0.82f, 0.92f) }
        };

        GUI.Label(new Rect(rect.x + 18f, rect.y + 14f, rect.width - 270f, 28f), "Runtime Storage", titleStyle);
        GUI.Label(new Rect(rect.x + 19f, rect.y + 43f, rect.width - 275f, 18f),
            "Inspect decrypted player data saved on this device", subtitleStyle);
        GUI.Label(new Rect(rect.x + 19f, rect.y + 63f, rect.width - 38f, 18f),
            Application.persistentDataPath, subtitleStyle);

        var buttonStyle = new GUIStyle(GUI.skin.button)
        {
            fixedHeight = 28f,
            fontStyle = FontStyle.Bold
        };
        if (GUI.Button(new Rect(rect.xMax - 214f, rect.y + 18f, 88f, 28f), "Refresh", buttonStyle))
            Refresh();

        if (GUI.Button(new Rect(rect.xMax - 116f, rect.y + 18f, 98f, 28f), "Open Folder", buttonStyle))
            EditorUtility.RevealInFinder(Application.persistentDataPath);
    }

    private static bool DrawStorageHeader(string label, StorageView view)
    {
        Rect headerRect = GUILayoutUtility.GetRect(0f, 42f, GUILayout.ExpandWidth(true));
        EditorGUI.DrawRect(headerRect, EditorGUIUtility.isProSkin
            ? new Color(0.16f, 0.18f, 0.22f)
            : new Color(0.88f, 0.92f, 0.97f));
        EditorGUI.DrawRect(new Rect(headerRect.x, headerRect.y, 4f, headerRect.height), AccentColor);

        var sectionStyle = new GUIStyle(EditorStyles.boldLabel) { fontSize = 15 };
        GUI.Label(new Rect(headerRect.x + 14f, headerRect.y + 10f, 180f, 22f), label, sectionStyle);

        string status = view != null && view.Exists ? "SAVED" : "NO DATA";
        Color statusColor = view != null && view.Exists ? SuccessColor : new Color(0.6f, 0.62f, 0.66f);
        var badgeRect = new Rect(headerRect.xMax - 80f, headerRect.y + 10f, 66f, 22f);
        EditorGUI.DrawRect(badgeRect, statusColor);
        var badgeStyle = new GUIStyle(EditorStyles.miniBoldLabel)
        {
            alignment = TextAnchor.MiddleCenter,
            normal = { textColor = Color.white }
        };
        GUI.Label(badgeRect, status, badgeStyle);

        if (view == null || !view.Exists)
        {
            EditorGUILayout.Space(8f);
            EditorGUILayout.HelpBox("No saved file was found.", MessageType.Info);
            return false;
        }

        EditorGUILayout.Space(8f);
        using (new EditorGUILayout.HorizontalScope())
        {
            DrawMetadata("FILE", view.FileName);
            DrawMetadata("LAST SAVED", view.LastWriteTime.ToString("yyyy-MM-dd  HH:mm:ss"));
            DrawMetadata("SIZE", EditorUtility.FormatBytes(view.FileSize), 90f);
        }
        EditorGUILayout.Space(8f);

        if (!string.IsNullOrEmpty(view.Error))
        {
            EditorGUILayout.HelpBox(view.Error, MessageType.Error);
            return false;
        }

        return true;
    }

    private static void DrawSettingStorage(StorageView view)
    {
        using (new EditorGUILayout.VerticalScope(CardStyle))
        {
            if (!DrawStorageHeader("Setting", view))
                return;

            if (view.Setting == null)
            {
                EditorGUILayout.HelpBox("The Setting JSON could not be parsed.", MessageType.Warning);
            }
            else
            {
                DrawTableHeader("KEY", "VALUE");
                int count = Math.Min(view.Setting.keys.Count, view.Setting.values.Count);
                for (int i = 0; i < count; i++)
                    DrawTableRow(view.Setting.keys[i], view.Setting.values[i].ToString());

                if (count == 0)
                    DrawEmptyState("No setting entries");
            }

            DrawRawJson(view);
        }
    }

    private static void DrawPlayerStorage(StorageView view)
    {
        using (new EditorGUILayout.VerticalScope(CardStyle))
        {
            if (!DrawStorageHeader("Player", view))
                return;

            if (view.Player == null)
            {
                EditorGUILayout.HelpBox("The Player JSON could not be parsed.", MessageType.Warning);
                DrawRawJson(view);
                return;
            }

            DrawSubsectionTitle("PLAYER FIELDS");
            DrawTableHeader("FIELD", "VALUE");
            DrawTableRow("PlayerId", view.Player.PlayerId);
            DrawTableRow("SelectedLanguage", view.Player.SelectedLanguage);
            DrawTableRow("GoldBalance", view.Player.GoldBalance.ToString());
            DrawTableRow("LastLoginDayOfMonth", view.Player.LastLoginDayOfMonth.ToString());
            DrawTableRow("LoginDayCount", view.Player.LoginDayCount.ToString());
            DrawTableRow("IsInitialLaunch", view.Player.IsInitialLaunch.ToString());
            DrawTableRow("CurrentLevelIndex", view.Player.CurrentLevelIndex.ToString());
            DrawTableRow("CanShowAds", view.Player.CanShowAds().ToString());

            DrawSubsectionTitle("OWNED PRODUCTS");
            DrawTableHeader("INDEX", "PRODUCT ID");
            var productIds = view.Player.OwnedProductIds;
            if (productIds == null || productIds.Count == 0)
            {
                DrawEmptyState("No owned products");
            }
            else
            {
                for (int i = 0; i < productIds.Count; i++)
                    DrawTableRow(i.ToString(), productIds[i]);
            }

            DrawSubsectionTitle("EXTENSION DATA");
            DrawTableHeader("KEY", "VALUE");
            DrawExtensionData(view.Player.ExtensionData);
            DrawRawJson(view);
        }
    }

    private static void DrawExtensionData(ExtensionDataClass extensionData)
    {
        if (extensionData == null || extensionData.keys == null || extensionData.keys.Count == 0)
        {
            DrawEmptyState("No extension entries");
            return;
        }

        for (int i = 0; i < extensionData.keys.Count; i++)
            DrawTableRow(extensionData.keys[i], DecodeExtensionValue(extensionData, i));
    }

    private static string DecodeExtensionValue(ExtensionDataClass extensionData, int index)
    {
        if (extensionData.values == null || index >= extensionData.values.Count)
            return "<missing value>";

        string rawValue = extensionData.values[index];
        if (extensionData.checksums == null || index >= extensionData.checksums.Count ||
            !int.TryParse(extensionData.checksums[index], out int checksum))
            return rawValue;

        if (MemoryChecksum.VerifyChecksum(rawValue, checksum))
            return MemoryObfuscation.DeobfuscateString(rawValue);

        if (!int.TryParse(rawValue, out int obfuscatedNumber))
            return "<invalid checksum>";

        int intValue = MemoryObfuscation.DeobfuscateInt(obfuscatedNumber);
        if (MemoryChecksum.VerifyChecksum(intValue, checksum))
            return intValue == 0 || intValue == 1
                ? $"{intValue} / {(intValue != 0).ToString().ToLowerInvariant()}"
                : intValue.ToString();

        if (MemoryChecksum.VerifyChecksum(obfuscatedNumber, checksum))
            return MemoryObfuscation.DeobfuscateFloat(obfuscatedNumber)
                .ToString("R", CultureInfo.InvariantCulture);

        return "<invalid checksum>";
    }

    private static void DrawTableHeader(string left, string right)
    {
        tableRowIndex = 0;
        Rect rect = GUILayoutUtility.GetRect(0f, 27f, GUILayout.ExpandWidth(true));
        EditorGUI.DrawRect(rect, EditorGUIUtility.isProSkin
            ? new Color(0.20f, 0.22f, 0.26f)
            : new Color(0.78f, 0.84f, 0.91f));

        var style = new GUIStyle(EditorStyles.miniBoldLabel)
        {
            alignment = TextAnchor.MiddleLeft
        };
        GUI.Label(new Rect(rect.x + 10f, rect.y, 174f, rect.height), left, style);
        GUI.Label(new Rect(rect.x + 194f, rect.y, rect.width - 204f, rect.height), right, style);
    }

    private static void DrawTableRow(string key, string value)
    {
        Rect rect = GUILayoutUtility.GetRect(0f, 28f, GUILayout.ExpandWidth(true));
        bool alternate = tableRowIndex++ % 2 != 0;
        Color rowColor = EditorGUIUtility.isProSkin
            ? (alternate ? new Color(0.145f, 0.15f, 0.17f) : new Color(0.17f, 0.18f, 0.20f))
            : (alternate ? new Color(0.94f, 0.95f, 0.97f) : new Color(0.98f, 0.98f, 0.99f));
        EditorGUI.DrawRect(rect, rowColor);
        EditorGUI.DrawRect(new Rect(rect.x + 184f, rect.y + 4f, 1f, rect.height - 8f),
            EditorGUIUtility.isProSkin ? new Color(0.28f, 0.29f, 0.31f) : new Color(0.82f, 0.84f, 0.87f));

        var keyStyle = new GUIStyle(EditorStyles.label)
        {
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleLeft,
            clipping = TextClipping.Clip
        };
        var valueStyle = new GUIStyle(EditorStyles.label)
        {
            alignment = TextAnchor.MiddleLeft,
            clipping = TextClipping.Clip
        };
        EditorGUI.SelectableLabel(new Rect(rect.x + 10f, rect.y + 4f, 164f, 20f), key ?? string.Empty, keyStyle);
        EditorGUI.SelectableLabel(new Rect(rect.x + 195f, rect.y + 4f, rect.width - 205f, 20f), value ?? "null", valueStyle);
    }

    private static void DrawRawJson(StorageView view)
    {
        EditorGUILayout.Space(12f);
        using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
        {
            view.ShowRawJson = EditorGUILayout.Foldout(view.ShowRawJson, "Raw decrypted JSON", true);
            GUILayout.FlexibleSpace();
            using (new EditorGUI.DisabledScope(!view.ShowRawJson))
            {
                if (GUILayout.Button("Copy JSON", EditorStyles.toolbarButton, GUILayout.Width(76f)))
                    EditorGUIUtility.systemCopyBuffer = view.Json;
            }
        }

        if (!view.ShowRawJson)
            return;

        var textStyle = new GUIStyle(EditorStyles.textArea)
        {
            wordWrap = true,
            padding = new RectOffset(10, 10, 8, 8)
        };
        EditorGUILayout.TextArea(view.Json, textStyle, GUILayout.MinHeight(140f));
    }

    private static void DrawSubsectionTitle(string title)
    {
        EditorGUILayout.Space(12f);
        Rect rect = GUILayoutUtility.GetRect(0f, 20f, GUILayout.ExpandWidth(true));
        EditorGUI.DrawRect(new Rect(rect.x, rect.y + 9f, 3f, 11f), AccentColor);
        var style = new GUIStyle(EditorStyles.miniBoldLabel)
        {
            normal = { textColor = AccentColor }
        };
        GUI.Label(new Rect(rect.x + 9f, rect.y, rect.width - 9f, rect.height), title, style);
    }

    private static void DrawEmptyState(string message)
    {
        Rect rect = GUILayoutUtility.GetRect(0f, 34f, GUILayout.ExpandWidth(true));
        EditorGUI.DrawRect(rect, EditorGUIUtility.isProSkin
            ? new Color(0.14f, 0.15f, 0.17f)
            : new Color(0.96f, 0.97f, 0.98f));
        var style = new GUIStyle(EditorStyles.centeredGreyMiniLabel)
        {
            alignment = TextAnchor.MiddleCenter,
            fontStyle = FontStyle.Italic
        };
        GUI.Label(rect, message, style);
    }

    private static void DrawMetadata(string label, string value, float width = 0f)
    {
        var style = new GUIStyle(EditorStyles.miniLabel)
        {
            richText = true,
            clipping = TextClipping.Clip
        };
        string text = $"<b>{label}</b>   {value}";
        if (width > 0f)
            GUILayout.Label(text, style, GUILayout.Width(width));
        else
            GUILayout.Label(text, style, GUILayout.ExpandWidth(true));
    }

    private static GUIStyle CardStyle
    {
        get
        {
            var style = new GUIStyle(EditorStyles.helpBox)
            {
                padding = new RectOffset(12, 12, 12, 12),
                margin = new RectOffset(12, 12, 0, 0)
            };
            return style;
        }
    }

    private void Refresh()
    {
        settingView = ReadStorage(StaticVariable.DATA_SETTING);
        playerView = ReadStorage(StaticVariable.DATA_PLAYER);
        Repaint();
    }

    private static StorageView ReadStorage(string dataName)
    {
        var fileName = HashLib.GetHashStringAndDeviceID(dataName);
        var path = Path.Combine(Application.persistentDataPath, fileName);
        var view = new StorageView
        {
            FileName = fileName,
            Exists = File.Exists(path)
        };

        if (!view.Exists)
            return view;

        try
        {
            var fileInfo = new FileInfo(path);
            view.FileSize = fileInfo.Length;
            view.LastWriteTime = fileInfo.LastWriteTime;

            var encryptedData = File.ReadAllText(path);
            var base64Data = HashLib.DecryptAndDeviceID(encryptedData);
            view.Json = HashLib.Base64Decode(base64Data);

            if (dataName == StaticVariable.DATA_SETTING)
                view.Setting = JsonUtility.FromJson<SettingView>(view.Json);
            else if (dataName == StaticVariable.DATA_PLAYER)
                view.Player = JsonUtility.FromJson<PlayerSerializable>(view.Json);
        }
        catch (Exception exception)
        {
            view.Error = $"Could not decrypt this file.\n{exception.Message}";
        }

        return view;
    }

    private sealed class StorageView
    {
        public bool Exists;
        public string FileName;
        public long FileSize;
        public DateTime LastWriteTime;
        public string Json;
        public string Error;
        public SettingView Setting;
        public PlayerSerializable Player;
        public bool ShowRawJson;
    }

    [Serializable]
    private sealed class SettingView
    {
        public List<string> keys = new List<string>();
        public List<bool> values = new List<bool>();
    }
}
