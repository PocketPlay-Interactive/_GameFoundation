[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [ValidateScript({ Test-Path -LiteralPath $_ -PathType Leaf })]
    [string]$UnityPath,

    [string]$UnityProjectPath
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$global:LASTEXITCODE = 0

$sourceRoot = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$upmRoot = Join-Path $sourceRoot 'upm'
$sourceManifestPath = Join-Path $sourceRoot 'package.json'
$sourceManifest = Get-Content -LiteralPath $sourceManifestPath -Raw | ConvertFrom-Json
$packageName = [string]$sourceManifest.name

function Write-Utf8NoBom {
    param([string]$Path, [string]$Content)

    $utf8 = New-Object System.Text.UTF8Encoding($false)
    [IO.File]::WriteAllText($Path, $Content, $utf8)
}

function Get-StableGuid {
    param([string]$RelativePath)

    $md5 = [Security.Cryptography.MD5]::Create()
    try {
        $bytes = [Text.Encoding]::UTF8.GetBytes("$packageName/$($RelativePath.Replace('\', '/'))")
        return ([BitConverter]::ToString($md5.ComputeHash($bytes))).Replace('-', '').ToLowerInvariant()
    }
    finally {
        $md5.Dispose()
    }
}

function Get-RelativePath {
    param(
        [string]$BasePath,
        [string]$TargetPath
    )

    $base = ([IO.Path]::GetFullPath($BasePath)).TrimEnd('\') + '\'
    $target = [IO.Path]::GetFullPath($TargetPath)
    $baseUri = [Uri]$base
    $targetUri = [Uri]$target
    return [Uri]::UnescapeDataString($baseUri.MakeRelativeUri($targetUri).ToString()).Replace('/', '\')
}

function Get-RelativePackagePath {
    param([string]$Path)

    return (Get-RelativePath $upmRoot $Path).Replace('\', '/')
}

function Write-AssetMeta {
    param([string]$AssetPath)

    $relativePath = Get-RelativePackagePath $AssetPath
    $guid = Get-StableGuid $relativePath
    $name = [IO.Path]::GetFileName($AssetPath)
    $extension = [IO.Path]::GetExtension($AssetPath).ToLowerInvariant()

    if ($name -eq 'package.json') {
        $content = @"
fileFormatVersion: 2
guid: $guid
PackageManifestImporter:
  externalObjects: {}
  userData:
  assetBundleName:
  assetBundleVariant:
"@
    }
    elseif ($name -in @('README.md', 'LICENSE.md') -or $extension -in @('.md', '.txt')) {
        $content = @"
fileFormatVersion: 2
guid: $guid
TextScriptImporter:
  externalObjects: {}
  userData:
  assetBundleName:
  assetBundleVariant:
"@
    }
    else {
        $content = @"
fileFormatVersion: 2
guid: $guid
"@
    }

    Write-Utf8NoBom "$AssetPath.meta" $content
}

function Write-FolderMeta {
    param([string]$FolderPath)

    $relativePath = Get-RelativePackagePath $FolderPath
    $guid = Get-StableGuid $relativePath
    $content = @"
fileFormatVersion: 2
guid: $guid
folderAsset: yes
DefaultImporter:
  externalObjects: {}
  userData:
  assetBundleName:
  assetBundleVariant:
"@
    Write-Utf8NoBom "$FolderPath.meta" $content
}

function Remove-ExistingPath {
    param(
        [string]$Path,
        [string]$AllowedRoot
    )

    if (-not (Test-Path -LiteralPath $Path)) {
        return
    }

    $resolvedPath = [IO.Path]::GetFullPath((Resolve-Path -LiteralPath $Path).Path).TrimEnd('\')
    $resolvedRoot = [IO.Path]::GetFullPath($AllowedRoot).TrimEnd('\')
    if ($resolvedPath -ne $resolvedRoot -and -not $resolvedPath.StartsWith("$resolvedRoot\", [StringComparison]::OrdinalIgnoreCase)) {
        throw "Refusing to remove path outside '$resolvedRoot': '$resolvedPath'."
    }

    Remove-Item -LiteralPath $resolvedPath -Recurse -Force
}

function Copy-SourcePackage {
    param([string]$Destination)

    New-Item -ItemType Directory -Path $Destination -Force | Out-Null
    $excludedNames = @('.git', 'upm', 'Tools', 'Library', 'Temp', 'Logs', 'Obj', 'UserSettings')
    Get-ChildItem -LiteralPath $sourceRoot -Force |
        Where-Object { $_.Name -notin $excludedNames } |
        Copy-Item -Destination $Destination -Recurse -Force
}

function Prepare-TemporaryDependencies {
    param(
        [string]$PackagesPath,
        [string]$StagedPackagePath
    )

    if (-not $ownsTemporaryProject) {
        return
    }

    $gitPath = (Get-Command git.exe -ErrorAction Stop).Source
    $clonePath = Join-Path $temporaryRoot 'UniTask'
    $unitaskPackagePath = Join-Path $PackagesPath 'com.cysharp.unitask'
    $cloneProcess = Start-Process -FilePath $gitPath -ArgumentList @('clone', '--depth', '1', 'https://github.com/Cysharp/UniTask.git', $clonePath) -WindowStyle Hidden -Wait -PassThru
    if ($cloneProcess.ExitCode -ne 0) {
        throw 'Failed to clone UniTask dependency.'
    }

    $unitaskSourcePath = Join-Path $clonePath 'src\UniTask\Assets\Plugins\UniTask'
    if (-not (Test-Path -LiteralPath (Join-Path $unitaskSourcePath 'package.json'))) {
        throw "UniTask package manifest missing: '$unitaskSourcePath'."
    }
    Copy-Item -LiteralPath $unitaskSourcePath -Destination $unitaskPackagePath -Recurse -Force

    $stagedManifestPath = Join-Path $StagedPackagePath 'package.json'
    $stagedManifest = Get-Content -LiteralPath $stagedManifestPath -Raw | ConvertFrom-Json
    if ($null -ne $stagedManifest.dependencies.PSObject.Properties['com.cysharp.unitask']) {
        $stagedManifest.dependencies.PSObject.Properties.Remove('com.cysharp.unitask')
    }
    Write-Utf8NoBom $stagedManifestPath ($stagedManifest | ConvertTo-Json -Depth 20)
}

function Invoke-Unity {
    param([string[]]$Arguments)

    $process = Start-Process -FilePath $unityPath -ArgumentList $Arguments -WindowStyle Hidden -Wait -PassThru
    return $process.ExitCode
}

function Copy-Documentation {
    param([string]$Destination)

    $sourceDocumentation = Join-Path $sourceRoot 'Documentation'
    if (Test-Path -LiteralPath $sourceDocumentation) {
        Get-ChildItem -LiteralPath $sourceDocumentation -Force |
            Copy-Item -Destination $Destination -Recurse -Force
    }

    $sampleDocumentation = Join-Path $Destination 'Samples'
    New-Item -ItemType Directory -Path $sampleDocumentation -Force | Out-Null
    $sourceSamples = Join-Path $sourceRoot 'Samples'
    if (-not (Test-Path -LiteralPath $sourceSamples)) {
        return
    }

    Get-ChildItem -LiteralPath $sourceSamples -Recurse -File |
        Where-Object { $_.Extension -in @('.md', '.cs') } |
        ForEach-Object {
            $relativePath = Get-RelativePath $sourceSamples $_.FullName
            if ($_.Extension -eq '.cs') {
                $relativePath = [IO.Path]::ChangeExtension($relativePath, '.cs.txt')
            }

            $destinationPath = Join-Path $sampleDocumentation $relativePath
            New-Item -ItemType Directory -Path (Split-Path $destinationPath -Parent) -Force | Out-Null
            Copy-Item -LiteralPath $_.FullName -Destination $destinationPath -Force
        }
}

function Copy-CompiledAssemblies {
    param(
        [string]$StagedPackagePath,
        [string]$ProjectPath
    )

    $scriptAssemblies = Join-Path $ProjectPath 'Library\ScriptAssemblies'
    if (-not (Test-Path -LiteralPath $scriptAssemblies)) {
        throw "Unity did not produce Library/ScriptAssemblies: '$scriptAssemblies'."
    }

    $runtimeDestination = Join-Path $upmRoot 'Runtime'
    $editorDestination = Join-Path $upmRoot 'Editor'
    $definitions = Get-ChildItem -LiteralPath $StagedPackagePath -Recurse -File -Filter '*.asmdef'
    foreach ($definition in $definitions) {
        $relativeDefinition = Get-RelativePath $StagedPackagePath $definition.FullName
        if ($relativeDefinition -match '(^|[\\/])(Tests|Demos)([\\/]|$)') {
            continue
        }

        $definitionManifest = Get-Content -LiteralPath $definition.FullName -Raw | ConvertFrom-Json
        $assemblyName = [string]$definitionManifest.name
        $compiledAssembly = Join-Path $scriptAssemblies "$assemblyName.dll"
        if (-not (Test-Path -LiteralPath $compiledAssembly)) {
            continue
        }

        $includePlatforms = @($definitionManifest.includePlatforms)
        $isEditor = $includePlatforms -contains 'Editor' -or
            $assemblyName.EndsWith('.Editor', [StringComparison]::OrdinalIgnoreCase) -or
            $relativeDefinition -match '[\\/]Editor[\\/]'
        $destination = if ($isEditor) { $editorDestination } else { $runtimeDestination }
        Copy-Item -LiteralPath $compiledAssembly -Destination (Join-Path $destination "$assemblyName.dll") -Force
    }

    $requiredAssemblies = @(
        (Join-Path $runtimeDestination 'Dann.GameFoundation.dll'),
        (Join-Path $editorDestination 'Dann.GameFoundation.Editor.dll')
    )
    foreach ($requiredAssembly in $requiredAssemblies) {
        if (-not (Test-Path -LiteralPath $requiredAssembly)) {
            throw "Required compiled assembly missing: '$requiredAssembly'."
        }
    }

    $sourceDlls = Get-ChildItem -LiteralPath $StagedPackagePath -Recurse -File -Filter '*.dll' |
        Where-Object { $_.FullName -notlike "$scriptAssemblies\*" }
    foreach ($sourceDll in $sourceDlls) {
        $relativeDll = Get-RelativePath $StagedPackagePath $sourceDll.FullName
        $isEditor = $relativeDll -match '(^|[\\/])Editor([\\/]|$)' -or
            $sourceDll.BaseName -match '(^|\.)(Editor|EditorPro)$'
        $destination = if ($isEditor) { $editorDestination } else { $runtimeDestination }
        $destinationPath = Join-Path $destination $sourceDll.Name
        if (-not (Test-Path -LiteralPath $destinationPath)) {
            Copy-Item -LiteralPath $sourceDll.FullName -Destination $destinationPath -Force
        }
    }
}

function Write-UpmMetadata {
    Get-ChildItem -LiteralPath $upmRoot -Recurse -Force -Filter '*.meta' |
        Remove-Item -Force

    Get-ChildItem -LiteralPath $upmRoot -Recurse -Directory |
        Sort-Object FullName |
        ForEach-Object { Write-FolderMeta $_.FullName }

    Get-ChildItem -LiteralPath $upmRoot -Recurse -File |
        Where-Object { $_.Extension -ne '.meta' } |
        ForEach-Object { Write-AssetMeta $_.FullName }
}

function Assert-BinaryPackage {
    $forbiddenFiles = Get-ChildItem -LiteralPath $upmRoot -Recurse -File |
        Where-Object { $_.Extension -in @('.cs', '.asmdef') }
    if ($forbiddenFiles) {
        throw "Binary package contains forbidden source files: $($forbiddenFiles.FullName -join ', ')"
    }

    $forbiddenDirectories = Get-ChildItem -LiteralPath $upmRoot -Recurse -Directory |
        Where-Object { $_.Name -eq 'Samples~' }
    if ($forbiddenDirectories) {
        throw "Binary package contains forbidden Samples~ directory."
    }

    $requiredPaths = @(
        (Join-Path $upmRoot 'package.json'),
        (Join-Path $upmRoot 'README.md'),
        (Join-Path $upmRoot 'LICENSE.md'),
        (Join-Path $upmRoot 'Runtime'),
        (Join-Path $upmRoot 'Editor'),
        (Join-Path $upmRoot 'Documentation')
    )
    foreach ($requiredPath in $requiredPaths) {
        if (-not (Test-Path -LiteralPath $requiredPath)) {
            throw "Required UPM path missing: '$requiredPath'."
        }
    }

    $upmManifest = Get-Content -LiteralPath (Join-Path $upmRoot 'package.json') -Raw | ConvertFrom-Json
    if ($null -ne $upmManifest.PSObject.Properties['samples']) {
        throw "upm/package.json must not contain the 'samples' field."
    }
    if (-not (Get-ChildItem -LiteralPath (Join-Path $upmRoot 'Runtime') -File -Filter '*.dll')) {
        throw 'UPM Runtime directory contains no DLL.'
    }
    if (-not (Get-ChildItem -LiteralPath (Join-Path $upmRoot 'Editor') -File -Filter '*.dll')) {
        throw 'UPM Editor directory contains no DLL.'
    }
}

$unityPath = [IO.Path]::GetFullPath($UnityPath)
$ownsTemporaryProject = [string]::IsNullOrWhiteSpace($UnityProjectPath)
$temporaryRoot = $null
$projectPath = $null
$stagedPackagePath = $null
$buildSucceeded = $false

try {
    if ($ownsTemporaryProject) {
        $temporaryRoot = Join-Path ([IO.Path]::GetTempPath()) "GameFoundation-Upm-$([Guid]::NewGuid().ToString('N'))"
        New-Item -ItemType Directory -Path $temporaryRoot -Force | Out-Null
        $projectPath = Join-Path $temporaryRoot 'Project'
        $createLog = Join-Path $temporaryRoot 'create-project.log'
        $createExitCode = Invoke-Unity @('-batchmode', '-quit', '-createProject', $projectPath, '-logFile', $createLog)
        if ($createExitCode -ne 0) {
            throw "Unity failed to create temporary project. Log: '$createLog'."
        }
    }
    else {
        $projectPath = [IO.Path]::GetFullPath($UnityProjectPath)
        if (-not (Test-Path -LiteralPath $projectPath -PathType Container)) {
            throw "Unity project path does not exist: '$projectPath'."
        }
    }

    $projectPath = [IO.Path]::GetFullPath($projectPath)
    if ($projectPath.TrimEnd('\').Equals($sourceRoot.TrimEnd('\'), [StringComparison]::OrdinalIgnoreCase)) {
        throw 'UnityProjectPath cannot be the package source root.'
    }

    $packagesPath = Join-Path $projectPath 'Packages'
    New-Item -ItemType Directory -Path $packagesPath -Force | Out-Null
    $stagedPackagePath = Join-Path $packagesPath $packageName
    Remove-ExistingPath $stagedPackagePath $packagesPath
    Copy-SourcePackage $stagedPackagePath
    Prepare-TemporaryDependencies $packagesPath $stagedPackagePath

    $buildLog = Join-Path $projectPath 'GameFoundation-UpmBuild.log'
    $buildExitCode = 0
    for ($attempt = 1; $attempt -le 2; $attempt++) {
        $buildExitCode = Invoke-Unity @('-batchmode', '-quit', '-projectPath', $projectPath, '-nographics', '-logFile', $buildLog)
        if (Test-Path -LiteralPath (Join-Path $projectPath 'Library\ScriptAssemblies')) {
            break
        }
    }
    if ($buildExitCode -ne 0 -and -not (Test-Path -LiteralPath (Join-Path $projectPath 'Library\ScriptAssemblies'))) {
        throw "Unity batchmode compilation failed with exit code $buildExitCode. Log: '$buildLog'."
    }
    if (Test-Path -LiteralPath $buildLog) {
        $compilerErrors = Select-String -LiteralPath $buildLog -Pattern 'error CS\d+|Script compilation failed|Compilation failed' -AllMatches
        if ($compilerErrors) {
            throw "Unity reported compilation errors. Log: '$buildLog'."
        }
    }

    Remove-ExistingPath $upmRoot $sourceRoot
    New-Item -ItemType Directory -Path $upmRoot, (Join-Path $upmRoot 'Runtime'), (Join-Path $upmRoot 'Editor'), (Join-Path $upmRoot 'Documentation') -Force | Out-Null

    $upmManifest = $sourceManifest.PSObject.Copy()
    if ($null -ne $upmManifest.PSObject.Properties['samples']) {
        $upmManifest.PSObject.Properties.Remove('samples')
    }
    Write-Utf8NoBom (Join-Path $upmRoot 'package.json') ($upmManifest | ConvertTo-Json -Depth 20)
    Copy-Item -LiteralPath (Join-Path $sourceRoot 'LICENSE.md') -Destination (Join-Path $upmRoot 'LICENSE.md') -Force
    Copy-Item -LiteralPath (Join-Path $sourceRoot 'CHANGELOG.md') -Destination (Join-Path $upmRoot 'CHANGELOG.md') -Force
    Copy-Item -LiteralPath (Join-Path $sourceRoot 'Library Notices.md') -Destination (Join-Path $upmRoot 'Library Notices.md') -Force
    $binaryReadme = @(
        '# Game Foundation',
        '',
        'Binary UPM package for Game Foundation.',
        '',
        'Install this folder through Unity Package Manager. Runtime and editor code ship as precompiled DLLs; source remains in the repository root for development.',
        '',
        'Documentation: `Documentation/index.md`',
        'License: `LICENSE.md`'
    ) -join [Environment]::NewLine
    Write-Utf8NoBom (Join-Path $upmRoot 'README.md') $binaryReadme

    Copy-Documentation (Join-Path $upmRoot 'Documentation')
    Copy-CompiledAssemblies $stagedPackagePath $projectPath
    Write-UpmMetadata
    Assert-BinaryPackage
    $buildSucceeded = $true

    Write-Output "UPM package built: '$upmRoot'"
}
finally {
    if ($ownsTemporaryProject -and $buildSucceeded -and $temporaryRoot -and (Test-Path -LiteralPath $temporaryRoot)) {
        Remove-Item -LiteralPath $temporaryRoot -Recurse -Force
    }
}
