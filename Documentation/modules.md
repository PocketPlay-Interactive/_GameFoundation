# Game Foundation Modules

Tài liệu này mô tả trách nhiệm, dependency và cách sử dụng các module chính. Code dùng chung nằm trong `Runtime`; code riêng của từng game đặt trong `Assets/_Project`.

## Nguyên tắc kiến trúc

~~~text
Project code / Features
        |
        v
     Services
        |
        v
       Core
~~~

- `Core` không phụ thuộc ngược vào Services hoặc logic riêng của game.
- `Services` cung cấp capability cấp ứng dụng: storage, audio, asset, pooling và time.
- `Features` là component hoặc hệ thống Unity tùy chọn cho gameplay/UI.
- `Bootstrap` lắp ghép module ở runtime, không chứa logic gameplay riêng.
- `Integrations` bọc API bên ngoài hoặc native platform.
- `Security` cung cấp integrity check và compatibility cho save data.
- Code chỉ dùng cho một game đặt trong `Assets/_Project/GameFoundationExtensions`.

## Sơ đồ module

| Module | Đường dẫn | Trách nhiệm |
| --- | --- | --- |
| Bootstrap | `Runtime/Bootstrap` | Khởi tạo scene, loading và vòng đời ứng dụng. |
| Core | `Runtime/Core` | Pattern, event, threading, I/O, utility và extensibility. |
| Services | `Runtime/Services` | Storage, audio, asset, pooling và network time. |
| Features | `Runtime/Features` | UI, input, interaction, pathfinding và common behaviours. |
| Integrations | `Runtime/Integrations` | Unity IAP, haptics và device API. |
| Security | `Runtime/Security` | Integrity, obfuscation và legacy save compatibility. |
| Editor | `Editor` | Tool chỉ chạy trong Unity Editor. |
| Libraries | `Libraries` | Source vendor dùng bởi package. |

## Bootstrap

### Manager

`Manager` là composition root. Đặt một instance trong scene đầu tiên.

Trình tự khởi tạo:

1. Đọc `RuntimeStorageData` trong `Awake`.
2. Cập nhật login state và lưu lại khi cần.
3. Hiển thị `LoadingCanvas` trong `Start`.
4. Chạy callback qua `OnSdkInitializationRequested`.
5. Merge module trong scene với module đăng ký trước scene load.
6. Khởi tạo module theo `Order` tăng dần.
7. Gọi `OnFoundationReady` sau khi bootstrap hoàn tất.
8. Lưu data khi application pause hoặc quit.

Setup tối thiểu:

~~~text
Scene
+-- GameFoundation
|   +-- Manager
+-- Concurrency
    +-- Concurrency
~~~

`LoadingCanvas` là UI loading tùy chọn. `Manager.HideLoading(delay)` dùng để ẩn loading sau khi game sẵn sàng.

### Concurrency

`Concurrency` chuyển action hoặc coroutine về Unity main thread. Scene phải có component này trước khi gọi `Concurrency.Instance()`.

~~~csharp
Concurrency.Instance().Enqueue(() =>
{
    Debug.Log("Executed on the Unity main thread.");
});
~~~

## Core

### Diagnostics và Extensibility

`LogSystem` gom log thành công, warning và error theo format chung.

Các API extensibility:

- `IGameFoundationModule`: contract gồm `Order` và `Initialize()`.
- `GameFoundationModuleBehaviour`: base class cho module cần object trong scene.
- `GameFoundationModuleRegistry`: registry cho module đăng ký bằng code.
- `ExtensionModuleNoticeAttribute`: hướng dẫn trong Inspector.

~~~csharp
using System.Collections;

public sealed class ProjectModule : GameFoundationModuleBehaviour
{
    public override IEnumerator Initialize()
    {
        yield return null;
    }
}
~~~

`Order` nhỏ hơn chạy trước. Registry bỏ qua cùng một instance được đăng ký nhiều lần.

### Patterns, I/O và Utilities

| API | Dùng cho |
| --- | --- |
| `Singleton<T>` | Component Unity có một instance trong scene. |
| `SingletonGlobal<T>` | Component Unity sống qua scene bằng `DontDestroyOnLoad`. |
| `GenericSingleton<T>` | Plain C# service có constructor mặc định. |
| `ServiceLocator` | Truy cập service theo type. |
| `EventBus` | Publish/subscribe event không cần reference trực tiếp. |
| `ObjectPool<T>` | Tái sử dụng component Unity. |
| `FileHandler` | Đọc/ghi file dùng chung. |

`Runtime/Core/Utilities` chứa extension cho LINQ, vector, text tween, JSON và timer.

## Services

### Persistence

`RuntimeStorageData` quản lý `SETTING`, `PLAYER` và `PLAYER.ExtensionData`.

~~~csharp
RuntimeStorageData.ReadData();

if (RuntimeStorageData.IsReady)
{
    RuntimeStorageData.PLAYER.ExtensionData.Set("my_game.score", 100);
    RuntimeStorageData.SaveAllData();
}
~~~

Data riêng nên có prefix của game. Không sửa `PlayerSerializable` chỉ để thêm data một game; tạo wrapper trong `Assets/_Project`.

### Audio

`SoundService` và `MusicService` là `IGameFoundationModule`. Kéo prefab tương ứng vào `Manager > Extension Modules`.

~~~csharp
SoundService.Get().PlayOnShot("button");
MusicService.Get().PlayMusic("menu");
MusicService.Get().StopMusic();
~~~

Sound key nằm trong `SoundService.SoundTable`; music key nằm trong `MusicService.MusicTable`. Setting mặc định là `SOUND` và `MUSIC`.

### Assets, Pooling và Time

- `ResourceHandle` và `ImageHandler` xử lý load/cache asset.
- `Pooling` quản lý pool prefab dùng chung; object phải được release sau khi dùng.
- `NetworkTime` lấy network time và giữ cache khi request thất bại.

## Features

### UI

- `CanvasManager` chuyển screen/popup theo ID qua `CanvasEvent` và `PopupEvent`.
- `ScreenCanvas` là base class cho screen Show/Hide.
- `PopupCanvas` là base class cho popup lifecycle.
- `ButtonSafe`, `SmartButton` và `BlockedSpam` hạn chế thao tác UI lặp.

~~~csharp
EventBus.Publish(new CanvasEvent { CanvasId = "home" });
EventBus.Publish(new PopupEvent { PopupId = "settings" });
~~~

### Interaction, Pathfinding và CombatText

`Runtime/Features/Interaction` gồm input, touch, raycast và shake. `AStarPathfinder`, `GraphManager`, `PointNode` và `PathDrawer` cung cấp A* graph. `FloatingDamageInstance` và `FloatingDamageSprites` hiển thị combat text.

## Integrations

### Unity IAP

Integration nằm trong `Runtime/Integrations/Purchasing/UnityIAP` và bị giới hạn bởi `IAPPURCHASE_ENABLE`.

1. Cài Unity IAP `5.x`.
2. Bật `IAPPURCHASE_ENABLE`.
3. Tạo asset `InappProductList`.
4. Gán product list cho `InappPurchase`.

### Haptics

`HapticFeedback` dùng NiceVibrations và đọc setting `vibrate`:

~~~csharp
HapticFeedback.Get().Selection();
HapticFeedback.Get().Success();
~~~

### Device Torch

`Torch` là facade cho Android/iOS torch:

~~~csharp
Torch.Initialize(this);
Torch.TurnOn();
Torch.TurnOff();
Torch.Blink(0.05f, 0.05f, 2);
~~~

## Security

- `MemoryChecksum` phát hiện value bị thay đổi; đây là integrity check.
- `MemoryObfuscation` che giấu value trong memory; không thay thế encryption hiện đại.
- `Runtime/Security/Legacy` giữ thuật toán đọc save format cũ, không dùng làm primitive mới.

~~~csharp
int obfuscated = MemoryObfuscation.ObfuscateInt(100);
int value = MemoryObfuscation.DeobfuscateInt(obfuscated);
~~~

## Editor và Libraries

`Editor` chỉ compile trong Unity Editor: build menu, define window, data tools, importer và inspector helper. Không gọi class Editor từ runtime assembly.

`Libraries` chứa DOTween, EditorCools và NiceVibrations. Không sửa source vendor; xem `Library Notices.md` trước khi phân phối package.

## Đặt code mới ở đâu?

| Nhu cầu | Vị trí |
| --- | --- |
| Helper, pattern, contract dùng chung | `Runtime/Core` |
| Capability dùng cho nhiều game | `Runtime/Services` |
| Component UI/gameplay tùy chọn | `Runtime/Features` |
| SDK/native platform wrapper | `Runtime/Integrations` |
| Integrity hoặc save compatibility | `Runtime/Security` |
| Logic chỉ một game | `Assets/_Project/GameFoundationExtensions` |
| Editor tool chỉ một game | `Assets/_Project/Editor` |

## Checklist thêm module

- Xác định module có thật sự dùng chung không.
- Chọn layer thấp nhất phù hợp.
- Không tạo dependency ngược từ Core lên Services.
- Thêm fallback khi integration tùy chọn bị tắt.
- Tạo sample hoặc test cho public API mới.
- Cập nhật tài liệu khi thay đổi lifecycle.
- Giữ file `.meta` và GUID khi di chuyển asset.
