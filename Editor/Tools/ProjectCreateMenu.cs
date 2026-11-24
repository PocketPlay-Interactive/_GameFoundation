using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

/// <summary>Project-window right-click: Assets/GameFoundation/... creates scripts, copies paths and shortcuts.</summary>
public static class ProjectCreateMenu
{
    private const string Root = "Assets/GameFoundation/";

    private const string ScreenTemplate = @"using UnityEngine;

public class #SCRIPTNAME# : ScreenCanvas
{
    protected override void OnShow()
    {
    }

    protected override void OnHide()
    {
    }
}
";

    private const string PopupTemplate = @"using UnityEngine.Events;

public class #SCRIPTNAME# : PopupCanvas
{
    public override void Initialized(UnityAction _PRIMARY_CALLBACK, UnityAction _SECONDARY_CALLBACK, params string[] msg)
    {
    }

    public override void OnShow()
    {
    }

    public override void OnHide()
    {
    }
}
";

    private const string ModuleTemplate = @"using System.Collections;

public class #SCRIPTNAME# : GameFoundationModuleBehaviour
{
    public override IEnumerator Initialize()
    {
        yield break;
    }
}
";

    [MenuItem(Root + "Create/Screen Script", false, 0)]
    private static void CreateScreen() => CreateScript("NewScreen", ScreenTemplate);

    [MenuItem(Root + "Create/Popup Script", false, 1)]
    private static void CreatePopup() => CreateScript("NewPopup", PopupTemplate);

    [MenuItem(Root + "Create/Foundation Module Script", false, 2)]
    private static void CreateModule() => CreateScript("NewFoundationModule", ModuleTemplate);

    // Create menus need a writable folder under Assets (not a read-only package folder).
    [MenuItem(Root + "Create/Screen Script", true)]
    [MenuItem(Root + "Create/Popup Script", true)]
    [MenuItem(Root + "Create/Foundation Module Script", true)]
    private static bool CanCreate() => !EditorApplication.isCompiling && IsInsideAssets(SelectedFolder());

    // Keep the template alive until Unity finishes inline renaming; no temporary file is needed.
    private static void CreateScript(string baseName, string template)
    {
        string folder = SelectedFolder();
        if (!IsInsideAssets(folder)) return;

        string path = AssetDatabase.GenerateUniqueAssetPath(folder + "/" + baseName + ".cs");
        var action = ScriptableObject.CreateInstance<CreateScriptAction>();
        action.Template = template;
        ProjectWindowUtil.StartNameEditingIfProjectWindowExists(0, action, path, null, null);
    }

    public class CreateScriptAction : UnityEditor.ProjectWindowCallback.EndNameEditAction
    {
        public string Template;

        public override void Action(int instanceId, string pathName, string resourceFile)
        {
            string className = Path.GetFileNameWithoutExtension(pathName);
            if (!IsInsideAssets(pathName) || !AssetDatabase.IsValidFolder(Path.GetDirectoryName(pathName).Replace('\\', '/')) ||
                !System.Text.RegularExpressions.Regex.IsMatch(className, @"^[A-Za-z_][A-Za-z0-9_]*$"))
            {
                Debug.LogError("[GameFoundation] Chọn folder trong Assets và tên class C# hợp lệ.");
                return;
            }

            try
            {
                using (var file = new FileStream(pathName, FileMode.CreateNew, FileAccess.Write))
                using (var writer = new StreamWriter(file, new System.Text.UTF8Encoding(false)))
                    writer.Write(Template.Replace("#SCRIPTNAME#", className).Replace("\r\n", "\n"));
                AssetDatabase.ImportAsset(pathName);
                ProjectWindowUtil.ShowCreatedAsset(AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(pathName));
            }
            catch (IOException exception)
            {
                Debug.LogError($"[GameFoundation] Không thể tạo {pathName}: {exception.Message}");
            }
        }
    }

    private static string SelectedFolder()
    {
        string path = AssetDatabase.GetAssetPath(Selection.activeObject);
        if (string.IsNullOrEmpty(path))
            return "Assets";

        return AssetDatabase.IsValidFolder(path) ? path : Path.GetDirectoryName(path).Replace('\\', '/');
    }

    private static bool IsInsideAssets(string path) =>
        path == "Assets" || path.StartsWith("Assets/", StringComparison.Ordinal);

    // ---- Copy -------------------------------------------------------------------------------

    [MenuItem(Root + "Copy/Asset Path", false, 20)]
    private static void CopyAssetPath() => EditorGUIUtility.systemCopyBuffer = AssetDatabase.GetAssetPath(Selection.activeObject);

    [MenuItem(Root + "Copy/Asset Path", true)]
    private static bool CanCopyAssetPath() => HasAsset();

    [MenuItem(Root + "Copy/GUID", false, 21)]
    private static void CopyGuid() => EditorGUIUtility.systemCopyBuffer = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(Selection.activeObject));

    [MenuItem(Root + "Copy/GUID", true)]
    private static bool CanCopyGuid() => HasAsset();

    [MenuItem(Root + "Copy/Resources Path", false, 22)]
    private static void CopyResourcesPath()
    {
        string path = AssetDatabase.GetAssetPath(Selection.activeObject);
        string resourcesPath = ResourcesPath(path);
        if (resourcesPath == null)
            return;

        // Resources.Load keys are not unique: two assets with the same path and name collide at runtime.
        string[] duplicates = AssetDatabase.GetAllAssetPaths()
            .Where(other => other != path && ResourcesPath(other) == resourcesPath
                && AssetDatabase.GetMainAssetTypeAtPath(other) == AssetDatabase.GetMainAssetTypeAtPath(path))
            .ToArray();
        if (duplicates.Length > 0)
            Debug.LogWarning($"[GameFoundation] \"{resourcesPath}\" matches several Resources assets of the same type; Resources.Load may return the wrong one:\n{string.Join("\n", duplicates)}");

        EditorGUIUtility.systemCopyBuffer = resourcesPath;
    }

    // Only assets inside a Resources folder can be loaded with Resources.Load.
    [MenuItem(Root + "Copy/Resources Path", true)]
    private static bool CanCopyResourcesPath() => HasAsset() && ResourcesPath(AssetDatabase.GetAssetPath(Selection.activeObject)) != null;

    private static bool HasAsset() =>
        Selection.activeObject != null && !string.IsNullOrEmpty(AssetDatabase.GetAssetPath(Selection.activeObject));

    private static string ResourcesPath(string assetPath)
    {
        if (string.IsNullOrEmpty(assetPath) || AssetDatabase.IsValidFolder(assetPath))
            return null;

        const string marker = "/Resources/";
        int index = assetPath.IndexOf(marker, StringComparison.Ordinal);
        if (index < 0)
            return null;

        string relative = assetPath.Substring(index + marker.Length);
        return relative.Substring(0, relative.Length - Path.GetExtension(relative).Length);
    }

    // ---- Shortcuts --------------------------------------------------------------------------

    // The Project window selection and Hierarchy selection are mutually exclusive. Start from the
    // Hierarchy instead and choose a destination under Assets; SaveAsPrefabAsset leaves the scene untouched.
    [MenuItem("GameObject/GameFoundation/Create Prefab...", false, 90)]
    private static void CreatePrefab(MenuCommand command)
    {
        GameObject source = command.context as GameObject ?? Selection.activeGameObject;
        if (source == null || !source.scene.IsValid()) return;

        string path = EditorUtility.SaveFilePanelInProject("Create Prefab", source.name, "prefab", "Choose a destination under Assets.");
        if (string.IsNullOrEmpty(path) || !IsInsideAssets(path)) return;
        if (File.Exists(path))
        {
            Debug.LogWarning($"[GameFoundation] Refusing to overwrite existing prefab: {path}");
            return;
        }

        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(source, path);
        if (prefab != null) EditorGUIUtility.PingObject(prefab);
    }

    [MenuItem("GameObject/GameFoundation/Create Prefab...", true)]
    private static bool CanCreatePrefab() => !EditorApplication.isPlayingOrWillChangePlaymode &&
        Selection.activeGameObject != null && Selection.activeGameObject.scene.IsValid();

    [MenuItem(Root + "Reveal in Explorer", false, 30)]
    private static void Reveal() => EditorUtility.RevealInFinder(AssetDatabase.GetAssetPath(Selection.activeObject));

    [MenuItem(Root + "Reveal in Explorer", true)]
    private static bool CanReveal() => HasAsset();

    [MenuItem(Root + "Tools/PNG Resizer", false, 40)]
    private static void OpenPngResizer() => EditorApplication.ExecuteMenuItem("GameFoundation/Tools/PNG Resizer");

    [MenuItem(Root + "Tools/Capture Camera", false, 41)]
    private static void OpenCaptureCamera() => EditorApplication.ExecuteMenuItem("GameFoundation/Tools/Capture Camera");
}
