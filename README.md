# GameFoundation

Thư viện nền dùng chung cho các Unity project: helper, manager, pattern, save/load, security utilities và wrapper SDK. Mục tiêu là copy sang dự án mới nhanh, bật SDK bằng define khi cần, và vẫn compile được khi SDK chưa import.

## Cấu Trúc

| Folder | Vai trò |
| --- | --- |
| `Core` | Code dùng chung: manager, extensibility, pattern, pooling, save/load, canvas, audio, input, editor tools, extensions. |
| `Integrations` | Wrapper SDK: AdMob, Firebase, AppsFlyer, native ads plugin. |
| `Security` | Utility chống sửa value runtime: checksum, obfuscation. |

```text
Assets/GameFoundation
|-- Core
|   |-- Extensibility
|   |-- _Game
|   |-- Unity.*
|   |-- User.*
|   |-- Editor
|   `-- EditorCools
|-- Integrations
|   |-- Ad
|   |-- Ad.Support
|   |-- Appflyer
|   `-- Firebase
`-- Security
```

## Cài Đặt

1. Copy nguyên folder `Assets/GameFoundation` sang project Unity mới.
2. Không cần copy `.meta` trong `Assets/GameFoundation`; để Unity của từng project tự sinh GUID riêng khi import.
3. Mở Unity và đợi reimport xong.
4. Tạo scene bootstrap theo mục [Tạo Scene Mới](#tạo-scene-mới).
5. Chỉ bật define cho SDK đã import thật sự.

## Tạo Scene Mới

### Checklist Script Mặc Định

Với một scene bootstrap mới, mặc định nên có các script sau:

| Bắt buộc? | Script | Gắn vào GameObject | Lý do |
| --- | --- | --- | --- |
| Bắt buộc | `Manager` | `GameFoundation` hoặc `Manager` | Khởi tạo save, SDK, extension modules, loading state. |
| Bắt buộc | `Concurrency` | `Concurrency` | Chạy callback/coroutine về main thread. Nhiều manager dùng `Concurrency.Instance()`. |
| Nên có | `LoadingCanvas` | `LoadingCanvas` hoặc child UI của `Manager` | UI loading mặc định cho bootstrap. Có thể bỏ trống nếu project tự quản lý loading. |
| Tùy game | `CanvasManager` | `CanvasManager` | Quản lý screen/popup nếu dùng hệ UI có sẵn của foundation. |
| Tùy game | `SoundManager` | `SoundManager` | Quản lý sound effect dùng chung. |
| Tùy game | `MusicManager` | `MusicManager` | Quản lý background music dùng chung. |
| Tùy game | `InputManager` | `InputManager` | Dùng input wrapper trong `Core/Unity.Interact/Input`. |
| Tùy game | `RaycastSystem` | `RaycastSystem` | Dùng raycast/touch interaction helper. |

Setup tối thiểu không ads/Firebase:

```text
Scene
|-- GameFoundation
|   `-- Manager
`-- Concurrency
    `-- Concurrency
```

Setup đầy đủ hay dùng cho mobile game:

```text
Scene
|-- GameFoundation
|   |-- Manager
|   `-- LoadingCanvas
|       |-- Canvas
|       `-- CanvasGroup
|-- Concurrency
|   `-- Concurrency
|-- CanvasManager
|   `-- CanvasManager
|-- SoundManager
|   `-- SoundManager
|-- MusicManager
|   `-- MusicManager
|-- FirebaseManager              # Nếu bật FIREBASE
|   `-- FirebaseManager
|-- AdManager                    # Nếu bật ADMOB
|   |-- AdManager
|   |-- AdmobInterstitalOpen
|   |-- AdMobInterstital
|   |-- AdmobNativeCollap        # Nếu bật USE_ADMOB_CUSTOM_PLUGIN
|   `-- AdMobNativeInterstital   # Nếu bật USE_ADMOB_CUSTOM_PLUGIN
`-- AppflyerEventTracking        # Nếu bật APPFLYER
    `-- AppflyerEventTracking
```

Nếu chỉ muốn dùng core helper và save/load, chỉ cần `Manager` + `Concurrency`. Các script còn lại thêm theo tính năng project thực sự dùng.

### Bắt Buộc

Tạo các GameObject sau trong scene đầu tiên:

| GameObject gợi ý | Script cần gắn | Ghi chú |
| --- | --- | --- |
| `GameFoundation` hoặc `Manager` | `Manager` | Bootstrap chính: đọc save, init SDK, init extension modules, quản lý loading. |
| `Concurrency` | `Concurrency` | Cần cho action/coroutine chạy về Unity main thread. Nhiều flow ads, callback, delayed action phụ thuộc object này. |

Trên object có script `Manager`:

- Field `Loading Canvas`: kéo object có script `LoadingCanvas` vào nếu dùng UI loading mặc định. Có thể để trống nếu scene không cần loading UI.
- Field `Extension Modules`: kéo các script project riêng kế thừa `GameFoundationModuleBehaviour` vào đây nếu cần init thêm data, remote config, scene service, economy, tutorial, v.v.

### Optional Theo Nhu Cầu

| Khi dùng | GameObject gợi ý | Script cần gắn | Define cần bật |
| --- | --- | --- | --- |
| Firebase Analytics | `FirebaseManager` | `FirebaseManager` | `FIREBASE` |
| AdMob | `AdManager` | `AdManager`, `AdmobInterstitalOpen`, `AdMobInterstital` và support component cần thiết | `ADMOB` |
| AdMob test id | `AdManager` | `AdManager` | `ADMOB;ADMOB_TEST` |
| Native ads/JKit | `AdManager` | `AdManager`, `AdmobNativeCollap`, `AdMobNativeInterstital` | `ADMOB;USE_ADMOB_CUSTOM_PLUGIN` |
| Bacon UMP consent flow | Bật qua `Manager` | SDK/script `Bacon.UMP` phải tồn tại trong project | `ADMOB;ADMOB_UMP_BACON` |
| AppsFlyer | `AppflyerEventTracking` | `AppflyerEventTracking` | `APPFLYER` |
| IAP | Không cần scene object | Gọi qua `InappPurchase.I` | `IAPPURCHASE_ENABLE` |
| Audio dùng chung | `SoundManager`, `MusicManager` | `SoundManager`, `MusicManager` | Không cần |
| UI dùng chung | `CanvasManager` | `CanvasManager` | Không cần |

Nếu không bật `ADMOB`, `Manager` sẽ tự finalize bootstrap sau khi init extension modules. Nếu có `AdManager` fallback trong scene thì callback bị gọi lặp cũng được chặn bởi `Manager.FinalizeAdSequence()`.

## Mở Rộng Cho Dự Án Mới

Không sửa trực tiếp `Manager`, `RuntimeStorageData`, `AdManager` cho logic riêng của từng game nếu có thể tránh được. Đặt code riêng của project vào:

```text
Assets/_Project/GameFoundationExtensions
```

Tạo module mới bằng cách kế thừa `GameFoundationModuleBehaviour`:

```csharp
using System.Collections;
using UnityEngine;

public class ProjectBootstrapModule : GameFoundationModuleBehaviour
{
    public override IEnumerator Initialize()
    {
        Debug.Log("Init project-specific data here");
        yield return null;
    }
}
```

Sau đó gắn script này lên một GameObject trong scene và kéo component đó vào `Manager > Extension Modules`.

Nếu muốn đăng ký bằng code thay vì kéo trên Inspector, dùng `GameFoundationModuleRegistry.Register(...)`. Tham số truyền vào phải là một object implement `IGameFoundationModule`, thường là component kế thừa `GameFoundationModuleBehaviour`.

Ví dụ đăng ký chính component hiện tại:

```csharp
using System.Collections;
using UnityEngine;

public class ProjectBootstrapModule : GameFoundationModuleBehaviour
{
    private void Awake()
    {
        GameFoundationModuleRegistry.Register(this);
    }

    private void OnDestroy()
    {
        GameFoundationModuleRegistry.Unregister(this);
    }

    public override IEnumerator Initialize()
    {
        Debug.Log("Init project-specific data here");
        yield return null;
    }
}
```

`Register(this)` nghĩa là đăng ký chính module này vào danh sách extension modules để `Manager` gọi `Initialize()`. Cách này chỉ cần khi module không tiện kéo vào `Manager > Extension Modules`, ví dụ module được tạo bằng code, nằm trong prefab spawn sớm, hoặc muốn tự bật/tắt module theo điều kiện.

Lưu ý: module phải được register trước khi `Manager` chạy bước init extension modules thì mới được gọi trong lượt bootstrap hiện tại. Với scene setup bình thường, cách dễ nhớ và ít lỗi nhất vẫn là kéo component vào `Manager > Extension Modules`.

### Thêm User Data Riêng Mà Không Sửa GameFoundation

Không thêm field riêng của từng game vào `PlayerSerializable`, `RuntimeStorageData` hoặc file nào trong `Assets/GameFoundation/Core`. Nếu cần lưu thêm data người chơi cho một game cụ thể, dùng sẵn `RuntimeStorageData.Player.ExtensionData` rồi bọc lại bằng class riêng trong:

```text
Assets/_Project/GameFoundationExtensions
```

Ví dụ tạo file:

```text
Assets/_Project/GameFoundationExtensions/UserProjectData.cs
```

```csharp
public static class UserProjectData
{
    private const string SkinIdKey = "project.skin_id";
    private const string BestScoreKey = "project.best_score";
    private const string TutorialDoneKey = "project.tutorial_done";

    public static string SkinId
    {
        get => RuntimeStorageData.Player.ExtensionData.Get(SkinIdKey, "default");
        set => RuntimeStorageData.Player.ExtensionData.Set(SkinIdKey, value);
    }

    public static int BestScore
    {
        get => RuntimeStorageData.Player.ExtensionData.GetInt(BestScoreKey, 0);
        set => RuntimeStorageData.Player.ExtensionData.SetInt(BestScoreKey, value);
    }

    public static bool TutorialDone
    {
        get => RuntimeStorageData.Player.ExtensionData.GetBool(TutorialDoneKey, false);
        set => RuntimeStorageData.Player.ExtensionData.SetBool(TutorialDoneKey, value);
    }

    public static void Save()
    {
        RuntimeStorageData.SaveAllData();
    }
}
```

Gameplay chỉ gọi class riêng này, không gọi key string rải rác khắp project:

```csharp
UserProjectData.BestScore = 1200;
UserProjectData.TutorialDone = true;
UserProjectData.Save();
```

Nếu data cần init default hoặc migrate khi mở game, tạo thêm module trong `_Project`:

```csharp
using System.Collections;

public class ProjectUserDataModule : GameFoundationModuleBehaviour
{
    public override IEnumerator Initialize()
    {
        if (string.IsNullOrEmpty(UserProjectData.SkinId))
            UserProjectData.SkinId = "default";

        yield return null;
    }
}
```

Sau đó gắn `ProjectUserDataModule` lên một GameObject trong scene và kéo vào `Manager > Extension Modules`.

Quy tắc cho user data riêng:

- Key nên có prefix riêng của project, ví dụ `project.best_score`, `project.skin_id`, để tránh trùng key với module khác.
- Data đơn giản dùng `ExtensionData.Set`, `SetInt`, `SetFloat`, `SetBool` và các hàm `Get` tương ứng.
- Sau khi đổi data quan trọng, gọi `RuntimeStorageData.SaveAllData()` hoặc bọc qua hàm `UserProjectData.Save()`.
- Nếu data phức tạp, nhiều field hoặc cần migration rõ ràng, vẫn đặt code trong `_Project` và serialize thành string/json rồi lưu qua `ExtensionData.Set`.
- Chỉ sửa model gốc trong `GameFoundation` khi data đó thật sự là tính năng dùng chung cho mọi game.

Quy tắc:

- Thứ nào dùng chung cho mọi project đặt trong `GameFoundation/Core`.
- Thứ nào chỉ của một game đặt trong `Assets/_Project/GameFoundationExtensions`.
- SDK hoặc package ngoài đặt trong `GameFoundation/Integrations` và bọc bằng define.
- Public API mà gameplay gọi nên có fallback/no-op khi define tắt.
- Config của từng game như ad unit id, product id, economy, level, remote flag nên nằm trong ScriptableObject hoặc folder `_Project`, không hard-code vào foundation.
- Không commit `.meta` trong `Assets/GameFoundation` nếu folder này được dùng như source shared giữa nhiều project.

## Scripting Define Symbols

Có thể bật/tắt define bằng editor window:

```text
Unity menu > GameFoundation > Define Symbols
```

Window này đọc define của selected build target hiện tại, cho tick/bỏ tick các SDK define thường dùng, rồi bấm `Apply Defines` để lưu.

Nếu muốn chỉnh tay, vào `Project Settings > Player > Other Settings > Scripting Define Symbols`, thêm symbol theo SDK đang dùng.

| Define | Dùng khi |
| --- | --- |
| `FIREBASE` | Đã import Firebase Unity SDK. |
| `ADMOB` | Đã import Google Mobile Ads SDK. |
| `ADMOB_TEST` | Muốn dùng test ad unit id của Google. |
| `ADMOB_UMP_BACON` | Đã import flow UMP có API `Bacon.UMP`. |
| `USE_ADMOB_CUSTOM_PLUGIN` | Đã import plugin native ads/JKit. |
| `APPFLYER` | Đã import AppsFlyer SDK. |
| `IAPPURCHASE_ENABLE` | Đã import Unity IAP. |

Nếu chưa import SDK, không bật define tương ứng.

## Core

### Singleton

```csharp
public class GameManager : SingletonGlobal<GameManager>
{
    public void Init() { }
}

GameManager.I.Init();
```

```csharp
public class InventoryService : GenericSingleton<InventoryService>
{
}

InventoryService.I.ToString();
```

### Concurrency

`Concurrency` dùng để enqueue action/coroutine về Unity main thread.

```csharp
Concurrency.Instance().Enqueue(() =>
{
    Debug.Log("Run on main thread next frame");
});

Concurrency.Instance().Enqueue(MyCoroutine(), 0.5f);
```

Scene phải có object gắn script `Concurrency` trước khi gọi `Concurrency.Instance()`.

### Runtime Storage Và Save

`RuntimeStorageData` đọc/ghi `SettingSerializable` và `PlayerSerializable`.

```csharp
RuntimeStorageData.ReadData();
RuntimeStorageData.Player.ExtensionData.SetInt("score", 1234);
RuntimeStorageData.SaveAllData();
```

Với dự án mới, nếu cần data riêng, ưu tiên tạo wrapper trong `Assets/_Project/GameFoundationExtensions` dùng `PlayerSerializable.ExtensionData`. Xem mục [Thêm User Data Riêng Mà Không Sửa GameFoundation](#thêm-user-data-riêng-mà-không-sửa-gamefoundation) trước khi sửa model gốc.

## Integrations

### Firebase

```text
FIREBASE
```

```csharp
yield return FirebaseManager.I.InitializeAsync();
FirebaseManager.I.TrackingEvent("level_start");
FirebaseManager.I.TrackingEvent("level_complete", ("level", 10), ("time", 42.5f));
FirebaseManager.I.TrackingAdsEvent("paid_ads_banner", "home_banner", "0.0012");
```

Khi không có `FIREBASE`, các hàm tracking là no-op.

### AdMob

```text
ADMOB
```

API hay dùng:

```csharp
AdManager.I.ShowBanner();
AdManager.I.HideBanner();

AdManager.I.ShowInterstitialAdWithSpaceTime(() =>
{
    // Continue gameplay
}, "level_complete");

AdManager.I.ShowRewardedAd(() =>
{
    // Give reward
}, "double_coin");
```

Lưu ý: kiểm tra lại ad unit id trên `AdManager` cho từng project mới. Không nên giữ production id của project khác.

### AppsFlyer

```text
APPFLYER
```

Tracking ad revenue cần cả AppsFlyer và AdMob:

```text
APPFLYER;ADMOB
```

`AppflyerEventTracking.I.LogAdRevenue(adValue)` forward paid event từ AdMob sang AppsFlyer.

### In-App Purchase

```text
IAPPURCHASE_ENABLE
```

Chỉ bật define này sau khi đã import Unity IAP. Product list nên để trong asset/project config riêng của game.

## Security

```csharp
int key = MemoryObfuscation.GenerateKey();
int encrypted = MemoryObfuscation.EncryptInt(100, key);
int value = MemoryObfuscation.DecryptInt(encrypted, key);

int checksum = MemoryChecksum.GenerateChecksum(value);
bool valid = MemoryChecksum.VerifyChecksum(value, checksum);
```

`MemoryChecksum` dùng device-bound HMAC cho value mới và vẫn chấp nhận legacy checksum khi đọc save cũ.

## Kiểm Tra Sau Khi Import

1. Mở Unity và đợi reimport xong.
2. Kiểm tra Console không có compile error.
3. Kiểm tra scene đầu tiên có object `Manager` và `Concurrency`.
4. Nếu dùng loading mặc định, `Manager.Loading Canvas` phải trỏ tới object có script `LoadingCanvas`.
5. Nếu bật define SDK, đảm bảo SDK đã import trước.
6. Regenerate `.csproj` từ Unity nếu IDE vẫn đọc path cũ.

## Troubleshooting

### Lỗi namespace Firebase/GoogleMobileAds/AppsFlyer/JKit không tồn tại

Tắt define tương ứng hoặc import đúng SDK trước khi bật define.

### Lỗi `Bacon` không tồn tại khi bật AdMob

Chỉ bật `ADMOB_UMP_BACON` khi project đã có SDK/script cung cấp `Bacon.UMP`. Nếu chỉ dùng Google Mobile Ads cơ bản, bật `ADMOB` là đủ.

### Gameplay không tiếp tục sau ads

Kiểm tra callback truyền vào `ShowInterstitialAdWithSpaceTime` / `ShowRewardedAd`, đảm bảo scene có `Concurrency`, và đảm bảo `Manager.FinalizeAdSequence()` được gọi khi flow ads kết thúc.

### Manager initialize bị treo

Kiểm tra scene có `AdManager` khi bật `ADMOB`. Nếu không dùng ads, bỏ define `ADMOB` để `Manager` tự finalize bootstrap.

### Mất reference prefab/script sau khi rename hoặc move

Nếu import `GameFoundation` như source shared không kèm `.meta`, Unity sẽ tự sinh GUID mới cho từng project. Hạn chế để prefab/scene trong foundation reference trực tiếp tới asset ngoài foundation; nếu cần prefab dùng chung ổn định GUID thì nên đóng gói thành Unity package riêng và quản lý `.meta` theo package đó.
