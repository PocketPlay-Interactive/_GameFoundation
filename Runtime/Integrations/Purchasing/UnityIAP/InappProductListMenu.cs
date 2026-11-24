#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

/// <summary>
/// GameObject/GameFoundation/IAP/Product List: creates Assets/Resources/Data/InappProductList.asset,
/// the exact name and folder InappPurchase loads at startup. Selects the existing asset if there is one.
/// </summary>
public static class InappProductListMenu
{
    private const string AssetPath = "Assets/Resources/Data/InappProductList.asset";

    [MenuItem("GameObject/GameFoundation/IAP/Product List", false, 50)]
    [MenuItem("Assets/GameFoundation/Create/IAP Product List", false, 3)]
    private static void CreateProductList()
    {
        InappProductList list = AssetDatabase.LoadAssetAtPath<InappProductList>(AssetPath);
        if (list == null && File.Exists(AssetPath))
        {
            Debug.LogError($"[GameFoundation] {AssetPath} already exists but is not an InappProductList; refusing to overwrite it.");
            return;
        }

        if (list == null)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(AssetPath));
            list = ScriptableObject.CreateInstance<InappProductList>();
            list.Products = new List<InappProduct>();
            AssetDatabase.CreateAsset(list, AssetPath);
            AssetDatabase.SaveAssets();
            Debug.Log($"[GameFoundation] Created {AssetPath}. Add one entry per store product (ProductId must match the store).");
        }

        Selection.activeObject = list;
        EditorGUIUtility.PingObject(list);
    }
}
#endif
