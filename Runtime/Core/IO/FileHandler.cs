using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

public static class FileHandler
{

    public static void SaveToJSON<T>(List<T> toSave, string filename)
    {
        Debug.Log(GetPath(filename));
        string content = UtilsJsonHelper.ToJsonArray(toSave.ToArray());
        WriteFile(GetPath(filename), content);
    }

    public static void SaveToJSON<T>(T toSave, string filename)
    {
        string content = JsonUtility.ToJson(toSave);
        WriteFile(GetPath(filename), content);
    }

    public static void SaveFile(string content, string filename)
    {
        var _path = GetPath(filename);
        Debug.Log(_path);
        WriteFile(_path, content);
#if UNITY_EDITOR
        UnityEditor.AssetDatabase.Refresh();
#endif
    }

    public static List<T> ReadListFromJSON<T>(string filename)
    {
        string content = ReadFile(GetPath(filename));

        if (string.IsNullOrEmpty(content) || content == "{}")
        {
            return new List<T>();
        }

        List<T> res = UtilsJsonHelper.FromJsonDataArray<T>(content).ToList();

        return res;

    }

    public static T ReadFromJSON<T>(string filename)
    {
        string content = ReadFile(GetPath(filename));

        if (string.IsNullOrEmpty(content) || content == "{}")
        {
            return default(T);
        }

        T res = JsonUtility.FromJson<T>(content);

        return res;

    }

    public static string GetPath(string filename)
    {
        return Application.dataPath + "/" + filename;
    }

    public static void WriteFile(string path, string content)
    {
        string directory = Path.GetDirectoryName(path);
        if (!string.IsNullOrEmpty(directory))
            Directory.CreateDirectory(directory);

        File.WriteAllText(path, content);
    }

    public static bool Exists(string path)
    {
        if (File.Exists(path))
            return true;
        return false;
    }

    public static string ReadFile(string path)
    {
        return File.Exists(path) ? File.ReadAllText(path) : "";
    }

    public static string Between(string STR, string FirstString, string LastString)
    {
        STR = Regex.Replace(STR, @"\t|\n|\r|", "");
        STR = Regex.Replace(STR, @"\s+", "");
        STR = STR.Replace("\"", "");
        string FinalString;
        int Pos1 = STR.IndexOf(FirstString)+ FirstString.Length;
        int Pos2 = STR.IndexOf(LastString);
        FinalString = STR.Substring(Pos1, Pos2-Pos1);       
        return FinalString;
    }

    public static Dictionary<string, string> GetDict(string stringValue)
    {
        var result = new Dictionary<string, string>();
        string value = stringValue;// File.ReadAllText(file);
        // Split the string.
        string[] tokens = value.Split(new char[] { ':', ',' }, StringSplitOptions.RemoveEmptyEntries);
        // Build up our dictionary from the string.
        for (int i = 0; i < tokens.Length; i += 2)
        {
            string name = tokens[i];
            string freq = tokens[i + 1];

            // Parse the int.
           // int count = int.Parse(freq);
            // Add the value to our dictionary.
            if (result.ContainsKey(name))
            {
                result[name] = freq;
            }
            else
            {
                result.Add(name, freq);
            }
        }
        
        return result;
    }
}
