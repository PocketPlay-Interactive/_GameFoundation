# Ví dụ Extension Modules

`Manager` khởi tạo module trong `InitializeExtensionModules()`.

Nguồn module:

- Scene module: `MonoBehaviour[] extensionModules` trên `Manager`.
- Runtime module: `GameFoundationModuleRegistry.Register(...)`.

Tất cả module cần implement:

```csharp
public interface IGameFoundationModule
{
    int Order { get; }
    IEnumerator Initialize();
}
```

Module có `Order` nhỏ chạy trước.

## Cách 1: Module có MonoBehaviour

Dùng khi module cần reference trong Inspector, GameObject, prefab, `SerializeField`, hoặc lifecycle của Unity.

File mẫu: `SampleMonoExtensionModule.cs`

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

Cách gắn:

1. Tạo GameObject trong scene.
2. Add component `SampleMonoExtensionModule`.
3. Chọn GameObject có `Manager`.
4. Kéo component vào `Manager -> Extension Modules`.

`GameFoundationModuleBehaviour` đã có sẵn field `order`.

## Cách 2: Module không cần MonoBehaviour

Dùng khi module chỉ cần code runtime, không cần object trong scene.

File mẫu: `SampleRuntimeExtensionModule.cs`

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

Không cần kéo vào Inspector. Unity gọi `Register()` trước khi scene load, sau đó `Manager` lấy module từ registry.

## Ghi nhớ

- `Initialize()` được gọi sau SDK coordinator, trước `FinalizeAdSequence()`.
- Module coroutine được chạy lần lượt theo `Order`.
- Nếu module cần đợi async, dùng `yield return` trong `Initialize()`.
