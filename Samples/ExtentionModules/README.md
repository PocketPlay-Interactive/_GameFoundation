# Extention Modules Sample

`Manager` khoi tao module trong `InitializeExtensionModules()`.

Nguon module:

- Scene module: `MonoBehaviour[] extensionModules` tren `Manager`.
- Runtime module: `GameFoundationModuleRegistry.Register(...)`.

Tat ca module can implement:

```csharp
public interface IGameFoundationModule
{
    int Order { get; }
    IEnumerator Initialize();
}
```

Module co `Order` nho chay truoc.

## Cach 1: Module co MonoBehaviour

Dung khi module can reference trong Inspector, GameObject, prefab, SerializeField, hoac lifecycle cua Unity.

File mau: `SampleMonoExtensionModule.cs`

```csharp
using System.Collections;
using UnityEngine;

[ExtensionModuleNotice("Drag this component into Manager -> Extension Modules.")]
public sealed class SampleMonoExtensionModule : GameFoundationModuleBehaviour
{
    [SerializeField] private string message = "Mono extension module initialized.";

    public override IEnumerator Initialize()
    {
        LogSystem.LogSuccess(message);
        yield break;
    }
}
```

Cach gan:

1. Tao GameObject trong scene.
2. Add component `SampleMonoExtensionModule`.
3. Chon GameObject co `Manager`.
4. Keo component vao `Manager -> Extension Modules`.

`GameFoundationModuleBehaviour` da co san field `order`.

## Cach 2: Module khong can MonoBehaviour

Dung khi module chi can code runtime, khong can object trong scene.

File mau: `SampleRuntimeExtensionModule.cs`

```csharp
using System.Collections;
using UnityEngine;

public sealed class SampleRuntimeExtensionModule : IGameFoundationModule
{
    public int Order => 10;

    public IEnumerator Initialize()
    {
        LogSystem.LogSuccess("Runtime extension module initialized.");
        yield break;
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Register()
    {
        GameFoundationModuleRegistry.Register(new SampleRuntimeExtensionModule());
    }
}
```

Khong can keo vao Inspector. Unity goi `Register()` truoc khi scene load, sau do `Manager` lay module tu registry.

## Ghi nho

- `Initialize()` duoc goi sau SDK coordinator, truoc `FinalizeAdSequence()`.
- Module coroutine duoc chay lan luot theo `Order`.
- Neu module can doi async, `yield return` trong `Initialize()`.
