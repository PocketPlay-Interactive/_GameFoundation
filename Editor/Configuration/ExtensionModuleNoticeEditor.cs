using System;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(MonoBehaviour), true)]
public class ExtensionModuleNoticeEditor : Editor
{
    public override void OnInspectorGUI()
    {
        var notice = Attribute.GetCustomAttribute(target.GetType(), typeof(ExtensionModuleNoticeAttribute), true) as ExtensionModuleNoticeAttribute;
        if (notice != null)
            EditorGUILayout.HelpBox(notice.Message, MessageType.Info);

        DrawDefaultInspector();
    }
}
