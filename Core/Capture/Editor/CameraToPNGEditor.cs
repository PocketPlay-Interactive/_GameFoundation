using UnityEngine;
using UnityEditor;
using System.IO;

public class CameraToPNGEditor : EditorWindow
{
    Camera targetCamera;
    int width = 512;
    int height = 512;

    [MenuItem("Capture/Editor")]
    public static void ShowWindow()
    {
        GetWindow<CameraToPNGEditor>("Capture");
    }

    void OnGUI()
    {
        targetCamera = EditorGUILayout.ObjectField("Camera", targetCamera, typeof(Camera), true) as Camera;
        width = EditorGUILayout.IntField("Width", width);
        height = EditorGUILayout.IntField("Height", height);

        if (GUILayout.Button("Capture & Save PNG"))
        {
            if (targetCamera != null)
            {
                CaptureCameraToPNG(targetCamera, width, height);
            }
            else
            {
                Debug.LogWarning("Camera is null!");
            }
        }
    }

    void CaptureCameraToPNG(Camera cam, int width, int height)
    {
        // Set up RenderTexture
        RenderTexture rt = new RenderTexture(width, height, 24);
        cam.targetTexture = rt;
        Texture2D screenShot = new Texture2D(width, height, TextureFormat.RGBA32, false);

        cam.Render(); // Render manually

        RenderTexture.active = rt;
        screenShot.ReadPixels(new Rect(0, 0, width, height), 0, 0);
        screenShot.Apply();

        // Cleanup
        cam.targetTexture = null;
        RenderTexture.active = null;
        DestroyImmediate(rt);

        // Encode PNG
        byte[] bytes = screenShot.EncodeToPNG();

        // Đường dẫn mặc định tới folder Capture trong Assets
        string folder = Application.dataPath + "/Capture";
        if (!Directory.Exists(folder))
            Directory.CreateDirectory(folder);

        // Tạo tên file duy nhất theo thời gian
        string fileName = $"camera_capture_{System.DateTime.Now:yyyyMMdd_HHmmss}.png";
        string path = Path.Combine(folder, fileName);

        File.WriteAllBytes(path, bytes);
        AssetDatabase.Refresh();
        Debug.Log($"Saved PNG to: {path}");
    }
}
