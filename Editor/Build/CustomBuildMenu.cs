using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

public sealed class CustomBuildMenu : EditorWindow
{
    private const string WindowTitle = "Build & Store Settings";

    private Vector2 scrollPosition;

    private string companyName;
    private string productName;
    private string version;

    private string androidApplicationId;
    private int androidVersionCode;
    private AndroidSdkVersions androidMinSdkVersion;
    private AndroidSdkVersions androidTargetSdkVersion;
    private AndroidArchitecture androidArchitectures;
    private bool buildAppBundle;
    private string keystorePath;
    private string keystorePassword;
    private string keyAlias;
    private string keyAliasPassword;

    private string iosApplicationId;
    private string iosBuildNumber;
    private string iosMinimumVersion;
    private string appleDeveloperTeamId;
    private bool appleAutomaticSigning;

    private bool developmentBuild;

    private static string SigningPrefsPrefix => $"CustomBuildMenu.{Hash128.Compute(Application.dataPath)}.AndroidSigning.";

    [MenuItem("Build/Build & Store Settings...")]
    public static void ShowWindow()
    {
        CustomBuildMenu window = GetWindow<CustomBuildMenu>();
        window.titleContent = new GUIContent(WindowTitle, EditorGUIUtility.IconContent("BuildSettings.Editor").image);
        window.minSize = new Vector2(560f, 620f);
        window.Show();
    }

    private void OnEnable()
    {
        LoadPlayerSettings();
        LoadSigningPreferences();
    }

    private void OnDisable()
    {
        SaveSigningPreferences();
    }

    private void OnGUI()
    {
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

        DrawHeader();
        DrawApplicationSection();
        DrawAndroidSection();
        DrawIosSection();
        DrawBuildSection();

        EditorGUILayout.EndScrollView();
    }

    private void DrawHeader()
    {
        EditorGUILayout.Space(8f);
        EditorGUILayout.LabelField(WindowTitle, EditorStyles.boldLabel);
        EditorGUILayout.HelpBox(
            "Các giá trị tại đây được ghi trực tiếp vào Player Settings. Version là phiên bản người dùng nhìn thấy; Android Version Code và iOS Build Number phải tăng sau mỗi lần đưa bản mới lên store.",
            MessageType.Info);
        EditorGUILayout.Space(6f);
    }

    private void DrawApplicationSection()
    {
        BeginSection("Thông tin ứng dụng");
        companyName = EditorGUILayout.TextField(new GUIContent("Company Name"), companyName);
        productName = EditorGUILayout.TextField(new GUIContent("Product Name"), productName);
        using (new EditorGUILayout.HorizontalScope())
        {
            version = EditorGUILayout.TextField(
                new GUIContent("Version", "Phiên bản hiển thị trên store, ví dụ 1.2.0."),
                version);
            if (GUILayout.Button("Tăng patch", GUILayout.Width(82f)))
            {
                version = IncrementPatchVersion(version);
                GUI.FocusControl(null);
            }
        }
        EndSection();
    }

    private void DrawAndroidSection()
    {
        BeginSection("Android / Google Play");
        androidApplicationId = EditorGUILayout.TextField(
            new GUIContent("Application ID", "Package name duy nhất, ví dụ com.company.game."),
            androidApplicationId);
        EditorGUILayout.HelpBox("Giữ nguyên Application ID nếu đây là bản cập nhật của app đã có trên Google Play.", MessageType.None);
        using (new EditorGUILayout.HorizontalScope())
        {
            androidVersionCode = EditorGUILayout.IntField(
                new GUIContent("Version Code", "Số nguyên phải tăng sau mỗi bản phát hành."),
                androidVersionCode);
            if (GUILayout.Button("+1", GUILayout.Width(82f)))
            {
                androidVersionCode = Math.Max(1, androidVersionCode + 1);
                GUI.FocusControl(null);
            }
        }
        buildAppBundle = EditorGUILayout.Toggle(
            new GUIContent("Build App Bundle", "Xuất file .aab để phát hành trên Google Play."),
            buildAppBundle);
        androidMinSdkVersion = (AndroidSdkVersions)EditorGUILayout.EnumPopup("Minimum API Level", androidMinSdkVersion);
        androidTargetSdkVersion = (AndroidSdkVersions)EditorGUILayout.EnumPopup(
            new GUIContent("Target API Level", "Automatic (highest installed) thường phù hợp yêu cầu mới nhất của Google Play."),
            androidTargetSdkVersion);
        androidArchitectures = (AndroidArchitecture)EditorGUILayout.EnumFlagsField(
            new GUIContent("Target Architectures", "Google Play yêu cầu ARM64 cho ứng dụng phát hành."),
            androidArchitectures);

        EditorGUILayout.Space(4f);
        EditorGUILayout.LabelField("Ký ứng dụng (tuỳ chọn)", EditorStyles.boldLabel);
        using (new EditorGUILayout.HorizontalScope())
        {
            keystorePath = EditorGUILayout.TextField("Keystore", keystorePath);
            if (GUILayout.Button("Chọn...", GUILayout.Width(72f)))
            {
                string selectedPath = EditorUtility.OpenFilePanel("Chọn Android Keystore", GetKeystoreDirectory(), string.Empty);
                if (!string.IsNullOrEmpty(selectedPath))
                {
                    keystorePath = selectedPath;
                    GUI.FocusControl(null);
                }
            }
        }

        keystorePassword = EditorGUILayout.PasswordField("Keystore Password", keystorePassword);
        keyAlias = EditorGUILayout.TextField("Key Alias", keyAlias);
        keyAliasPassword = EditorGUILayout.PasswordField("Key Alias Password", keyAliasPassword);
        EditorGUILayout.HelpBox("Keystore và mật khẩu được lưu riêng cho project này trên máy local, không ghi vào source control.", MessageType.None);
        EndSection();
    }

    private void DrawIosSection()
    {
        BeginSection("iOS / App Store");
        iosApplicationId = EditorGUILayout.TextField(
            new GUIContent("Bundle Identifier", "Bundle ID duy nhất, ví dụ com.company.game."),
            iosApplicationId);
        EditorGUILayout.HelpBox("Giữ nguyên Bundle Identifier nếu đây là bản cập nhật của app đã có trên App Store.", MessageType.None);
        using (new EditorGUILayout.HorizontalScope())
        {
            iosBuildNumber = EditorGUILayout.TextField(
                new GUIContent("Build Number", "Phải tăng sau mỗi lần upload lên App Store Connect."),
                iosBuildNumber);
            if (GUILayout.Button("+1", GUILayout.Width(82f)))
            {
                iosBuildNumber = IncrementLastNumericPart(iosBuildNumber);
                GUI.FocusControl(null);
            }
        }
        iosMinimumVersion = EditorGUILayout.TextField(
            new GUIContent("Minimum iOS Version", "Phiên bản iOS thấp nhất app hỗ trợ, ví dụ 15.0."),
            iosMinimumVersion);
        appleDeveloperTeamId = EditorGUILayout.TextField("Developer Team ID", appleDeveloperTeamId);
        appleAutomaticSigning = EditorGUILayout.Toggle("Automatic Signing", appleAutomaticSigning);
        EndSection();
    }

    private void DrawBuildSection()
    {
        BeginSection("Áp dụng & Build");
        developmentBuild = EditorGUILayout.Toggle("Development Build", developmentBuild);

        EditorGUILayout.Space(4f);
        using (new EditorGUILayout.HorizontalScope())
        {
            if (GUILayout.Button("Áp dụng settings", GUILayout.Height(34f)))
            {
                ApplySettings(true);
            }

            if (GUILayout.Button("Mở Player Settings", GUILayout.Height(34f)))
            {
                SettingsService.OpenProjectSettings("Project/Player");
            }
        }

        EditorGUILayout.Space(4f);
        using (new EditorGUILayout.HorizontalScope())
        {
            GUI.backgroundColor = new Color(0.55f, 0.85f, 0.55f);
            if (GUILayout.Button(buildAppBundle ? "Build Android (.aab)" : "Build Android (.apk)", GUILayout.Height(40f)))
            {
                BuildAndroid();
            }

            GUI.backgroundColor = new Color(0.55f, 0.72f, 0.95f);
            if (GUILayout.Button("Build iOS (Xcode)", GUILayout.Height(40f)))
            {
                BuildIos();
            }

            GUI.backgroundColor = Color.white;
        }
        EndSection();
    }

    private void LoadPlayerSettings()
    {
        companyName = PlayerSettings.companyName;
        productName = PlayerSettings.productName;
        version = PlayerSettings.bundleVersion;

        androidApplicationId = PlayerSettings.GetApplicationIdentifier(NamedBuildTarget.Android);
        androidVersionCode = PlayerSettings.Android.bundleVersionCode;
        androidMinSdkVersion = PlayerSettings.Android.minSdkVersion;
        androidTargetSdkVersion = PlayerSettings.Android.targetSdkVersion;
        androidArchitectures = PlayerSettings.Android.targetArchitectures;
        buildAppBundle = EditorUserBuildSettings.buildAppBundle;
        keystorePath = PlayerSettings.Android.keystoreName;
        keyAlias = PlayerSettings.Android.keyaliasName;
        keystorePassword = string.Empty;
        keyAliasPassword = string.Empty;

        iosApplicationId = PlayerSettings.GetApplicationIdentifier(NamedBuildTarget.iOS);
        iosBuildNumber = PlayerSettings.iOS.buildNumber;
        iosMinimumVersion = PlayerSettings.iOS.targetOSVersionString;
        appleDeveloperTeamId = PlayerSettings.iOS.appleDeveloperTeamID;
        appleAutomaticSigning = PlayerSettings.iOS.appleEnableAutomaticSigning;
    }

    private bool ApplySettings(bool showSuccessDialog)
    {
        if (!ValidateSettings(out string validationError))
        {
            EditorUtility.DisplayDialog("Settings chưa hợp lệ", validationError, "OK");
            return false;
        }

        PlayerSettings.companyName = companyName.Trim();
        PlayerSettings.productName = productName.Trim();
        PlayerSettings.bundleVersion = version.Trim();

        PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.Android, androidApplicationId.Trim());
        PlayerSettings.Android.bundleVersionCode = androidVersionCode;
        PlayerSettings.Android.minSdkVersion = androidMinSdkVersion;
        PlayerSettings.Android.targetSdkVersion = androidTargetSdkVersion;
        PlayerSettings.Android.targetArchitectures = androidArchitectures;
        EditorUserBuildSettings.buildAppBundle = buildAppBundle;
        ApplyAndroidSigning();
        SaveSigningPreferences();

        PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.iOS, iosApplicationId.Trim());
        PlayerSettings.iOS.buildNumber = iosBuildNumber.Trim();
        PlayerSettings.iOS.targetOSVersionString = iosMinimumVersion.Trim();
        PlayerSettings.iOS.appleDeveloperTeamID = appleDeveloperTeamId.Trim();
        PlayerSettings.iOS.appleEnableAutomaticSigning = appleAutomaticSigning;

        AssetDatabase.SaveAssets();

        if (showSuccessDialog)
        {
            EditorUtility.DisplayDialog("Build Settings", "Đã áp dụng toàn bộ thông tin Android và iOS.", "OK");
        }

        return true;
    }

    private void ApplyAndroidSigning()
    {
        bool hasKeystore = !string.IsNullOrWhiteSpace(keystorePath);
        PlayerSettings.Android.useCustomKeystore = hasKeystore;
        if (!hasKeystore)
        {
            return;
        }

        PlayerSettings.Android.keystoreName = keystorePath.Trim();
        PlayerSettings.Android.keyaliasName = keyAlias.Trim();

        if (!string.IsNullOrEmpty(keystorePassword))
        {
            PlayerSettings.Android.keystorePass = keystorePassword;
        }

        if (!string.IsNullOrEmpty(keyAliasPassword))
        {
            PlayerSettings.Android.keyaliasPass = keyAliasPassword;
        }
    }

    private void LoadSigningPreferences()
    {
        keystorePath = EditorPrefs.GetString(SigningPrefsPrefix + "KeystorePath", keystorePath);
        keystorePassword = EditorPrefs.GetString(SigningPrefsPrefix + "KeystorePassword", string.Empty);
        keyAlias = EditorPrefs.GetString(SigningPrefsPrefix + "KeyAlias", keyAlias);
        keyAliasPassword = EditorPrefs.GetString(SigningPrefsPrefix + "KeyAliasPassword", string.Empty);
    }

    private void SaveSigningPreferences()
    {
        EditorPrefs.SetString(SigningPrefsPrefix + "KeystorePath", keystorePath ?? string.Empty);
        EditorPrefs.SetString(SigningPrefsPrefix + "KeystorePassword", keystorePassword ?? string.Empty);
        EditorPrefs.SetString(SigningPrefsPrefix + "KeyAlias", keyAlias ?? string.Empty);
        EditorPrefs.SetString(SigningPrefsPrefix + "KeyAliasPassword", keyAliasPassword ?? string.Empty);
    }

    private bool ValidateSettings(out string error)
    {
        if (string.IsNullOrWhiteSpace(companyName) || string.IsNullOrWhiteSpace(productName))
        {
            error = "Company Name và Product Name không được để trống.";
            return false;
        }

        if (string.IsNullOrWhiteSpace(version))
        {
            error = "Version không được để trống.";
            return false;
        }

        if (!IsValidApplicationId(androidApplicationId) || !IsValidApplicationId(iosApplicationId))
        {
            error = "Application ID / Bundle Identifier phải có ít nhất hai phần, chỉ dùng chữ, số, dấu chấm hoặc dấu gạch dưới; mỗi phần phải bắt đầu bằng chữ.";
            return false;
        }

        if (androidVersionCode < 1)
        {
            error = "Android Version Code phải lớn hơn hoặc bằng 1.";
            return false;
        }

        if (string.IsNullOrWhiteSpace(iosBuildNumber))
        {
            error = "iOS Build Number không được để trống.";
            return false;
        }

        if (string.IsNullOrWhiteSpace(iosMinimumVersion))
        {
            error = "Minimum iOS Version không được để trống.";
            return false;
        }

        if ((androidArchitectures & AndroidArchitecture.ARM64) == 0)
        {
            error = "Target Architectures phải có ARM64 để phát hành trên Google Play.";
            return false;
        }

        if (!string.IsNullOrWhiteSpace(keystorePath) && !File.Exists(keystorePath))
        {
            error = "Không tìm thấy file Android Keystore đã chọn.";
            return false;
        }

        if (!string.IsNullOrWhiteSpace(keystorePath) && string.IsNullOrWhiteSpace(keyAlias))
        {
            error = "Key Alias không được để trống khi dùng custom keystore.";
            return false;
        }

        error = string.Empty;
        return true;
    }

    private static bool IsValidApplicationId(string applicationId)
    {
        if (string.IsNullOrWhiteSpace(applicationId))
        {
            return false;
        }

        string[] parts = applicationId.Trim().Split('.');
        return parts.Length >= 2 && parts.All(IsValidApplicationIdPart);
    }

    private static bool IsValidApplicationIdPart(string part)
    {
        if (string.IsNullOrEmpty(part) || !char.IsLetter(part[0]))
        {
            return false;
        }

        return part.All(character => char.IsLetterOrDigit(character) || character == '_');
    }

    private void BuildAndroid()
    {
        if (!ApplySettings(false) || !TryGetEnabledScenes(out string[] scenes))
        {
            return;
        }

        string extension = buildAppBundle ? "aab" : "apk";
        string defaultName = GetAndroidBuildName(extension);
        string outputPath = EditorUtility.SaveFilePanel("Chọn nơi lưu Android build", GetDefaultBuildDirectory("Android"), defaultName, extension);
        if (string.IsNullOrEmpty(outputPath))
        {
            return;
        }

        BuildPlayer(scenes, outputPath, BuildTarget.Android);
    }

    private void BuildIos()
    {
        if (!ApplySettings(false) || !TryGetEnabledScenes(out string[] scenes))
        {
            return;
        }

        string outputPath = EditorUtility.SaveFolderPanel("Chọn thư mục Xcode project", GetDefaultBuildDirectory("iOS"), GetSafeBuildName(string.Empty));
        if (string.IsNullOrEmpty(outputPath))
        {
            return;
        }

        BuildPlayer(scenes, outputPath, BuildTarget.iOS);
    }

    private void BuildPlayer(string[] scenes, string outputPath, BuildTarget target)
    {
        BuildTargetGroup targetGroup = target == BuildTarget.Android ? BuildTargetGroup.Android : BuildTargetGroup.iOS;
        if (EditorUserBuildSettings.activeBuildTarget != target &&
            !EditorUserBuildSettings.SwitchActiveBuildTarget(targetGroup, target))
        {
            EditorUtility.DisplayDialog("Không thể đổi platform", $"Unity không thể chuyển active build target sang {target}.", "OK");
            return;
        }

        BuildOptions options = developmentBuild ? BuildOptions.Development : BuildOptions.None;
        BuildReport report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
        {
            scenes = scenes,
            locationPathName = outputPath,
            target = target,
            options = options
        });

        BuildSummary summary = report.summary;
        if (summary.result == BuildResult.Succeeded)
        {
            EditorUtility.RevealInFinder(outputPath);
            EditorUtility.DisplayDialog("Build thành công", $"{target} build hoàn tất\nDung lượng: {FormatBytes(summary.totalSize)}\nThời gian: {summary.totalTime}", "OK");
        }
        else
        {
            EditorUtility.DisplayDialog("Build không thành công", $"Kết quả: {summary.result}\nErrors: {summary.totalErrors}", "OK");
        }
    }

    private static bool TryGetEnabledScenes(out string[] scenes)
    {
        scenes = EditorBuildSettings.scenes
            .Where(scene => scene.enabled && !string.IsNullOrWhiteSpace(scene.path))
            .Select(scene => scene.path)
            .ToArray();

        if (scenes.Length > 0)
        {
            return true;
        }

        EditorUtility.DisplayDialog("Chưa có scene", "Hãy thêm ít nhất một scene được bật trong Build Profiles / Build Settings.", "OK");
        return false;
    }

    private string GetSafeBuildName(string extension)
    {
        string name = $"{productName}-{version}";
        foreach (char invalidCharacter in Path.GetInvalidFileNameChars())
        {
            name = name.Replace(invalidCharacter, '-');
        }

        return string.IsNullOrEmpty(extension) ? name : $"{name}.{extension}";
    }

    private string GetAndroidBuildName(string extension)
    {
        string safeProductName = productName.Trim().Replace(' ', '_');
        string name = $"{safeProductName}_{version.Trim()}_{androidVersionCode}_{DateTime.Now:yyyyMMdd_HHmmss}";
        foreach (char invalidCharacter in Path.GetInvalidFileNameChars())
        {
            name = name.Replace(invalidCharacter, '-');
        }

        return $"{name}.{extension}";
    }

    private static string IncrementLastNumericPart(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return "1";
        }

        string[] parts = value.Trim().Split('.');
        int lastIndex = parts.Length - 1;
        if (int.TryParse(parts[lastIndex], out int lastNumber))
        {
            parts[lastIndex] = (lastNumber + 1).ToString();
            return string.Join(".", parts);
        }

        return value;
    }

    private static string IncrementPatchVersion(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return "0.0.1";
        }

        string[] parts = value.Trim().Split('.');
        if (parts.Length > 3 || parts.Any(part => !int.TryParse(part, out _)))
        {
            return value;
        }

        int major = parts.Length > 0 ? int.Parse(parts[0]) : 0;
        int minor = parts.Length > 1 ? int.Parse(parts[1]) : 0;
        int patch = parts.Length > 2 ? int.Parse(parts[2]) : 0;
        return $"{major}.{minor}.{patch + 1}";
    }

    private static string GetDefaultBuildDirectory(string platform)
    {
        string directory = Path.GetFullPath(Path.Combine(Application.dataPath, "..", "Builds", platform));
        Directory.CreateDirectory(directory);
        return directory;
    }

    private string GetKeystoreDirectory()
    {
        if (!string.IsNullOrWhiteSpace(keystorePath) && File.Exists(keystorePath))
        {
            return Path.GetDirectoryName(keystorePath);
        }

        return Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
    }

    private static string FormatBytes(ulong byteCount)
    {
        const double megabyte = 1024d * 1024d;
        return $"{byteCount / megabyte:0.0} MB";
    }

    private static void BeginSection(string title)
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.LabelField(title, EditorStyles.boldLabel);
        EditorGUILayout.Space(2f);
    }

    private static void EndSection()
    {
        EditorGUILayout.Space(3f);
        EditorGUILayout.EndVertical();
        EditorGUILayout.Space(5f);
    }
}
