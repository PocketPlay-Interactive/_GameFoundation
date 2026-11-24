using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>Hierarchy right-click menu for creating GameFoundation scene components.</summary>
public static class FoundationHierarchyMenu
{
    private const string Root = "GameObject/GameFoundation/";

    [MenuItem(Root + "Setup/Minimal", false, 0)]
    private static void Minimal() => ExecuteMenu("GameFoundation/Scene/Create Minimal Setup");

    [MenuItem(Root + "Setup/Full", false, 1)]
    private static void Full() => ExecuteMenu("GameFoundation/Scene/Create Full Setup");

    [MenuItem(Root + "Manager/Manager", false, 10)]
    private static void Manager() => Select(SceneInstallerMenu.EnsureManager(ActiveScene()).gameObject);

    [MenuItem(Root + "Manager/Loading Canvas", false, 11)]
    private static void LoadingCanvas() => Select(SceneInstallerMenu.EnsureLoadingCanvas(ActiveScene()).gameObject);

    [MenuItem(Root + "Manager/Canvas Manager", false, 12)]
    private static void CanvasManager() => Select(SceneInstallerMenu.EnsureCanvasManager(ActiveScene()).gameObject);

    [MenuItem(Root + "Services/Sound Service", false, 20)]
    private static void Sound() => Select(SceneInstallerMenu.EnsureSound(ActiveScene()).gameObject);

    [MenuItem(Root + "Services/Music Service", false, 21)]
    private static void Music() => Select(SceneInstallerMenu.EnsureMusic(ActiveScene()).gameObject);

    [MenuItem(Root + "Input/Input Manager", false, 30)]
    private static void Input() => Select(Ensure<InputManager>("InputManager").gameObject);

    [MenuItem(Root + "Input/Touch System", false, 31)]
    private static void Touch() => CreateInputSystem<TouchSystem>("TouchSystem");

    [MenuItem(Root + "Input/Raycast System", false, 32)]
    private static void Raycast() => CreateInputSystem<RaycastSystem>("RaycastSystem");

    [MenuItem(Root + "Input/Shake System", false, 33)]
    private static void Shake() => CreateInputSystem<ShakeSystem>("ShakeSystem");

    // All items are scene operations: disable them in Play Mode and when the active scene is invalid.
    [MenuItem(Root + "Setup/Minimal", true)]
    [MenuItem(Root + "Setup/Full", true)]
    [MenuItem(Root + "Manager/Manager", true)]
    [MenuItem(Root + "Manager/Loading Canvas", true)]
    [MenuItem(Root + "Manager/Canvas Manager", true)]
    [MenuItem(Root + "Services/Sound Service", true)]
    [MenuItem(Root + "Services/Music Service", true)]
    [MenuItem(Root + "Input/Input Manager", true)]
    [MenuItem(Root + "Input/Touch System", true)]
    [MenuItem(Root + "Input/Raycast System", true)]
    [MenuItem(Root + "Input/Shake System", true)]
    private static bool Validate() =>
        ActiveScene().IsValid() && !EditorSceneManager.IsPreviewScene(ActiveScene()) && !EditorApplication.isPlayingOrWillChangePlaymode;

    private static Scene ActiveScene() => SceneManager.GetActiveScene();

    private static void ExecuteMenu(string menu) => EditorApplication.ExecuteMenuItem(menu);

    private static T Ensure<T>(string name) where T : Component
    {
        Scene scene = ActiveScene();
        T existing = SceneInstallerMenu.FindInScene<T>(scene);
        return existing != null ? existing : SceneInstallerMenu.CreateInScene(name, scene).AddComponent<T>();
    }

    private static void CreateInputSystem<T>(string name) where T : Component
    {
        Ensure<InputManager>("InputManager");
        Select(Ensure<T>(name).gameObject);
    }

    private static void Select(GameObject go)
    {
        EditorSceneManager.MarkSceneDirty(ActiveScene());
        Selection.activeGameObject = go;
        EditorGUIUtility.PingObject(go);
    }
}
