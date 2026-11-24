using Pathfinding;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>GameObject/GameFoundation/Gameplay/Pathfinding Setup: GraphManager + AStarPathfinder wired together.</summary>
public static class PathfindingHierarchyMenu
{
    private const string MenuPath = "GameObject/GameFoundation/Gameplay/Pathfinding Setup";

    [MenuItem(MenuPath, false, 40)]
    private static void CreateSetup()
    {
        Scene scene = SceneManager.GetActiveScene();

        GraphManager graph = Find<GraphManager>(scene);
        if (graph == null)
        {
            GameObject graphObject = new GameObject("GraphManager");
            SceneManager.MoveGameObjectToScene(graphObject, scene);
            Undo.RegisterCreatedObjectUndo(graphObject, "Create GraphManager");
            graph = graphObject.AddComponent<GraphManager>();
        }

        AStarPathfinder pathfinder = Find<AStarPathfinder>(scene);
        if (pathfinder == null)
        {
            GameObject pathObject = new GameObject("AStarPathfinder");
            SceneManager.MoveGameObjectToScene(pathObject, scene);
            Undo.RegisterCreatedObjectUndo(pathObject, "Create AStarPathfinder");
            pathfinder = pathObject.AddComponent<AStarPathfinder>();
        }

        if (pathfinder.graphManager == null)
        {
            Undo.RecordObject(pathfinder, "Link GraphManager");
            pathfinder.graphManager = graph;
            EditorUtility.SetDirty(pathfinder);
        }

        EditorSceneManager.MarkSceneDirty(scene);
        Selection.activeGameObject = graph.gameObject;
        EditorGUIUtility.PingObject(graph.gameObject);
    }

    [MenuItem(MenuPath, true)]
    private static bool Validate()
    {
        Scene scene = SceneManager.GetActiveScene();
        return scene.IsValid() && !EditorSceneManager.IsPreviewScene(scene) && !EditorApplication.isPlayingOrWillChangePlaymode;
    }

    private static T Find<T>(Scene scene) where T : Component
    {
        foreach (T component in Object.FindObjectsByType<T>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (component.gameObject.scene == scene)
                return component;
        }

        return null;
    }
}
