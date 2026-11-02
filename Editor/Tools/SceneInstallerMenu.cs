using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Builds the bootstrap hierarchies documented in README "Tạo Scene Mới" directly in the open scene.
// No prefab or asset is stored in the package; re-running only adds what is missing.
public static class SceneInstallerMenu
{
    private const string MinimalMenu = "GameFoundation/Scene/Create Minimal Setup";
    private const string FullMenu = "GameFoundation/Scene/Create Full Setup";

    [MenuItem(MinimalMenu)]
    private static void CreateMinimal() => Build(full: false);

    [MenuItem(FullMenu)]
    private static void CreateFull() => Build(full: true);

    [MenuItem(MinimalMenu, true)]
    [MenuItem(FullMenu, true)]
    private static bool Validate()
    {
        Scene scene = SceneManager.GetActiveScene();
        return scene.IsValid() && !EditorSceneManager.IsPreviewScene(scene) && !EditorApplication.isPlayingOrWillChangePlaymode;
    }

    private static void Build(bool full)
    {
        Scene scene = SceneManager.GetActiveScene();

        try
        {
            // GameFoundation
            // |-- Manager (+ ManagerCoordinator)
            // `-- LoadingCanvas            (full)
            Manager manager = Find<Manager>(scene);
            if (manager == null)
            {
                GameObject root = Create("GameFoundation", scene);
                manager = root.AddComponent<Manager>();
                root.AddComponent<ManagerCoordinator>();
            }

            ManagerCoordinator coordinator = manager.GetComponent<ManagerCoordinator>() ?? Find<ManagerCoordinator>(scene);
            if (coordinator == null)
                coordinator = manager.gameObject.AddComponent<ManagerCoordinator>();

            // Concurrency
            if (Find<Concurrency>(scene) == null)
                Create("Concurrency", scene).AddComponent<Concurrency>();

            SerializedObject serializedManager = new SerializedObject(manager);
            SetReference(serializedManager.FindProperty("managerCoordinator"), coordinator);

            if (full)
            {
                LoadingCanvas loading = Find<LoadingCanvas>(scene) ?? CreateLoadingCanvas(manager.transform);
                SetReference(new SerializedObject(coordinator).FindProperty("loadingCanvas"), loading, apply: true);

                if (Find<CanvasManager>(scene) == null)
                    Create("CanvasManager", scene).AddComponent<CanvasManager>();

                SoundService sound = Find<SoundService>(scene) ?? CreateSoundService(scene);
                MusicService music = Find<MusicService>(scene) ?? CreateMusicService(scene);

                SerializedProperty modules = serializedManager.FindProperty("extensionModules");
                AddModuleReference(modules, sound);
                AddModuleReference(modules, music);
            }

            serializedManager.ApplyModifiedProperties();
            EditorSceneManager.MarkSceneDirty(scene);
            Selection.activeGameObject = manager.gameObject;
            Debug.Log($"[GameFoundation] Created {(full ? "full" : "minimal")} setup.");
        }
        catch (Exception exception)
        {
            EditorUtility.DisplayDialog("Game Foundation", exception.Message, "OK");
            Debug.LogException(exception);
        }
    }

    private static LoadingCanvas CreateLoadingCanvas(Transform parent)
    {
        GameObject go = new GameObject("LoadingCanvas", typeof(RectTransform));
        Undo.RegisterCreatedObjectUndo(go, "Create LoadingCanvas");
        go.transform.SetParent(parent, false);
        go.layer = LayerMask.NameToLayer("UI");

        Canvas canvas = go.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 999;
        go.AddComponent<CanvasScaler>();
        go.AddComponent<GraphicRaycaster>();
        go.AddComponent<CanvasGroup>();
        LoadingCanvas loading = go.AddComponent<LoadingCanvas>();
        SetReference(new SerializedObject(loading).FindProperty("canvasGroup"), go.GetComponent<CanvasGroup>(), apply: true);

        GameObject bg = new GameObject("Background", typeof(RectTransform), typeof(Image));
        bg.transform.SetParent(go.transform, false);
        bg.layer = go.layer;
        bg.GetComponent<Image>().color = Color.black;
        RectTransform rect = bg.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = rect.offsetMax = Vector2.zero;
        return loading;
    }

    private static SoundService CreateSoundService(Scene scene)
    {
        GameObject go = Create("SoundService", scene);
        SoundService sound = go.AddComponent<SoundService>();

        GameObject special = new GameObject("AudioSourceSpecial");
        special.transform.SetParent(go.transform, false);
        AudioSource source = special.AddComponent<AudioSource>();
        source.playOnAwake = false;
        SetReference(new SerializedObject(sound).FindProperty("audioSourceSpecial"), source, apply: true);
        return sound;
    }

    private static MusicService CreateMusicService(Scene scene)
    {
        GameObject go = Create("MusicService", scene);
        AudioSource source = go.AddComponent<AudioSource>();
        source.playOnAwake = false;
        source.loop = true;
        MusicService music = go.AddComponent<MusicService>();
        SetReference(new SerializedObject(music).FindProperty("audioSource"), source, apply: true);
        return music;
    }

    private static GameObject Create(string name, Scene scene)
    {
        GameObject go = new GameObject(name);
        SceneManager.MoveGameObjectToScene(go, scene);
        Undo.RegisterCreatedObjectUndo(go, "Create " + name);
        return go;
    }

    private static T Find<T>(Scene scene) where T : Component
    {
        foreach (T component in UnityEngine.Object.FindObjectsByType<T>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (component.gameObject.scene == scene)
                return component;
        }

        return null;
    }

    private static void SetReference(SerializedProperty property, UnityEngine.Object value, bool apply = false)
    {
        if (property == null)
            throw new InvalidOperationException("Serialized field not found; GameFoundation source changed.");

        property.objectReferenceValue = value;
        if (apply)
            property.serializedObject.ApplyModifiedProperties();
    }

    private static void AddModuleReference(SerializedProperty modules, MonoBehaviour module)
    {
        if (modules == null)
            throw new InvalidOperationException("Manager.extensionModules was not found.");

        for (int index = 0; index < modules.arraySize; index++)
        {
            SerializedProperty element = modules.GetArrayElementAtIndex(index);
            if (element.objectReferenceValue == module)
                return;

            if (element.objectReferenceValue == null)
            {
                element.objectReferenceValue = module;
                return;
            }
        }

        modules.InsertArrayElementAtIndex(modules.arraySize);
        modules.GetArrayElementAtIndex(modules.arraySize - 1).objectReferenceValue = module;
    }
}
