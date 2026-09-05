using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class SceneInstallerMenu
{
    private const string MenuPath = "GameFoundation/Scene/Install Manager and Audio Services";
    private const string ManagerPrefabPath = "Packages/com.dann.gamefoundation/Runtime/Bootstrap/Prefabs/Manager.prefab";
    private const string SoundServicePrefabPath = "Packages/com.dann.gamefoundation/Runtime/Bootstrap/Prefabs/SoundService.prefab";
    private const string MusicServicePrefabPath = "Packages/com.dann.gamefoundation/Runtime/Bootstrap/Prefabs/MusicService.prefab";

    [MenuItem(MenuPath)]
    private static void Install()
    {
        Scene scene = SceneManager.GetActiveScene();

        try
        {
            Manager manager = FindOrCreate<Manager>(ManagerPrefabPath, scene);
            SoundService soundService = FindOrCreate<SoundService>(SoundServicePrefabPath, scene);
            MusicService musicService = FindOrCreate<MusicService>(MusicServicePrefabPath, scene);

            SerializedObject serializedManager = new SerializedObject(manager);
            SerializedProperty extensionModules = serializedManager.FindProperty("extensionModules");
            AddModuleReference(extensionModules, soundService);
            AddModuleReference(extensionModules, musicService);
            serializedManager.ApplyModifiedProperties();

            Debug.Assert(HasModuleReference(extensionModules, soundService));
            Debug.Assert(HasModuleReference(extensionModules, musicService));

            EditorSceneManager.MarkSceneDirty(scene);
            Selection.activeGameObject = manager.gameObject;
            Debug.Log("[GameFoundation] Installed Manager, SoundService and MusicService.");
        }
        catch (Exception exception)
        {
            EditorUtility.DisplayDialog("Game Foundation", exception.Message, "OK");
            Debug.LogException(exception);
        }
    }

    [MenuItem(MenuPath, true)]
    private static bool ValidateInstall()
    {
        Scene scene = SceneManager.GetActiveScene();
        return scene.IsValid() && !EditorSceneManager.IsPreviewScene(scene) && !EditorApplication.isPlayingOrWillChangePlaymode;
    }

    private static T FindOrCreate<T>(string prefabPath, Scene scene) where T : Component
    {
        foreach (T component in UnityEngine.Object.FindObjectsByType<T>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (component.gameObject.scene == scene)
                return component;
        }

        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        if (prefab == null)
            throw new InvalidOperationException($"Prefab not found: {prefabPath}");

        GameObject instance = PrefabUtility.InstantiatePrefab(prefab, scene) as GameObject;
        if (instance == null)
            throw new InvalidOperationException($"Could not instantiate prefab: {prefabPath}");

        Undo.RegisterCreatedObjectUndo(instance, "Install Game Foundation Service");
        T componentInstance = instance.GetComponent<T>();
        if (componentInstance == null)
            throw new InvalidOperationException($"Component {typeof(T).Name} not found on prefab: {prefabPath}");

        return componentInstance;
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

    private static bool HasModuleReference(SerializedProperty modules, MonoBehaviour module)
    {
        for (int index = 0; index < modules.arraySize; index++)
        {
            if (modules.GetArrayElementAtIndex(index).objectReferenceValue == module)
                return true;
        }

        return false;
    }
}
