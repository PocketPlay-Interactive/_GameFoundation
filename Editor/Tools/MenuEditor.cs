using UnityEngine.SceneManagement;
using UnityEngine;
//using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.UI;
using System.Collections.Generic;
using Unity.VisualScripting;


public class MenuEditor
{
    [MenuItem("Tools/Scene/Game %s2")]
    static void OpenGameScene()
    {
        EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
        EditorSceneManager.OpenScene("Assets/Scenes/Game.unity");
    }

    [MenuItem("Tools/Scene/Change %`")]
    static void ChangeScene()
    {
        var scene = SceneManager.GetActiveScene();
        OpenGameScene();
        Debug.Log("Active Scene is '" + scene.name + "'.");
    }

    [MenuItem("Tools/Deletes/ALL DATA")]
    public static void DeleteAllData()
    {
        RuntimeStorageData.DeleteData(RuntimeStorageData.DATATYPE.PLAYER);
        RuntimeStorageData.DeleteData(RuntimeStorageData.DATATYPE.SETTING);
        PlayerPrefs.DeleteAll();
        RuntimeStorageData.DeleteAllData();
    }
}
