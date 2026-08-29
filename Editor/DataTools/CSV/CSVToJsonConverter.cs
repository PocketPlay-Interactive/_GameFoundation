using System.IO;
using System.Text.RegularExpressions;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
#endif

public static class CSVToJsonConverter
{
    // Hàm chuyển đổi CSV sang JSON, trả về chuỗi JSON, có thể lưu file nếu muốn
    public static string Convert(
        string csvFilePath,
        string outputFilePath = null,
        char separator = ',',
        bool hasHeaderRow = true,
        bool prettyPrint = true)
    {
        if (!File.Exists(csvFilePath))
        {
#if UNITY_EDITOR
            Debug.LogError($"CSV file not found at: {csvFilePath}");
#endif
            return null;
        }

        var lines = File.ReadAllLines(csvFilePath);
        if (lines.Length == 0) return "[]";

        var headers = new List<string>();
        int dataStart = 0;
        string pattern = $"{separator}(?=(?:[^\"]*\"[^\"]*\")*[^\"]*$)";

        if (hasHeaderRow)
        {
            headers.AddRange(Regex.Split(lines[0], pattern));
            for (int i = 0; i < headers.Count; i++)
                headers[i] = headers[i].Trim('"');
            dataStart = 1;
        }
        else
        {
            int cols = Regex.Split(lines[0], pattern).Length;
            for (int i = 0; i < cols; i++) headers.Add($"Column{i + 1}");
        }

        var rows = new List<Dictionary<string, string>>();
        for (int i = dataStart; i < lines.Length; i++)
        {
            if (string.IsNullOrWhiteSpace(lines[i])) continue;
            var values = Regex.Split(lines[i], pattern);
            if (values.Length != headers.Count) continue;
            var row = new Dictionary<string, string>();
            for (int j = 0; j < values.Length; j++)
            {
                var val = values[j].Trim().Trim('"').Replace("\"\"", "\"");
                row[headers[j]] = val;
            }
            rows.Add(row);
        }

        var json = BuildJson(rows, prettyPrint);

        if (!string.IsNullOrEmpty(outputFilePath))
        {
            var dir = Path.GetDirectoryName(outputFilePath);
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
            File.WriteAllText(outputFilePath, json);
#if UNITY_EDITOR
            AssetDatabase.Refresh();
#endif
        }
        return json;
    }

    // Hàm build JSON đơn giản
    private static string BuildJson(List<Dictionary<string, string>> rows, bool pretty)
    {
        var sb = new System.Text.StringBuilder();
        sb.Append("[");
        for (int i = 0; i < rows.Count; i++)
        {
            if (pretty) sb.Append("\n  ");
            sb.Append("{");
            int k = 0;
            foreach (var pair in rows[i])
            {
                if (k++ > 0) sb.Append(",");
                if (pretty) sb.Append(" ");
                sb.Append($"\"{Escape(pair.Key)}\":\"{Escape(pair.Value)}\"");
            }
            sb.Append("}");
            if (i < rows.Count - 1) sb.Append(",");
        }
        if (pretty && rows.Count > 0) sb.Append("\n");
        sb.Append("]");
        return sb.ToString();
    }

    private static string Escape(string s)
    {
        return s.Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\n", "\\n").Replace("\r", "\\r").Replace("\t", "\\t");
    }
}