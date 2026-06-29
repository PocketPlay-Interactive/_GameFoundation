using UnityEngine;
using UnityEditor;
using System.IO;

public class PNGResizerTool : EditorWindow
{
    private Texture2D singleTexture;
    private int newWidth = 256;
    private int newHeight = 256;
    private bool overwrite = false;

    [MenuItem("Utils/PNG Resizer")]
    public static void ShowWindow()
    {
        GetWindow<PNGResizerTool>("PNG Resizer");
    }

    private int RoundToNearest4(int value) => Mathf.RoundToInt(value / 4f) * 4;

    private void OnGUI()
    {
        GUILayout.Label("Resize PNG (Single)", EditorStyles.boldLabel);
        singleTexture = (Texture2D)EditorGUILayout.ObjectField("Source PNG", singleTexture, typeof(Texture2D), false);
        newWidth = EditorGUILayout.IntField("New Width", newWidth);
        newHeight = EditorGUILayout.IntField("New Height", newHeight);
        overwrite = EditorGUILayout.Toggle("Overwrite File", overwrite);

        if (GUILayout.Button("Resize & Save"))
        {
            if (singleTexture != null)
                ResizeAndSavePNG(singleTexture, newWidth, newHeight, overwrite);
            else
                Debug.LogError("No PNG selected!");
        }

        if (GUILayout.Button("Resize & Save (Nearest /4)"))
        {
            if (singleTexture != null)
            {
                int w = RoundToNearest4(newWidth);
                int h = RoundToNearest4(newHeight);
                ResizeAndSavePNG(singleTexture, w, h, overwrite);
            }
            else
                Debug.LogError("No PNG selected!");
        }

        GUILayout.Space(10);
        GUILayout.Label("Batch Resize PNG (Selection)", EditorStyles.boldLabel);
        if (GUILayout.Button("Resize All Selected PNGs"))
        {
            foreach (var obj in Selection.objects)
            {
                var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(AssetDatabase.GetAssetPath(obj));
                if (tex != null)
                    ResizeAndSavePNG(tex, newWidth, newHeight, overwrite);
            }
        }
        if (GUILayout.Button("Resize All Selected PNGs (Nearest /4)"))
        {
            foreach (var obj in Selection.objects)
            {
                var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(AssetDatabase.GetAssetPath(obj));
                if (tex != null)
                {
                    int w = RoundToNearest4(newWidth);
                    int h = RoundToNearest4(newHeight);
                    ResizeAndSavePNG(tex, w, h, overwrite);
                }
            }
        }
    }

    private void ResizeAndSavePNG(Texture2D source, int width, int height, bool overwriteFile)
    {
        string assetPath = AssetDatabase.GetAssetPath(source);
        if (string.IsNullOrEmpty(assetPath))
        {
            Debug.LogError("Could not find the asset path!");
            return;
        }

        RenderTexture rt = RenderTexture.GetTemporary(width, height);
        Graphics.Blit(source, rt);

        RenderTexture.active = rt;
        Texture2D resized = new Texture2D(width, height, TextureFormat.RGBA32, false);
        resized.ReadPixels(new Rect(0, 0, width, height), 0, 0);
        resized.Apply();

        RenderTexture.active = null;
        RenderTexture.ReleaseTemporary(rt);

        byte[] pngData = resized.EncodeToPNG();

        string savePath = assetPath;
        if (!overwriteFile)
        {
            string dir = Path.GetDirectoryName(assetPath);
            string name = Path.GetFileNameWithoutExtension(assetPath);
            string ext = Path.GetExtension(assetPath);
            savePath = Path.Combine(dir, name + "_resized" + ext);
        }

        File.WriteAllBytes(savePath, pngData);
        AssetDatabase.Refresh();

        Debug.Log($"Resized PNG saved at: {savePath}");
    }
}