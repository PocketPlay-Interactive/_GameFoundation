using UnityEngine;
using UnityEditor;

public static class UtilsEditor
{
    [MenuItem("Utils/Remove All Missing Scripts %#r")]
    public static void RemoveAllMissingScriptsInSelection()
    {
        int go_count = 0, components_count = 0, missing_count = 0;
        GameObject[] selection = Selection.gameObjects;
        foreach (GameObject go in selection)
        {
            RemoveInGO(go, ref go_count, ref components_count, ref missing_count);
            EditorUtility.SetDirty(go);
        }
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log($"Searched {go_count} GameObjects, {components_count} components, removed {missing_count} missing scripts.");
    }

    private static void RemoveInGO(GameObject go, ref int go_count, ref int comp_count, ref int missing_count)
    {
        go_count++;
        int removed = GameObjectUtility.RemoveMonoBehavioursWithMissingScript(go);
        if (removed > 0)
            missing_count += removed;

        var components = go.GetComponents<Component>();
        comp_count += components.Length;

        foreach (Transform child in go.transform)
            RemoveInGO(child.gameObject, ref go_count, ref comp_count, ref missing_count);
    }
}