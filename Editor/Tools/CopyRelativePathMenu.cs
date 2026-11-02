using System.Linq;
using UnityEditor;
using UnityEngine;

public static class CopyRelativePathMenu
{
    private const string MenuPath = "GameObject/Copy Relative Path";

    // Hierarchy right-click shows GameObject/ menu items. Path is from scene root, e.g. "Canvas/Panel/Button".
    [MenuItem(MenuPath, false, 0)]
    private static void Copy()
    {
        // ponytail: multi-select joined by newline, order = selection order. Fine until a different format is needed.
        EditorGUIUtility.systemCopyBuffer = string.Join("\n", Selection.gameObjects.Select(GetPath));
    }

    [MenuItem(MenuPath, true)]
    private static bool Validate() => Selection.gameObjects.Length > 0;

    private static string GetPath(GameObject go)
    {
        var path = go.name;
        for (var t = go.transform.parent; t != null; t = t.parent)
            path = t.name + "/" + path;
        return path;
    }
}
