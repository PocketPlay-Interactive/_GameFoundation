# Quick Start

## Import

Thêm package qua Unity Package Manager hoặc `Packages/manifest.json`:

```json
"com.dann.gamefoundation": "https://github.com/PocketPlay-Interactive/_GameFoundation.git"
```

## Scene tối thiểu

`ManagerCoordinator` giữ loading tối thiểu `15` giây theo mặc định. Có thể đổi bằng `Manager.Get().SetBootstrapCompletionDelay(14f)`. Để chờ flow bên ngoài, bật `Wait For External Completion`, override `IsCompletionReady()` và gọi `CompleteLoading()` khi flow hoàn tất. `Auto Complete On Timeout` vẫn cứu flow nếu quá thời gian.

1. Kéo `Runtime/Bootstrap/Prefabs/Manager.prefab` vào scene đầu tiên.
2. Gán `LoadingCanvas` nếu cần loading UI mặc định.
3. Thêm các extension module cần dùng vào `Manager`.
4. Chạy scene, kiểm tra Console.

## API thường dùng

```csharp
RuntimeStorageData.SaveAllData();
SoundService.Get().PlayOnShot("button");
MusicService.Get().PlayMusic("menu");
```

## Cấu trúc package

- `Runtime`: runtime code, services, features và integrations.
- `Editor`: Unity Editor tools.
- `Libraries`: thư viện bundled.
- `Samples`: scene và code mẫu.
- `Tests`: runtime và editor tests.
