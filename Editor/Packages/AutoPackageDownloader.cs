using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEngine;

public static class AutoPackageDownloader
{
    private const string DefaultGameFoundationRepositoryUrl = "https://github.com/PocketPlay-Interactive/_GameFoundation.git";
    private const string UnityIapPackageName = "com.unity.purchasing";

    // Danh sach cac package can tai.
    private static readonly string[] PackagesToInstall =
    {
        "https://github.com/Cysharp/UniTask.git?path=src/UniTask/Assets/Plugins/UniTask",
        "com.unity.ide.visualstudio"
        // Them package khac neu muon.
    };

    private static ListRequest _currentListRequest;
    private static Queue<string> _installQueue;
    private static AddRequest _currentRequest;

    [MenuItem("GameFoundation/Packages/Install Dependencies")]
    public static void DownloadAllPackages()
    {
        Debug.Log("[AutoPackageDownloader] Checking installed packages...");
        _currentListRequest = Client.List(true);
        EditorApplication.update += CheckAndInstall;
    }

    [MenuItem("GameFoundation/Packages/Upgrade GameFoundation")]
    public static void UpgradeGameFoundation()
    {
        Debug.Log("[AutoPackageDownloader] Upgrading GameFoundation: " + DefaultGameFoundationRepositoryUrl);
        _currentRequest = Client.Add(DefaultGameFoundationRepositoryUrl);
        EditorApplication.update += Progress;
    }

    [MenuItem("GameFoundation/Packages/Install InappPurchase")]
    public static void InstallInappPurchase()
    {
        Debug.Log("[AutoPackageDownloader] Installing Unity IAP: " + UnityIapPackageName);
        _currentRequest = Client.Add(UnityIapPackageName);
        EditorApplication.update += Progress;
    }

    private static void CheckAndInstall()
    {
        if (!_currentListRequest.IsCompleted)
            return;

        if (_currentListRequest.Status == StatusCode.Success)
        {
            var installed = _currentListRequest.Result.Select(pkg => pkg.name).ToHashSet();
            var toInstall = PackagesToInstall
                .Where(pkg => !installed.Any(installedPackage => pkg.Contains(installedPackage)))
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
            _installQueue = null;
            return;
        }

        string packageName = _installQueue.Dequeue();
        Debug.Log($"[AutoPackageDownloader] Installing package: {packageName}");
        _currentRequest = Client.Add(packageName);
        EditorApplication.update += Progress;
    }

    private static void Progress()
    {
        if (!_currentRequest.IsCompleted)
            return;

        if (_currentRequest.Status == StatusCode.Success)
            Debug.Log($"[AutoPackageDownloader] Successfully installed: {_currentRequest.Result.packageId}");
        else if (_currentRequest.Status >= StatusCode.Failure)
            Debug.LogError($"[AutoPackageDownloader] Failed to install: {_currentRequest.Error.message}");

        EditorApplication.update -= Progress;
        if (_installQueue != null)
            InstallNext();
    }

}
