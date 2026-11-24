using System.IO;
using UnityEditor;
using UnityEngine;

public static class DebugHierarchyMenu
{
    private const string Root = "GameObject/GameFoundation/Debug/";

    [MenuItem(Root + "Open Runtime Storage", false, 100)]
    private static void OpenRuntimeStorage() => EditorApplication.ExecuteMenuItem("GameFoundation/Data Tables/Runtime Storage");

    [MenuItem(Root + "Open Save Folder", false, 101)]
    private static void OpenSaveFolder() => EditorUtility.RevealInFinder(Application.persistentDataPath);

    [MenuItem(Root + "Delete Saved Data", false, 102)]
    private static void DeleteSavedData()
    {
        if (!EditorUtility.DisplayDialog("Game Foundation", "Delete package save files and all PlayerPrefs of this project?", "Delete", "Cancel"))
            return;

        EditorApplication.ExecuteMenuItem("GameFoundation/Tools/Delete Saved Data");
        Debug.Log("[GameFoundation] Saved data deleted.");
    }

    [MenuItem(Root + "Delete Saved Data", true)]
    private static bool CanDelete() => !EditorApplication.isPlaying;
}
