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
    [MenuItem("Tools/Deletes/ALL DATA")]
    public static void DeleteAllData()
    {
        RuntimeStorageData.DeleteData(RuntimeStorageData.DATATYPE.PLAYER);
        RuntimeStorageData.DeleteData(RuntimeStorageData.DATATYPE.SETTING);
        PlayerPrefs.DeleteAll();
        RuntimeStorageData.DeleteAllData();
    }
}
