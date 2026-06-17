# GameFoundation

Thư viện nền dùng chung cho các Unity project: helper, manager, pattern, extension, security utilities và wrapper SDK. Mục tiêu là có thể copy sang dự án mới nhanh, bật SDK bằng define khi cần, và vẫn compile được khi SDK chưa import.

## Mục Lục

- [Cấu trúc](#cấu-trúc)
- [Cài đặt](#cài-đặt)
- [Scripting Define Symbols](#scripting-define-symbols)
- [Core](#core)
- [Integrations](#integrations)
- [Security](#security)
- [Quy ước mở rộng](#quy-ước-mở-rộng)
- [Kiểm tra sau khi import](#kiểm-tra-sau-khi-import)
- [Troubleshooting](#troubleshooting)

## Cấu Trúc

| Folder | Vai trò |
| --- | --- |
| `Core` | Code dùng chung: manager, pattern, pooling, save/load, canvas, audio, input, editor tools, extensions. |
| `Integrations` | Các wrapper SDK: AdMob, Firebase, AppsFlyer, native ads plugin. |
| `Security` | Utility chống sửa value runtime: checksum, obfuscation. |

```text
Assets/GameFoundation
|-- Core
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
2. Copy kèm `Assets/GameFoundation.meta` để giữ GUID folder.
3. Mở Unity và đợi reimport xong.
4. Nếu dùng flow mặc định, đảm bảo scene có các object/prefab cần thiết như `Manager`, `Concurrency`, `FirebaseManager`, `AdManager`.
5. Chỉ bật define cho SDK đã import thật sự.

## Scripting Define Symbols

Vào `Project Settings > Player > Other Settings > Scripting Define Symbols`, thêm symbol theo SDK đang dùng.

| Define | Dùng khi |
| --- | --- |
| `FIREBASE` | Đã import Firebase Unity SDK. |
| `ADMOB` | Đã import Google Mobile Ads SDK. |
| `ADMOB_TEST` | Muốn dùng test ad unit id của Google. |
| `USE_ADMOB_CUSTOM_PLUGIN` | Đã import plugin native ads/JKit. |
| `APPFLYER` | Đã import AppsFlyer SDK. |
| `IAPPURCHASE_ENABLE` | Đã import Unity IAP. |

Nếu chưa import SDK, không bật define tương ứng. Các wrapper đã có fallback/no-op để project vẫn compile.

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
public class UIManager : Singleton<UIManager>
{
}

UIManager.I.Show();
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

Scene phải có `Concurrency` object trước khi gọi `Concurrency.Instance()`.

### LogSystem

```csharp
LogSystem.LogSuccess("Loaded");
LogSystem.LogWarning("Missing optional config");
LogSystem.LogError("Failed");
LogSystem.LogArray(1, 2, 3);
```

### Runtime Storage Và Save

`PlayerPrefsUtils` và `RuntimeStorageData` dùng cho save/load nhanh. Khi copy sang dự án mới, kiểm tra lại model data để tránh lệch field.

## Integrations

### Firebase

Yêu cầu:

```text
FIREBASE
```

Ví dụ:

```csharp
yield return FirebaseManager.I.InitializeAsync();

FirebaseManager.I.TrackingEvent("level_start");
FirebaseManager.I.TrackingEvent("level_complete", ("level", 10), ("time", 42.5f));
FirebaseManager.I.TrackingAdsEvent("paid_ads_banner", "home_banner", "0.0012");
```

Khi không có `FIREBASE`, các hàm tracking là no-op.

### AdMob

Yêu cầu cơ bản:

```text
ADMOB
```

Dùng test id:

```text
ADMOB;ADMOB_TEST
```

Dùng native ads/JKit:

```text
ADMOB;USE_ADMOB_CUSTOM_PLUGIN
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

Khi không bật `ADMOB`, fallback sẽ gọi callback trực tiếp để test gameplay không cần SDK ads.

### AppsFlyer

Yêu cầu:

```text
APPFLYER
```

Tracking ad revenue cần cả AppsFlyer và AdMob:

```text
APPFLYER;ADMOB
```

`AppflyerEventTracking.I.LogAdRevenue(adValue)` sẽ forward paid event từ AdMob sang AppsFlyer.

### In-App Purchase

Yêu cầu:

```text
IAPPURCHASE_ENABLE
```

Chỉ bật define này sau khi đã import Unity IAP.

## Security

```csharp
int key = MemoryObfuscation.GenerateKey();
int encrypted = MemoryObfuscation.EncryptInt(100, key);
int value = MemoryObfuscation.DecryptInt(encrypted, key);

int checksum = MemoryChecksum.GenerateChecksum(value);
bool valid = MemoryChecksum.VerifyChecksum(value, checksum);
```

`MemoryChecksum` uses device-bound HMAC for new values and still accepts the legacy checksum format when reading old saves.

## Quy Ước Mở Rộng

- Helper dùng chung đặt trong `Core`.
- SDK hoặc package ngoài đặt trong `Integrations`.
- Anti-cheat, validation, runtime protection đặt trong `Security`.
- SDK không phải built-in Unity phải được bọc bằng `#if DEFINE`.
- API public được gameplay gọi nên có fallback/no-op khi define tắt.
- Khi di chuyển asset, move kèm `.meta` để giữ GUID.

## Kiểm Tra Sau Khi Import

1. Mở Unity và đợi reimport xong.
2. Kiểm tra Console không có compile error.
3. Nếu dùng IDE, regenerate `.csproj` từ Unity khi path bị cũ.
4. Nếu bật define SDK, đảm bảo SDK đã import trước.
5. Test scene đầu tiên có `Manager` và `Concurrency`.

## Troubleshooting

### Lỗi namespace Firebase/GoogleMobileAds/AppsFlyer/JKit không tồn tại

Tắt define tương ứng hoặc import đúng SDK trước khi bật define.

### Gameplay không tiếp tục sau ads

Kiểm tra callback truyền vào `ShowInterstitialAdWithSpaceTime` / `ShowRewardedAd`, và đảm bảo scene có `Concurrency`.

### Manager initialize bị treo

Kiểm tra `ADMOB` chỉ bật khi AdMob/UMP flow đã sẵn sàng. Nếu không dùng ads, bỏ define để fallback tự finalize.

### Mất reference prefab/script sau khi rename hoặc move

Đảm bảo move kèm file `.meta`. Folder này đã được sắp xếp để giữ lại `.meta` của các folder cũ.
