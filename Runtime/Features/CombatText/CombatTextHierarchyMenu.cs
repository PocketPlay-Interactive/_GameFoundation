#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>GameObject/GameFoundation/Gameplay/Floating Damage: instantiates the packaged FloatingDamageSprites prefab.</summary>
public static class CombatTextHierarchyMenu
{
    private const string MenuPath = "GameObject/GameFoundation/Gameplay/Floating Damage";
    private const string PrefabName = "FloatingDamageSprites";

    [MenuItem(MenuPath, false, 41)]
    private static void CreateFloatingDamage()
    {
        Scene scene = SceneManager.GetActiveScene();
        foreach (FloatingDamageSprites item in Object.FindObjectsByType<FloatingDamageSprites>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (item.gameObject.scene == scene)
            {
                Selection.activeGameObject = item.gameObject;
                EditorGUIUtility.PingObject(item.gameObject);
                return;
            }
        }

        GameObject instance = null;
        foreach (string guid in AssetDatabase.FindAssets(PrefabName + " t:Prefab"))
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GUIDToAssetPath(guid));
            if (prefab != null && prefab.name == PrefabName && prefab.GetComponent<FloatingDamageSprites>() != null)
            {
                instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, scene);
                break;
            }
        }

        if (instance == null)
        {
            instance = new GameObject(PrefabName, typeof(FloatingDamageSprites));
            SceneManager.MoveGameObjectToScene(instance, scene);
            Debug.LogWarning("[GameFoundation] Prefab FloatingDamageSprites was not found; assign Damage Instance Prefab manually.");
        }

        Undo.RegisterCreatedObjectUndo(instance, "Create Floating Damage");
        EditorSceneManager.MarkSceneDirty(scene);
        Selection.activeGameObject = instance;
        EditorGUIUtility.PingObject(instance);
    }

    [MenuItem(MenuPath, true)]
    private static bool Validate()
    {
        Scene scene = SceneManager.GetActiveScene();
        return scene.IsValid() && !EditorSceneManager.IsPreviewScene(scene) && !EditorApplication.isPlayingOrWillChangePlaymode;
    }
}
#endif
