using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Dann.GameFoundation.Editor
{
    [CustomEditor(typeof(global::ButtonSound))]
    public sealed class ButtonSoundEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            SerializedProperty soundKey = serializedObject.FindProperty("soundKey");
            string[] keys = GetSoundKeys();

            if (keys.Length == 0)
            {
                EditorGUILayout.PropertyField(soundKey);
                EditorGUILayout.HelpBox("No SoundService keys found. Add keys in SoundService -> Sounds.", MessageType.Info);
            }
            else
            {
                string[] options = new string[keys.Length + 1];
                options[0] = "";
                keys.CopyTo(options, 1);

                int index = System.Array.IndexOf(options, soundKey.stringValue);
                int nextIndex = EditorGUILayout.Popup("Sound Key", Mathf.Max(0, index), options);
                soundKey.stringValue = options[nextIndex];
            }
            serializedObject.ApplyModifiedProperties();
        }

        private static string[] GetSoundKeys()
        {
            HashSet<string> keys = new HashSet<string>();

            foreach (global::SoundService service in Resources.FindObjectsOfTypeAll<global::SoundService>())
                AddKeys(service, keys);

            foreach (string guid in AssetDatabase.FindAssets("t:Prefab"))
            {
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GUIDToAssetPath(guid));
                if (!prefab) continue;

                foreach (global::SoundService service in prefab.GetComponentsInChildren<global::SoundService>(true))
                    AddKeys(service, keys);
            }

            string[] result = new string[keys.Count];
            keys.CopyTo(result);
            System.Array.Sort(result);
            return result;
        }

        private static void AddKeys(global::SoundService service, HashSet<string> keys)
        {
            if (!service) return;

            SerializedObject serializedService = new SerializedObject(service);
            SerializedProperty sounds = serializedService.FindProperty("sounds");
            if (sounds == null) return;

            for (int i = 0; i < sounds.arraySize; i++)
            {
                SerializedProperty element = sounds.GetArrayElementAtIndex(i);
                SerializedProperty clips = element.FindPropertyRelative("clips");
                if (clips == null || clips.arraySize == 0) continue;

                string key = element.FindPropertyRelative("key")?.stringValue;

                if (!string.IsNullOrWhiteSpace(key))
                    keys.Add(key);
            }
        }
    }
}
