using UnityEngine;
using UnityEditor;
using System.IO;
using UnityEditor.Build.Reporting;
using System.Linq;
using UnityEditor.SceneManagement;
using Unity.VisualScripting;
using System;

public class CustomBuildMenu
{
    [MenuItem("Build/Setup Build Password")]
    public static void SetupPasswordAndAndroidInfo()
    {
        PlayerSettings.Android.keystorePass = "123456";
        PlayerSettings.Android.keyaliasPass = "123456";
        EditorUtility.DisplayDialog("Build Password", "Đã setup password, Android info và company name từ ScriptableObject!", "OK");
    }
}