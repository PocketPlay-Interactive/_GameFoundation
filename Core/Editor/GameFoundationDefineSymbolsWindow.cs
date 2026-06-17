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
        new DefineOption("FIREBASE", "Firebase Analytics"),
        new DefineOption("ADMOB", "Google Mobile Ads"),
        new DefineOption("ADMOB_TEST", "Google test ad unit id"),
        new DefineOption("ADMOB_UMP_BACON", "Bacon UMP consent flow"),
        new DefineOption("USE_ADMOB_CUSTOM_PLUGIN", "Native ads/JKit plugin"),
        new DefineOption("APPFLYER", "AppsFlyer SDK"),
        new DefineOption("IAPPURCHASE_ENABLE", "Unity IAP"),
    };

    private readonly Dictionary<string, bool> states = new Dictionary<string, bool>();
    private Vector2 scrollPosition;
    private string customDefine = "";

    [MenuItem("GameFoundation/Define Symbols")]
    public static void ShowWindow()
    {
        var window = GetWindow<GameFoundationDefineSymbolsWindow>("GF Defines");

        window.minSize = new Vector2(600, 400);
        window.maxSize = new Vector2(1200, 800);
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
        EditorGUILayout.LabelField("GameFoundation Define Symbols", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox(
            "Toggle SDK defines for the selected build target group. Click Apply once after changing.",
            MessageType.Info);

        EditorGUILayout.LabelField("Selected Target", GetSelectedTargetName());
        EditorGUILayout.Space(6);
    }

    private void DrawPresetDefines()
    {
        EditorGUILayout.LabelField("SDK Defines", EditorStyles.boldLabel);

        foreach (var option in DefineOptions)
        {
            bool current = states.TryGetValue(option.Symbol, out bool value) && value;

            EditorGUILayout.BeginHorizontal();
            states[option.Symbol] = EditorGUILayout.ToggleLeft(option.Symbol, current, GUILayout.Width(210));
            EditorGUILayout.LabelField(option.Description);
            EditorGUILayout.EndHorizontal();
        }

        EditorGUILayout.Space(8);
        EditorGUILayout.BeginHorizontal();

        if (GUILayout.Button("Enable All"))
            SetAllPresetStates(true);

        if (GUILayout.Button("Disable All"))
            SetAllPresetStates(false);

        if (GUILayout.Button("Refresh"))
            RefreshStates();

        EditorGUILayout.EndHorizontal();
    }

    private void DrawCustomDefineTools()
    {
        EditorGUILayout.Space(14);
        EditorGUILayout.LabelField("Custom Define", EditorStyles.boldLabel);

        EditorGUILayout.BeginHorizontal();
        customDefine = EditorGUILayout.TextField(customDefine);

        using (new EditorGUI.DisabledScope(string.IsNullOrWhiteSpace(customDefine)))
        {
            if (GUILayout.Button("Add", GUILayout.Width(70)))
            {
                states[TrimDefine(customDefine)] = true;
                customDefine = "";
            }

            if (GUILayout.Button("Remove", GUILayout.Width(70)))
            {
                states[TrimDefine(customDefine)] = false;
                customDefine = "";
            }
        }

        EditorGUILayout.EndHorizontal();
    }

    private void DrawCurrentDefines()
    {
        EditorGUILayout.Space(14);
        EditorGUILayout.LabelField("Current Defines", EditorStyles.boldLabel);

        string[] defines = GetCurrentDefines();
        string defineText = defines.Length == 0 ? "(none)" : string.Join("; ", defines);
        EditorGUILayout.TextArea(defineText, GUILayout.MinHeight(54));
    }

    private void DrawFooter()
    {
        EditorGUILayout.Space(8);
        EditorGUILayout.BeginHorizontal();

        if (GUILayout.Button("Apply Defines", GUILayout.Height(28)))
            ApplyStates();

        if (GUILayout.Button("Open Player Settings", GUILayout.Height(28)))
            SettingsService.OpenProjectSettings("Project/Player");

        EditorGUILayout.EndHorizontal();
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
            .Select(TrimDefine)
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

    private static string[] GetCurrentDefines()
    {
        string rawDefines = GetCurrentDefineString();

        if (string.IsNullOrWhiteSpace(rawDefines))
            return new string[0];

        return rawDefines
            .Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries)
            .Select(TrimDefine)
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
