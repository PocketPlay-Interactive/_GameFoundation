using System.IO;
using UnityEditor;
using UnityEngine;

public class PNGResizerTool : EditorWindow
{
    private const int MinSize = 1;
    private const int ButtonHeight = 28;

    private int newWidth = 256;
    private int newHeight = 256;
    private bool overwrite;
    private bool trimTransparentPixels;
    private Vector2 scrollPosition;

    [MenuItem("Utils/PNG Resizer")]
    public static void ShowWindow()
    {
        PNGResizerTool window = GetWindow<PNGResizerTool>("PNG Resizer");
        window.minSize = new Vector2(380f, 300f);
    }

    private int RoundToNearest4(int value)
    {
        return Mathf.Max(4, Mathf.RoundToInt(value / 4f) * 4);
    }

    private void OnGUI()
    {
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

        DrawHeader();
        EditorGUILayout.Space(8f);
        DrawSettingsSection();
        EditorGUILayout.Space(10f);
        DrawBatchResizeSection();

        EditorGUILayout.EndScrollView();
    }

    private void OnSelectionChange()
    {
        Repaint();
    }

    private void DrawHeader()
    {
        GUILayout.Label("PNG Resizer", new GUIStyle(EditorStyles.boldLabel)
        {
            alignment = TextAnchor.MiddleCenter,
            fontSize = 16
        });
        GUILayout.Label("Resize selected PNGs in the Project window.", EditorStyles.centeredGreyMiniLabel);
    }

    private void DrawSettingsSection()
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        GUILayout.Label("Output Settings", EditorStyles.boldLabel);

        newWidth = Mathf.Max(MinSize, EditorGUILayout.IntField("Width", newWidth));
        newHeight = Mathf.Max(MinSize, EditorGUILayout.IntField("Height", newHeight));
        overwrite = EditorGUILayout.Toggle(
            new GUIContent("Overwrite Source", "Replace the original PNG instead of creating a _resized copy."),
            overwrite);
        trimTransparentPixels = EditorGUILayout.Toggle(
            new GUIContent("Trim Transparent Border", "Crop fully transparent borders before resizing."),
            trimTransparentPixels);

        if (trimTransparentPixels)
        {
            EditorGUILayout.HelpBox(
                "Transparent borders are removed before scaling, so the saved PNG still uses the requested output size.",
                MessageType.Info);
        }

        int roundedWidth = RoundToNearest4(newWidth);
        int roundedHeight = RoundToNearest4(newHeight);
        EditorGUILayout.LabelField("Nearest Multiple of 4", $"{roundedWidth} x {roundedHeight}");
        EditorGUILayout.EndVertical();
    }

    private void DrawBatchResizeSection()
    {
        int selectedPngCount = GetSelectedTextureCount();

        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        GUILayout.Label("Resize Selection", EditorStyles.boldLabel);
        EditorGUILayout.LabelField("Selected PNGs", selectedPngCount.ToString());

        using (new EditorGUI.DisabledScope(selectedPngCount == 0))
        {
            if (GUILayout.Button("Resize Selected PNGs", GUILayout.Height(ButtonHeight)))
            {
                ResizeSelectedPNGs(newWidth, newHeight);
            }

            if (GUILayout.Button("Resize Selected PNGs (Multiple of 4)", GUILayout.Height(ButtonHeight)))
            {
                ResizeSelectedPNGs(RoundToNearest4(newWidth), RoundToNearest4(newHeight));
            }
        }

        if (selectedPngCount == 0)
        {
            EditorGUILayout.HelpBox("Select one or more PNGs in the Project window.", MessageType.None);
        }

        EditorGUILayout.EndVertical();
    }

    private int GetSelectedTextureCount()
    {
        int count = 0;
        foreach (Object selectedObject in Selection.objects)
        {
            string path = AssetDatabase.GetAssetPath(selectedObject);
            if (AssetDatabase.LoadAssetAtPath<Texture2D>(path) != null &&
                string.Equals(Path.GetExtension(path), ".png", System.StringComparison.OrdinalIgnoreCase))
            {
                count++;
            }
        }

        return count;
    }

    private void ResizeSelectedPNGs(int width, int height)
    {
        int resizedCount = 0;
        foreach (Object selectedObject in Selection.objects)
        {
            string path = AssetDatabase.GetAssetPath(selectedObject);
            Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            if (texture == null || !string.Equals(Path.GetExtension(path), ".png", System.StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            ResizeAndSavePNG(texture, width, height, overwrite, trimTransparentPixels);
            resizedCount++;
        }

        Debug.Log($"PNG Resizer: processed {resizedCount} selected PNG(s).");
    }

    private void ResizeAndSavePNG(
        Texture2D source,
        int width,
        int height,
        bool overwriteFile,
        bool trimTransparentBorder)
    {
        string assetPath = AssetDatabase.GetAssetPath(source);
        if (string.IsNullOrEmpty(assetPath) || !string.Equals(Path.GetExtension(assetPath), ".png", System.StringComparison.OrdinalIgnoreCase))
        {
            Debug.LogError("PNG Resizer: the source must be a PNG asset.");
            return;
        }

        Texture2D readableSource = null;
        Texture2D trimmedSource = null;
        Texture2D resized = null;

        try
        {
            Texture sourceToResize = source;
            if (trimTransparentBorder)
            {
                readableSource = CreateReadableCopy(source);
                trimmedSource = TrimTransparentBorder(readableSource);
                sourceToResize = trimmedSource;
            }

            resized = ResizeTexture(sourceToResize, width, height);
            byte[] pngData = resized.EncodeToPNG();
            string savePath = GetSavePath(assetPath, overwriteFile);

            File.WriteAllBytes(savePath, pngData);
            AssetDatabase.ImportAsset(savePath, ImportAssetOptions.ForceUpdate);
            Debug.Log($"PNG Resizer: saved {width} x {height} PNG at {savePath}");
        }
        finally
        {
            DestroyImmediate(resized);
            DestroyImmediate(trimmedSource);
            DestroyImmediate(readableSource);
        }
    }

    private static Texture2D CreateReadableCopy(Texture source)
    {
        return ResizeTexture(source, source.width, source.height);
    }

    private static Texture2D ResizeTexture(Texture source, int width, int height)
    {
        RenderTexture previous = RenderTexture.active;
        RenderTexture temporary = RenderTexture.GetTemporary(width, height, 0, RenderTextureFormat.ARGB32);

        try
        {
            Graphics.Blit(source, temporary);
            RenderTexture.active = temporary;

            Texture2D result = new Texture2D(width, height, TextureFormat.RGBA32, false);
            result.ReadPixels(new Rect(0, 0, width, height), 0, 0);
            result.Apply();
            return result;
        }
        finally
        {
            RenderTexture.active = previous;
            RenderTexture.ReleaseTemporary(temporary);
        }
    }

    private static Texture2D TrimTransparentBorder(Texture2D source)
    {
        Color32[] pixels = source.GetPixels32();
        int minX = source.width;
        int minY = source.height;
        int maxX = -1;
        int maxY = -1;

        for (int y = 0; y < source.height; y++)
        {
            for (int x = 0; x < source.width; x++)
            {
                if (pixels[y * source.width + x].a == 0)
                {
                    continue;
                }

                minX = Mathf.Min(minX, x);
                minY = Mathf.Min(minY, y);
                maxX = Mathf.Max(maxX, x);
                maxY = Mathf.Max(maxY, y);
            }
        }

        if (maxX < minX || maxY < minY)
        {
            Texture2D empty = new Texture2D(1, 1, TextureFormat.RGBA32, false);
            empty.SetPixel(0, 0, Color.clear);
            empty.Apply();
            return empty;
        }

        int trimmedWidth = maxX - minX + 1;
        int trimmedHeight = maxY - minY + 1;
        Color32[] trimmedPixels = new Color32[trimmedWidth * trimmedHeight];

        for (int y = 0; y < trimmedHeight; y++)
        {
            for (int x = 0; x < trimmedWidth; x++)
            {
                trimmedPixels[y * trimmedWidth + x] = pixels[(minY + y) * source.width + minX + x];
            }
        }

        Texture2D trimmed = new Texture2D(trimmedWidth, trimmedHeight, TextureFormat.RGBA32, false);
        trimmed.SetPixels32(trimmedPixels);
        trimmed.Apply();
        return trimmed;
    }

    private static string GetSavePath(string assetPath, bool overwriteFile)
    {
        if (overwriteFile)
        {
            return assetPath;
        }

        string directory = Path.GetDirectoryName(assetPath);
        string name = Path.GetFileNameWithoutExtension(assetPath);
        string extension = Path.GetExtension(assetPath);
        return Path.Combine(directory ?? string.Empty, name + "_resized" + extension).Replace('\\', '/');
    }
}
