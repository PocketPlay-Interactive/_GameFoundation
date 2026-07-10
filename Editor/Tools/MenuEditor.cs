using UnityEngine.SceneManagement;
using UnityEngine;
//using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.UI;
using System.Collections.Generic;


public class MenuEditor
{
    [MenuItem("GameFoundation/Tools/Deletes/ALL DATA")]
    public static void DeleteAllData()
    {
        RuntimeStorageData.DeleteData(RuntimeStorageData.DATATYPE.PLAYER);
        RuntimeStorageData.DeleteData(RuntimeStorageData.DATATYPE.SETTING);
        PlayerPrefs.DeleteAll();
        RuntimeStorageData.DeleteAllData();
    }
}
