# Binary UPM Package

The repository root keeps the Unity source package. The release package is generated under `upm/` and contains only precompiled DLLs, documentation, license files, and Unity `.meta` files.

Build with a temporary Unity project:

```powershell
powershell.exe -NoProfile -ExecutionPolicy Bypass `
  -File .\Tools\Build-UpmPackage.ps1 `
  -UnityPath "C:\Program Files\Unity\Hub\Editor\<VERSION>\Editor\Unity.exe"
```

Use `-UnityProjectPath` to compile in an existing Unity project. The script stages the source package under `Packages/com.dann.gamefoundation`, copies compiled assemblies from `Library/ScriptAssemblies`, converts sample C# files to `.cs.txt`, writes deterministic `.meta` files, and rejects `.cs`, `.asmdef`, or `Samples~` content in `upm/`.

Install the result through Package Manager with `Add package from disk...`, selecting `upm/package.json`.
