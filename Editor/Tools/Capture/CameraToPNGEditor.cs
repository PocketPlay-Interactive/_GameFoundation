using System;
using System.IO;
using UnityEditor;
using UnityEngine;

public class CameraToPNGEditor : EditorWindow
{
    private const int MinResolution = 16;
    private const int MaxResolution = 16384;

    private enum ResolutionPreset
    {
        Custom,
        Square256,
        Square512,
        Square1024,
        FullHD,
        QuadHD,
        UltraHD
    }

    [SerializeField] private Camera targetCamera;
    [SerializeField] private ResolutionPreset resolutionPreset = ResolutionPreset.Square512;
    [SerializeField] private int width = 512;
    [SerializeField] private int height = 512;
    [SerializeField] private int antiAliasing = 1;
    [SerializeField] private bool transparentBackground;
    [SerializeField] private string outputFolder = "Assets/Captures";
    [SerializeField] private string fileNamePrefix = "camera_capture";

    private Texture2D previewTexture;
    private string lastSavedPath;
    private Vector2 scrollPosition;

    [MenuItem("Tools/Capture/Camera to PNG")]
    public static void ShowWindow()
    {
        CameraToPNGEditor window = GetWindow<CameraToPNGEditor>();
        window.titleContent = new GUIContent("Camera Capture");
        window.minSize = new Vector2(460f, 460f);
        window.Show();
    }

    private void OnEnable()
    {
        titleContent = new GUIContent("Camera Capture");
    }

    private void OnDisable()
    {
        DestroyPreview();
    }

    private void OnGUI()
    {
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

        EditorGUILayout.Space(8f);
        EditorGUILayout.LabelField("Camera to PNG", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox(
            "Render a camera at a custom resolution without changing the Game view.",
            MessageType.Info);

        DrawCameraSection();
        DrawCaptureSection();
        DrawOutputSection();
        DrawActions();
        DrawPreview();

        EditorGUILayout.EndScrollView();
    }

    private void DrawCameraSection()
    {
        EditorGUILayout.Space(6f);
        EditorGUILayout.LabelField("Source", EditorStyles.boldLabel);

        targetCamera = (Camera)EditorGUILayout.ObjectField(
            new GUIContent("Camera", "Camera to render."),
            targetCamera,
            typeof(Camera),
            true);

        using (new EditorGUILayout.HorizontalScope())
        {
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Use Main Camera", GUILayout.Width(125f)))
            {
                targetCamera = Camera.main;
                if (targetCamera == null)
                    ShowNotification(new GUIContent("No camera tagged MainCamera was found."));
            }

            if (GUILayout.Button("Use Selected", GUILayout.Width(105f)))
            {
                Camera selectedCamera = GetCameraFromSelection();
                if (selectedCamera != null)
                    targetCamera = selectedCamera;
                else
                    ShowNotification(new GUIContent("The selection does not contain a Camera."));
            }
        }
    }

    private void DrawCaptureSection()
    {
        EditorGUILayout.Space(10f);
        EditorGUILayout.LabelField("Capture", EditorStyles.boldLabel);

        EditorGUI.BeginChangeCheck();
        resolutionPreset = (ResolutionPreset)EditorGUILayout.EnumPopup("Resolution", resolutionPreset);
        if (EditorGUI.EndChangeCheck())
            ApplyResolutionPreset();

        using (new EditorGUI.DisabledScope(resolutionPreset != ResolutionPreset.Custom))
        using (new EditorGUILayout.HorizontalScope())
        {
            EditorGUILayout.PrefixLabel("Size");
            width = EditorGUILayout.IntField(width, GUILayout.MinWidth(55f));
            GUILayout.Label("×", GUILayout.Width(14f));
            height = EditorGUILayout.IntField(height, GUILayout.MinWidth(55f));
            GUILayout.Label("px", GUILayout.Width(20f));
        }

        width = Mathf.Clamp(width, MinResolution, MaxResolution);
        height = Mathf.Clamp(height, MinResolution, MaxResolution);

        int[] aaValues = { 1, 2, 4, 8 };
        string[] aaLabels = { "Off", "2×", "4×", "8×" };
        antiAliasing = EditorGUILayout.IntPopup("Anti-aliasing", antiAliasing, aaLabels, aaValues);
        transparentBackground = EditorGUILayout.Toggle(
            new GUIContent("Transparent", "Temporarily renders the camera with a transparent solid background."),
            transparentBackground);

        if ((long)width * height > 33_177_600)
        {
            EditorGUILayout.HelpBox(
                "This is a very large image and may use substantial GPU and system memory.",
                MessageType.Warning);
        }
    }

    private void DrawOutputSection()
    {
        EditorGUILayout.Space(10f);
        EditorGUILayout.LabelField("Output", EditorStyles.boldLabel);

        using (new EditorGUILayout.HorizontalScope())
        {
            outputFolder = EditorGUILayout.TextField("Folder", outputFolder);
            if (GUILayout.Button("Browse…", GUILayout.Width(72f)))
                BrowseForOutputFolder();
        }

        fileNamePrefix = EditorGUILayout.TextField("File prefix", fileNamePrefix);
        EditorGUILayout.LabelField(
            "Name preview",
            $"{SanitizeFileName(fileNamePrefix)}_yyyyMMdd_HHmmss_fff.png",
            EditorStyles.miniLabel);
    }

    private void DrawActions()
    {
        EditorGUILayout.Space(12f);
        bool canCapture = targetCamera != null && !string.IsNullOrWhiteSpace(outputFolder);

        using (new EditorGUI.DisabledScope(!canCapture))
        {
            if (GUILayout.Button("Capture & Save PNG", GUILayout.Height(34f)))
                CaptureAndSave();
        }

        if (!canCapture)
        {
            EditorGUILayout.HelpBox("Assign a camera and choose an output folder.", MessageType.Warning);
        }

        if (!string.IsNullOrEmpty(lastSavedPath))
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.SelectableLabel(lastSavedPath, EditorStyles.textField, GUILayout.Height(19f));
                if (GUILayout.Button("Reveal", GUILayout.Width(62f)))
                    EditorUtility.RevealInFinder(lastSavedPath);
            }
        }
    }

    private void DrawPreview()
    {
        if (previewTexture == null)
            return;

        EditorGUILayout.Space(10f);
        EditorGUILayout.LabelField("Last Capture", EditorStyles.boldLabel);

        Rect availableRect = GUILayoutUtility.GetRect(100f, 260f, GUILayout.ExpandWidth(true));
        float aspect = (float)previewTexture.width / previewTexture.height;
        Rect previewRect = FitRect(availableRect, aspect);
        EditorGUI.DrawPreviewTexture(previewRect, previewTexture, null, ScaleMode.ScaleToFit);
    }

    private void CaptureAndSave()
    {
        string folderPath = ResolveOutputFolder(outputFolder);
        if (string.IsNullOrEmpty(folderPath))
        {
            EditorUtility.DisplayDialog("Camera Capture", "The output folder is invalid.", "OK");
            return;
        }

        try
        {
            Directory.CreateDirectory(folderPath);
            string savedPath = CreateUniquePath(folderPath, SanitizeFileName(fileNamePrefix));
            Texture2D capturedTexture = CaptureCamera(targetCamera, width, height, antiAliasing, transparentBackground);

            try
            {
                File.WriteAllBytes(savedPath, capturedTexture.EncodeToPNG());
            }
            catch
            {
                DestroyImmediate(capturedTexture);
                throw;
            }

            DestroyPreview();
            previewTexture = capturedTexture;
            lastSavedPath = savedPath;

            if (IsInsideAssetsFolder(savedPath))
                AssetDatabase.Refresh();

            Debug.Log($"Camera capture saved to: {savedPath}", targetCamera);
            ShowNotification(new GUIContent("PNG saved successfully"));
            Repaint();
        }
        catch (Exception exception)
        {
            Debug.LogException(exception);
            EditorUtility.DisplayDialog(
                "Camera Capture Failed",
                $"Could not capture or save the PNG.\n\n{exception.Message}",
                "OK");
        }
    }

    private static Texture2D CaptureCamera(Camera camera, int captureWidth, int captureHeight, int aa, bool transparent)
    {
        RenderTexture previousActive = RenderTexture.active;
        RenderTexture previousTarget = camera.targetTexture;
        CameraClearFlags previousClearFlags = camera.clearFlags;
        Color previousBackgroundColor = camera.backgroundColor;

        RenderTexture renderTexture = null;
        Texture2D texture = null;

        try
        {
            renderTexture = new RenderTexture(captureWidth, captureHeight, 24, RenderTextureFormat.ARGB32)
            {
                antiAliasing = aa,
                name = "Camera Capture (Temporary)"
            };
            renderTexture.Create();

            texture = new Texture2D(captureWidth, captureHeight, TextureFormat.RGBA32, false);
            camera.targetTexture = renderTexture;

            if (transparent)
            {
                camera.clearFlags = CameraClearFlags.SolidColor;
                camera.backgroundColor = new Color(0f, 0f, 0f, 0f);
            }

            camera.Render();
            RenderTexture.active = renderTexture;
            texture.ReadPixels(new Rect(0f, 0f, captureWidth, captureHeight), 0, 0, false);
            texture.Apply(false, false);
            return texture;
        }
        catch
        {
            if (texture != null)
                DestroyImmediate(texture);
            throw;
        }
        finally
        {
            camera.targetTexture = previousTarget;
            camera.clearFlags = previousClearFlags;
            camera.backgroundColor = previousBackgroundColor;
            RenderTexture.active = previousActive;

            if (renderTexture != null)
            {
                renderTexture.Release();
                DestroyImmediate(renderTexture);
            }
        }
    }

    private void ApplyResolutionPreset()
    {
        switch (resolutionPreset)
        {
            case ResolutionPreset.Square256: width = height = 256; break;
            case ResolutionPreset.Square512: width = height = 512; break;
            case ResolutionPreset.Square1024: width = height = 1024; break;
            case ResolutionPreset.FullHD: width = 1920; height = 1080; break;
            case ResolutionPreset.QuadHD: width = 2560; height = 1440; break;
            case ResolutionPreset.UltraHD: width = 3840; height = 2160; break;
        }
    }

    private void BrowseForOutputFolder()
    {
        string currentFolder = ResolveOutputFolder(outputFolder);
        string selectedFolder = EditorUtility.OpenFolderPanel("PNG Output Folder", currentFolder, string.Empty);
        if (string.IsNullOrEmpty(selectedFolder))
            return;

        outputFolder = ToProjectRelativePath(selectedFolder);
        GUI.FocusControl(null);
    }

    private static Camera GetCameraFromSelection()
    {
        if (Selection.activeGameObject != null)
            return Selection.activeGameObject.GetComponent<Camera>();

        return Selection.activeObject as Camera;
    }

    private static string ResolveOutputFolder(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
            return null;

        string trimmedPath = path.Trim().Trim('"');
        if (Path.IsPathRooted(trimmedPath))
            return Path.GetFullPath(trimmedPath);

        return Path.GetFullPath(Path.Combine(GetProjectRoot(), trimmedPath));
    }

    private static string ToProjectRelativePath(string path)
    {
        string fullPath = Path.GetFullPath(path).Replace('\\', '/').TrimEnd('/');
        string projectRoot = GetProjectRoot().Replace('\\', '/').TrimEnd('/');

        if (fullPath.Equals(projectRoot, StringComparison.OrdinalIgnoreCase))
            return ".";

        string rootPrefix = projectRoot + "/";
        return fullPath.StartsWith(rootPrefix, StringComparison.OrdinalIgnoreCase)
            ? fullPath.Substring(rootPrefix.Length)
            : fullPath;
    }

    private static bool IsInsideAssetsFolder(string path)
    {
        string fullPath = Path.GetFullPath(path).Replace('\\', '/');
        string assetsPath = Path.GetFullPath(Application.dataPath).Replace('\\', '/').TrimEnd('/') + "/";
        return fullPath.StartsWith(assetsPath, StringComparison.OrdinalIgnoreCase);
    }

    private static string CreateUniquePath(string folder, string prefix)
    {
        string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss_fff");
        string path = Path.Combine(folder, $"{prefix}_{timestamp}.png");
        int suffix = 1;

        while (File.Exists(path))
            path = Path.Combine(folder, $"{prefix}_{timestamp}_{suffix++}.png");

        return path;
    }

    private static string SanitizeFileName(string value)
    {
        string result = string.IsNullOrWhiteSpace(value) ? "camera_capture" : value.Trim();
        foreach (char invalidCharacter in Path.GetInvalidFileNameChars())
            result = result.Replace(invalidCharacter, '_');
        return result;
    }

    private static string GetProjectRoot()
    {
        return Directory.GetParent(Application.dataPath).FullName;
    }

    private static Rect FitRect(Rect container, float aspect)
    {
        float width = container.width;
        float height = width / aspect;

        if (height > container.height)
        {
            height = container.height;
            width = height * aspect;
        }

        return new Rect(
            container.x + (container.width - width) * 0.5f,
            container.y + (container.height - height) * 0.5f,
            width,
            height);
    }

    private void DestroyPreview()
    {
        if (previewTexture == null)
            return;

        DestroyImmediate(previewTexture);
        previewTexture = null;
    }
}
