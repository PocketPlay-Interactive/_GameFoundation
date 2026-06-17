using UnityEditor;
using UnityEditor.PackageManager.Requests;
using UnityEditor.PackageManager;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public static class AutoPackageDownloader
{
    // Danh sách các package cần tải
    private static readonly string[] PackagesToInstall = new[]
    {
        "https://github.com/Cysharp/UniTask.git?path=src/UniTask/Assets/Plugins/UniTask",
        "com.unity.ide.visualstudio"
        // Thêm package khác nếu muốn
    };

    [MenuItem("Packages/Download All Packages")]
    public static void DownloadAllPackages()
    {
        Debug.Log("[AutoPackageDownloader] Checking installed packages...");
        ListRequest listRequest = Client.List(true);
        _currentListRequest = listRequest;
        EditorApplication.update += CheckAndInstall;
    }

    private static ListRequest _currentListRequest;
    private static void CheckAndInstall()
    {
        if (!_currentListRequest.IsCompleted) return;

        if (_currentListRequest.Status == StatusCode.Success)
        {
            var installed = _currentListRequest.Result.Select(pkg => pkg.name).ToHashSet();
            var toInstall = PackagesToInstall
                .Where(pkg => !installed.Any(inst => pkg.Contains(inst)))
                .ToList();

            if (toInstall.Count == 0)
            {
                Debug.Log("[AutoPackageDownloader] All packages already installed.");
            }
            else
            {
                Debug.Log($"[AutoPackageDownloader] Installing: {string.Join(", ", toInstall)}");
                InstallPackages(toInstall);
            }
        }
        else
        {
            Debug.LogError("[AutoPackageDownloader] Failed to list packages: " + _currentListRequest.Error.message);
        }

        EditorApplication.update -= CheckAndInstall;
    }

    private static Queue<string> _installQueue;
    private static AddRequest _currentRequest;

    private static void InstallPackages(List<string> packages)
    {
        _installQueue = new Queue<string>(packages);
        InstallNext();
    }

    private static void InstallNext()
    {
        if (_installQueue.Count == 0)
        {
            Debug.Log("[AutoPackageDownloader] All selected packages installed.");
            return;
        }

        string pkg = _installQueue.Dequeue();
        Debug.Log($"[AutoPackageDownloader] Installing package: {pkg}");
        _currentRequest = Client.Add(pkg);
        EditorApplication.update += Progress;
    }

    private static void Progress()
    {
        if (!_currentRequest.IsCompleted) return;

        if (_currentRequest.Status == StatusCode.Success)
            Debug.Log($"[AutoPackageDownloader] Successfully installed: {_currentRequest.Result.packageId}");
        else if (_currentRequest.Status >= StatusCode.Failure)
            Debug.LogError($"[AutoPackageDownloader] Failed to install: {_currentRequest.Error.message}");

        EditorApplication.update -= Progress;
        InstallNext();
    }
}