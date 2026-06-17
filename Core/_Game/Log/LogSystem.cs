using System.Collections.Generic;
using UnityEngine;
using System.Runtime.CompilerServices;

public static class LogSystem
{
    public static void LogByColor(string text, string color) =>
        Debug.Log($"<color={color}>{text}</color>");

    public static void LogWarning(string text) => LogByColor(text, "yellow");
    public static void LogError(string text) => LogByColor(text, "red");
    public static void LogError(object text) => LogByColor(text?.ToString() ?? "null", "red");
    public static void LogSuccess(string text) => LogByColor(text, "green");

    public static void ShowCallerInfo(string message,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string sourceFilePath = "",
        [CallerLineNumber] int sourceLineNumber = 0)
    {
        Debug.Log($"message: {message}\nmember: {memberName}\nfile: {sourceFilePath}\nline: {sourceLineNumber}");
    }

    public static void LogArray<T>(params T[] arr) =>
        Debug.Log("[ " + string.Join(", ", arr) + " ]");

    public static void LogList<T>(List<T> list) =>
        Debug.Log("[ " + string.Join(", ", list) + " ]");
}
