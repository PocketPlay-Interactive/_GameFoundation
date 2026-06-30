using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEngine;

public static class AutoPackageDownloader
{
    private const string GameFoundationRepositoryUrlPrefsKey = "AutoPackageDownloader.GameFoundationRepositoryUrl";
    private const string GameFoundationAssetPath = "Assets/GameFoundation";

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

    [MenuItem("Packages/Download All Packages")]
    public static void DownloadAllPackages()
    {
        Debug.Log("[AutoPackageDownloader] Checking installed packages...");
        _currentListRequest = Client.List(true);
        EditorApplication.update += CheckAndInstall;
    }

    [MenuItem("Packages/Download Git GameFoundation")]
    public static void DownloadGameFoundationFromGit()
    {
        Dictionary<string, byte[]> localMetaFiles = null;
        bool stashedMetaChanges = false;

        try
        {
            string targetPath = Path.Combine(Directory.GetCurrentDirectory(), GameFoundationAssetPath);
            localMetaFiles = BackupLocalMetaFiles(targetPath);

            if (Directory.Exists(targetPath))
            {
                if (!Directory.Exists(Path.Combine(targetPath, ".git")))
                {
                    if (!EditorUtility.DisplayDialog(
                            "Replace GameFoundation",
                            "Assets/GameFoundation exists but is not a git repository. Replace it with the latest version cloned from git?",
                            "Replace",
                            "Cancel"))
                    {
                        Debug.Log("[AutoPackageDownloader] GameFoundation update canceled.");
                        return;
                    }

                    CloneGameFoundation(targetPath, true);
                }
                else
                {
                    Debug.Log("[AutoPackageDownloader] Pulling latest GameFoundation from git...");
                    stashedMetaChanges = StashLocalMetaChanges(targetPath);
                    RunGitCommand("pull --ff-only", targetPath);
                }
            }
            else
            {
                CloneGameFoundation(targetPath, false);
            }

            RestoreLocalMetaFiles(localMetaFiles);
            DropStashedMetaChanges(targetPath, stashedMetaChanges);
            stashedMetaChanges = false;

            AssetDatabase.Refresh();
            Debug.Log("[AutoPackageDownloader] GameFoundation is up to date.");
        }
        catch (System.Exception ex)
        {
            RestoreLocalMetaFiles(localMetaFiles);

            string targetPath = Path.Combine(Directory.GetCurrentDirectory(), GameFoundationAssetPath);
            DropStashedMetaChanges(targetPath, stashedMetaChanges);

            Debug.LogError("[AutoPackageDownloader] Failed to update GameFoundation: " + ex.Message);
        }
    }

    private static Dictionary<string, byte[]> BackupLocalMetaFiles(string targetPath)
    {
        var metaFiles = new Dictionary<string, byte[]>(StringComparer.OrdinalIgnoreCase);

        if (Directory.Exists(targetPath))
        {
            foreach (string metaPath in Directory.GetFiles(targetPath, "*.meta", SearchOption.AllDirectories))
                metaFiles[metaPath] = File.ReadAllBytes(metaPath);
        }

        string rootMetaPath = targetPath + ".meta";
        if (File.Exists(rootMetaPath))
            metaFiles[rootMetaPath] = File.ReadAllBytes(rootMetaPath);

        Debug.Log($"[AutoPackageDownloader] Preserving {metaFiles.Count} local GameFoundation meta files.");
        return metaFiles;
    }

    private static void RestoreLocalMetaFiles(Dictionary<string, byte[]> metaFiles)
    {
        if (metaFiles == null)
            return;

        foreach (var metaFile in metaFiles)
        {
            string directory = Path.GetDirectoryName(metaFile.Key);
            if (!string.IsNullOrEmpty(directory))
                Directory.CreateDirectory(directory);

            File.WriteAllBytes(metaFile.Key, metaFile.Value);
        }
    }

    private static bool StashLocalMetaChanges(string targetPath)
    {
        string status = RunGitCommand("status --porcelain -- \"*.meta\"", targetPath);
        if (string.IsNullOrWhiteSpace(status))
            return false;

        Debug.Log("[AutoPackageDownloader] Temporarily stashing local GameFoundation meta changes...");
        RunGitCommand(
            "stash push --include-untracked -m \"AutoPackageDownloader preserved meta files\" -- \"*.meta\"",
            targetPath);
        return true;
    }

    private static void DropStashedMetaChanges(string targetPath, bool stashedMetaChanges)
    {
        if (!stashedMetaChanges || !Directory.Exists(Path.Combine(targetPath, ".git")))
            return;

        RunGitCommand("stash drop stash@{0}", targetPath);
    }

    private static void CloneGameFoundation(string targetPath, bool replaceExisting)
    {
        string repositoryUrl = EditorPrefs.GetString(GameFoundationRepositoryUrlPrefsKey, string.Empty);
        if (string.IsNullOrWhiteSpace(repositoryUrl))
        {
            throw new System.InvalidOperationException(
                "GameFoundation git URL is empty. Use GameFoundation/Configure Git Repository URL first.");
        }

        if (replaceExisting)
        {
            FileUtil.DeleteFileOrDirectory(targetPath);
            FileUtil.DeleteFileOrDirectory(targetPath + ".meta");
        }

        Directory.CreateDirectory(Path.GetDirectoryName(targetPath));
        Debug.Log("[AutoPackageDownloader] Cloning GameFoundation from git...");
        RunGitCommand($"clone \"{repositoryUrl}\" \"{targetPath}\"");
    }

    [MenuItem("GameFoundation/Configure Git Repository URL")]
    public static void OpenGameFoundationRepositoryUrlWindow()
    {
        GameFoundationGitUrlWindow.ShowWindow();
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
        InstallNext();
    }

    private static string RunGitCommand(string arguments, string workingDirectory = null)
    {
        var process = new System.Diagnostics.Process
        {
            StartInfo = new System.Diagnostics.ProcessStartInfo
            {
                FileName = "git",
                Arguments = arguments,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true,
                WorkingDirectory = workingDirectory ?? Directory.GetCurrentDirectory()
            }
        };

        process.Start();
        string output = process.StandardOutput.ReadToEnd();
        string error = process.StandardError.ReadToEnd();
        process.WaitForExit();

        if (process.ExitCode != 0)
            throw new System.InvalidOperationException(error);

        if (!string.IsNullOrWhiteSpace(output))
            Debug.Log("[AutoPackageDownloader] " + output);

        if (!string.IsNullOrWhiteSpace(error))
            Debug.Log("[AutoPackageDownloader] " + error);

        return output;
    }

    private class GameFoundationGitUrlWindow : EditorWindow
    {
        private string _repositoryUrl;

        public static void ShowWindow()
        {
            var window = GetWindow<GameFoundationGitUrlWindow>("GameFoundation Git");
            window.minSize = new UnityEngine.Vector2(420, 90);
            window._repositoryUrl = EditorPrefs.GetString(GameFoundationRepositoryUrlPrefsKey, string.Empty);
            window.Show();
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField("GameFoundation Git Repository", EditorStyles.boldLabel);
            _repositoryUrl = EditorGUILayout.TextField("Git URL", _repositoryUrl);

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Save"))
            {
                EditorPrefs.SetString(GameFoundationRepositoryUrlPrefsKey, _repositoryUrl.Trim());
                Debug.Log("[AutoPackageDownloader] Saved GameFoundation git URL: " + _repositoryUrl.Trim());
                Close();
            }

            if (GUILayout.Button("Clear"))
            {
                _repositoryUrl = string.Empty;
                EditorPrefs.DeleteKey(GameFoundationRepositoryUrlPrefsKey);
                Debug.Log("[AutoPackageDownloader] Cleared GameFoundation git URL.");
            }
            EditorGUILayout.EndHorizontal();
        }
    }
}
