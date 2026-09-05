using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
#if UNITY_2021_2_OR_NEWER
using UnityEditor.Build;
#endif
using UnityEngine;

public class GameFoundationDefineSymbolsWindow : EditorWindow
{
    private static readonly DefineOption[] DefineOptions =
    {
        new DefineOption("IAPPURCHASE_ENABLE", "Unity IAP"),
    };

    private readonly Dictionary<string, bool> states = new Dictionary<string, bool>();
    private Vector2 scrollPosition;
    private string customDefine = "";

    private static readonly Color AccentColor = new Color(0.25f, 0.58f, 0.96f);
    private static readonly Color EnabledColor = new Color(0.25f, 0.72f, 0.46f);
    private static readonly Color DisabledColor = new Color(0.55f, 0.57f, 0.62f);

    [MenuItem("GameFoundation/Configuration/SDK Defines")]
    public static void ShowWindow()
    {
        var window = GetWindow<GameFoundationDefineSymbolsWindow>();
        window.titleContent = new GUIContent("SDK Defines");

        window.minSize = new Vector2(620, 520);
    }

    private void OnEnable()
    {
        RefreshStates();
    }

    private void OnGUI()
    {
        DrawHeader();

        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
        DrawPresetDefines();
        DrawCustomDefineTools();
        DrawCurrentDefines();
        EditorGUILayout.EndScrollView();

        DrawFooter();
    }

    private void DrawHeader()
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

        GUI.Label(new Rect(rect.x + 18f, rect.y + 14f, rect.width - 240f, 28f), "SDK Defines", titleStyle);
        GUI.Label(new Rect(rect.x + 19f, rect.y + 43f, rect.width - 260f, 18f),
            "Toggle optional GameFoundation SDK integrations for the selected build target", subtitleStyle);
        GUI.Label(new Rect(rect.x + 19f, rect.y + 63f, rect.width - 38f, 18f),
            "Selected Target: " + GetSelectedTargetName(), subtitleStyle);

        var buttonStyle = new GUIStyle(GUI.skin.button)
        {
            fixedHeight = 28f,
            fontStyle = FontStyle.Bold
        };
        if (GUI.Button(new Rect(rect.xMax - 214f, rect.y + 18f, 88f, 28f), "Refresh", buttonStyle))
            RefreshStates();

        if (GUI.Button(new Rect(rect.xMax - 116f, rect.y + 18f, 98f, 28f), "Player Settings", buttonStyle))
            SettingsService.OpenProjectSettings("Project/Player");
    }

    private void DrawPresetDefines()
    {
        EditorGUILayout.Space(12);
        using (new EditorGUILayout.VerticalScope(CardStyle))
        {
            DrawSectionHeader("SDK Integrations", "Enable only after importing the matching SDK package.");

            for (int i = 0; i < DefineOptions.Length; i++)
            {
                var option = DefineOptions[i];
                bool current = states.TryGetValue(option.Symbol, out bool value) && value;
                Rect rowRect = GUILayoutUtility.GetRect(0f, 34f, GUILayout.ExpandWidth(true));
                bool alternate = i % 2 != 0;
                EditorGUI.DrawRect(rowRect, EditorGUIUtility.isProSkin
                    ? (alternate ? new Color(0.145f, 0.15f, 0.17f) : new Color(0.17f, 0.18f, 0.20f))
                    : (alternate ? new Color(0.94f, 0.95f, 0.97f) : new Color(0.98f, 0.98f, 0.99f)));

                states[option.Symbol] = GUI.Toggle(
                    new Rect(rowRect.x + 10f, rowRect.y + 7f, 210f, 20f),
                    current,
                    option.Symbol);
                GUI.Label(new Rect(rowRect.x + 226f, rowRect.y + 8f, rowRect.width - 336f, 18f),
                    option.Description,
                    EditorStyles.label);
                DrawBadge(new Rect(rowRect.xMax - 88f, rowRect.y + 7f, 74f, 20f), current ? "ENABLED" : "OFF", current ? EnabledColor : DisabledColor);
            }

            EditorGUILayout.Space(8);
            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Enable All", GUILayout.Height(28)))
                    SetAllPresetStates(true);

                if (GUILayout.Button("Disable All", GUILayout.Height(28)))
                    SetAllPresetStates(false);
            }
        }
    }

    private static void DrawSectionHeader(string title, string subtitle)
    {
        Rect headerRect = GUILayoutUtility.GetRect(0f, 42f, GUILayout.ExpandWidth(true));
        EditorGUI.DrawRect(headerRect, EditorGUIUtility.isProSkin
            ? new Color(0.16f, 0.18f, 0.22f)
            : new Color(0.88f, 0.92f, 0.97f));
        EditorGUI.DrawRect(new Rect(headerRect.x, headerRect.y, 4f, headerRect.height), AccentColor);

        var titleStyle = new GUIStyle(EditorStyles.boldLabel) { fontSize = 15 };
        var subtitleStyle = new GUIStyle(EditorStyles.miniLabel);
        GUI.Label(new Rect(headerRect.x + 14f, headerRect.y + 7f, headerRect.width - 28f, 18f), title, titleStyle);
        GUI.Label(new Rect(headerRect.x + 14f, headerRect.y + 24f, headerRect.width - 28f, 16f), subtitle, subtitleStyle);
        EditorGUILayout.Space(8);
    }

    private void DrawCustomDefineTools()
    {
        EditorGUILayout.Space(12);
        using (new EditorGUILayout.VerticalScope(CardStyle))
        {
            DrawSectionHeader("Custom Define", "Add project-specific symbols without leaving this window.");

            EditorGUILayout.BeginHorizontal();
            customDefine = EditorGUILayout.TextField(customDefine);

            using (new EditorGUI.DisabledScope(string.IsNullOrWhiteSpace(customDefine)))
            {
                if (GUILayout.Button("Add", GUILayout.Width(78)))
                {
                    states[NormalizeDefine(customDefine)] = true;
                    customDefine = "";
                }

                if (GUILayout.Button("Remove", GUILayout.Width(78)))
                {
                    states[NormalizeDefine(customDefine)] = false;
                    customDefine = "";
                }
            }

            EditorGUILayout.EndHorizontal();
        }
    }

    private void DrawCurrentDefines()
    {
        EditorGUILayout.Space(12);
        using (new EditorGUILayout.VerticalScope(CardStyle))
        {
            DrawSectionHeader("Current Defines", "The exact value stored in Player Settings after apply.");
            string[] defines = GetCurrentDefines();
            string defineText = defines.Length == 0 ? "(none)" : string.Join("; ", defines);
            EditorGUILayout.TextArea(defineText, GUILayout.MinHeight(60));
        }
    }

    private void DrawFooter()
    {
        EditorGUILayout.Space(12);
        EditorGUILayout.BeginHorizontal();

        GUI.backgroundColor = EnabledColor;
        if (GUILayout.Button("Apply Defines", GUILayout.Height(34)))
            ApplyStates();
        GUI.backgroundColor = Color.white;

        EditorGUILayout.EndHorizontal();
    }

    private static void DrawBadge(Rect rect, string text, Color color)
    {
        EditorGUI.DrawRect(rect, color);
        var style = new GUIStyle(EditorStyles.miniBoldLabel)
        {
            alignment = TextAnchor.MiddleCenter,
            normal = { textColor = Color.white }
        };
        GUI.Label(rect, text, style);
    }

    private static GUIStyle CardStyle
    {
        get
        {
            return new GUIStyle(EditorStyles.helpBox)
            {
                padding = new RectOffset(12, 12, 12, 12),
                margin = new RectOffset(12, 12, 0, 0)
            };
        }
    }

    private void RefreshStates()
    {
        states.Clear();

        foreach (string define in GetCurrentDefines())
            states[define] = true;

        foreach (var option in DefineOptions)
        {
            if (!states.ContainsKey(option.Symbol))
                states[option.Symbol] = false;
        }
    }

    private void ApplyStates()
    {
        var defines = states
            .Where(pair => pair.Value)
            .Select(pair => pair.Key)
            .Where(define => !string.IsNullOrWhiteSpace(define))
            .Select(NormalizeDefine)
            .Distinct()
            .OrderBy(define => define)
            .ToArray();

        SetCurrentDefines(defines);
        RefreshStates();
        Debug.Log("[GameFoundationDefineSymbolsWindow] Applied defines: " + string.Join(";", defines));
    }

    private void SetAllPresetStates(bool enabled)
    {
        foreach (var option in DefineOptions)
            states[option.Symbol] = enabled;
    }

    private static string TrimDefine(string define)
    {
        return define.Trim();
    }

    private static string NormalizeDefine(string define)
    {
        return TrimDefine(define);
    }

    private static string[] GetCurrentDefines()
    {
        string rawDefines = GetCurrentDefineString();

        if (string.IsNullOrWhiteSpace(rawDefines))
            return new string[0];

        return rawDefines
            .Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries)
            .Select(NormalizeDefine)
            .Where(define => !string.IsNullOrWhiteSpace(define))
            .Distinct()
            .OrderBy(define => define)
            .ToArray();
    }

    private static string GetCurrentDefineString()
    {
#if UNITY_2021_2_OR_NEWER
        return PlayerSettings.GetScriptingDefineSymbols(GetSelectedNamedBuildTarget());
#else
        return PlayerSettings.GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup);
#endif
    }

    private static void SetCurrentDefines(string[] defines)
    {
        string defineString = string.Join(";", defines);

#if UNITY_2021_2_OR_NEWER
        PlayerSettings.SetScriptingDefineSymbols(GetSelectedNamedBuildTarget(), defineString);
#else
        PlayerSettings.SetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup, defineString);
#endif
    }

    private static string GetSelectedTargetName()
    {
#if UNITY_2021_2_OR_NEWER
        return GetSelectedNamedBuildTarget().TargetName;
#else
        return EditorUserBuildSettings.selectedBuildTargetGroup.ToString();
#endif
    }

#if UNITY_2021_2_OR_NEWER
    private static NamedBuildTarget GetSelectedNamedBuildTarget()
    {
        return NamedBuildTarget.FromBuildTargetGroup(EditorUserBuildSettings.selectedBuildTargetGroup);
    }
#endif

    private struct DefineOption
    {
        public readonly string Symbol;
        public readonly string Description;

        public DefineOption(string symbol, string description)
        {
            Symbol = symbol;
            Description = description;
        }
    }
}
