# GameFoundation

Thư viện nền dùng chung cho các Unity project: helper, manager, pattern, save/load, security utilities và wrapper SDK. Mục tiêu là copy sang dự án mới nhanh, bật SDK bằng define khi cần, và vẫn compile được khi SDK chưa import.

## Cấu Trúc

| Folder | Vai trò |
| --- | --- |
| `Core` | Code dùng chung: manager, extensibility, pattern, pooling, save/load, canvas, audio, input, editor tools, extensions. |
| `Services` | Capability dùng chung cấp ứng dụng, có thể quản lý state, cache, I/O hoặc một workflow hoàn chỉnh. |
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
|-- Services
|   `-- Time
|-- Integrations
|   |-- Ad
|   |-- Ad.Support
|   |-- Appflyer
|   `-- Firebase
`-- Security
```

### Ranh Giới Giữa Core, Services Và Integrations

Không phân loại theo tên class có chữ `Manager` hay không. Phân loại theo trách nhiệm và chiều dependency:

```text
Project / Features
        |
        v
     Services
        |
        v
       Core
```

- `Core` là lớp nền thấp nhất: contract, data structure, pattern, helper thuần, serialization, concurrency và Unity adapter cơ bản. Core không được phụ thuộc ngược vào Services hoặc logic riêng của một game.
- `Services` cung cấp một capability hoàn chỉnh cho gameplay sử dụng, thường có state/cache, truy cập file/network/resource hoặc điều phối nhiều utility. Service có thể là static class, plain C# object hoặc `MonoBehaviour` nếu thực sự cần Unity lifecycle.
- `Integrations` chứa code chạm trực tiếp SDK/package/native API bên ngoài như AdMob, Firebase, Unity IAP hoặc camera torch của từng platform.
- Logic chỉ phục vụ một game, ID sản phẩm, enum âm thanh, event gameplay và config economy phải nằm trong `Assets/_Project`, không nằm trong Core hay Services dùng chung.

Quy tắc dependency mong muốn:

- `Services` được phép phụ thuộc `Core`; `Core` không được gọi ngược lên `Services`.
- Integration nên được bọc sau interface/facade có fallback khi define tắt.
- Bootstrap (`Manager`) là composition root, được phép nối Core, Services và Integrations với nhau; không lấy dependency của `Manager` làm lý do đưa mọi thứ vào Core.

### Kết Quả Rà Soát Folder Core

Các nhóm nên chuyển khỏi `Core` trong một đợt refactor riêng:

| Code hiện tại | Đích đề xuất | Lý do |
| --- | --- | --- |
| `_Game/Storage/RuntimeStorage.cs`, `User.JsonFormater/*`, `_Game/SaveUtils/PlayerPrefsUtils.cs` | `Services/Storage` | Đây là persistence capability hoàn chỉnh, quản lý model, file save, mã hóa và runtime state. |
| `_Game/Audio/SoundManager.cs`, `MusicManager.cs` | `Services/Audio` | Quản lý state và playback toàn ứng dụng. Cần bỏ enum `WIN`, `LOSE` riêng của game khỏi service trước khi coi là dùng chung. |
| `_Game/File/ResourceHandle.cs`, `ImageHandler.cs` | `Services/Assets` | Có cache, network/file I/O và quản lý vòng đời resource. |
| `_Game/Pooling/Pooling.cs` | `Services/Pooling` | Đây là facade global điều phối prefab, resource và pool. Generic `ObjectPool<T>` vẫn ở Core. |

Các nhóm nên giữ ở `Core`:

- `Extensibility`: contract và registry module.
- `Pattern`: singleton base, event bus, service locator và `ObjectPool<T>` tổng quát.
- Helper/extension thuần như JSON wrapper, timer conversion, collection/vector helper.
- `Unity.Concurrency`: hạ tầng chạy callback/coroutine trên main thread.
- Low-level file/hash/security primitive không sở hữu workflow save của người chơi.
- Editor attribute/tool thực sự dùng chung. Tool chỉ phục vụ một feature nên đi cùng feature đó.

Các nhóm không nên đưa sang `Services` chỉ để làm Core nhỏ hơn:

| Nhóm | Đích phù hợp hơn |
| --- | --- |
| `_Game/Purchase` | `Integrations/IAP`, vì code chạm trực tiếp Unity IAP. |
| `Unity.Interact/Torchs` | `Integrations/Device/Torch`, vì đây là native platform bridge. |
| `Pathfinding`, `FloatingDamageSprites`, `Capture`, `User.Object` | `Features` hoặc module riêng; đây là feature/component, không phải application service. |
| `_Game/Canvas` | `Features/UI`; `CanvasManager` điều phối UI nhưng phụ thuộc trực tiếp screen/popup component. |
| `Unity.Interact`, `Unity.Raycast` | Giữ như Unity subsystem trong Core nếu mọi game đều dùng; nếu optional thì tách thành feature/package. |
| `Manager` | `Bootstrap`; đây là composition root, không phải service nghiệp vụ. |

Ngoài việc chuyển folder, có một số code cần làm sạch trước:

- `Core/Config.cs` đang chứa product ID và shake key cụ thể; chuyển các giá trị riêng này sang `_Project` hoặc ScriptableObject config.
- `Core/GameEvent.cs` đang trộn event nền với event IAP/gameplay; thay bằng event type riêng hoặc chuyển phần project-specific ra `_Project`.
- `SoundManager.Sound` và `MusicManager.Music` không nên chứa danh sách enum riêng của từng game trong thư viện dùng chung; dùng ID/config do project cung cấp.
- `Core/EnumConfig.cs` đang rỗng; xóa hoặc chỉ giữ khi có trách nhiệm dùng chung rõ ràng.

Đây là cấu trúc đích đề xuất, chưa phải cấu trúc hiện tại:

```text
Core
|-- Extensibility
|-- Patterns
|-- Serialization
|-- Collections
|-- Concurrency
`-- Unity
Services
|-- Time
|-- Storage
|-- Audio
|-- Assets
`-- Pooling
Integrations
|-- Ads
|-- Analytics
|-- IAP
`-- Device
Features
|-- UI
|-- Pathfinding
|-- FloatingDamage
`-- UserComponents
```

Nên refactor từng nhóm và giữ public API tương thích trước, thay vì move toàn bộ cùng lúc. Nếu sau này dùng assembly definition, chiều dependency trên có thể được enforce bằng `.asmdef` thay vì chỉ dựa vào convention.

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

#### Lưu Một Object Bằng JSON

`ExtensionData` chỉ lưu trực tiếp `string`, `int`, `float` và `bool`. Muốn lưu một object, serialize object thành JSON string, lưu string đó bằng `ExtensionData.Set`, rồi deserialize khi đọc.

Ví dụ model riêng của project:

```csharp
using System;
using System.Collections.Generic;

[Serializable]
public class InventorySaveData
{
    public int version = 1;
    public int coins;
    public List<string> ownedSkinIds = new List<string>();
}
```

Bọc toàn bộ key và logic JSON trong `UserProjectData`, không serialize rải rác ở gameplay:

```csharp
using UnityEngine;

public static class UserProjectData
{
    private const string InventoryKey = "project.inventory";

    public static InventorySaveData LoadInventory()
    {
        string json = RuntimeStorageData.Player.ExtensionData.Get(InventoryKey);

        if (string.IsNullOrEmpty(json))
            return new InventorySaveData();

        try
        {
            InventorySaveData data =
                JsonUtility.FromJson<InventorySaveData>(json);

            return data ?? new InventorySaveData();
        }
        catch (System.Exception exception)
        {
            Debug.LogWarning(
                $"Could not read inventory data: {exception.Message}");
            return new InventorySaveData();
        }
    }

    public static void SaveInventory(InventorySaveData data)
    {
        if (data == null)
            return;

        string json = JsonUtility.ToJson(data);
        RuntimeStorageData.Player.ExtensionData.Set(InventoryKey, json);
        RuntimeStorageData.SaveAllData();
    }
}
```

Gameplay sử dụng object bình thường và chỉ gọi wrapper để load/save:

```csharp
InventorySaveData inventory = UserProjectData.LoadInventory();
inventory.coins += 100;
inventory.ownedSkinIds.Add("skin_red");
UserProjectData.SaveInventory(inventory);
```

Luồng dữ liệu thực tế:

```text
InventorySaveData
    -> JsonUtility.ToJson
    -> ExtensionData.Set("project.inventory", json)
    -> RuntimeStorageData.SaveAllData
    -> file player save đã được bảo vệ bởi GameFoundation
```

JSON này là một string nằm bên trong player save, không phải file `.json` riêng trong `Application.persistentDataPath`.
Không cần tự mã hóa hoặc Base64 JSON trước khi gọi `Set`; `ExtensionData` và runtime storage sẽ xử lý lớp bảo vệ save hiện có.

Giới hạn của `JsonUtility`:

- Class cần có `[Serializable]`.
- Dữ liệu cần lưu phải là field; property không được serialize.
- `List<T>` và array được hỗ trợ. `Dictionary<TKey, TValue>` không được hỗ trợ trực tiếp; chuyển dictionary thành một list entry có `key` và `value`.
- Hạn chế dùng interface hoặc kiểu đa hình trong save model.
- Nên có field `version` để migrate khi cấu trúc object thay đổi.

Chỉ gọi wrapper sau khi `Manager.Awake()` đã chạy `RuntimeStorageData.ReadData()`. Với flow mặc định, có thể load default và migrate object trong một `GameFoundationModuleBehaviour`, vì extension modules được khởi tạo sau khi player save đã được đọc.

Với object nhỏ và luôn thay đổi cùng nhau, lưu một JSON blob là đủ. Với data lớn hoặc các phần có vòng đời khác nhau, chia thành nhiều key như `project.inventory`, `project.progress`, `project.settings` để tránh phải serialize lại toàn bộ data mỗi lần.

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

## Services

`Services` chứa các capability dùng chung để gameplay gọi ở mức cao. Service chỉ dành cho logic có thể tái sử dụng giữa nhiều project; logic riêng của một game vẫn đặt trong `Assets/_Project`.

Hiện tại folder này mới có `Time/NetworkTime.cs`. Các folder Storage, Audio, Assets và Pooling trong cấu trúc đề xuất phía trên là hướng refactor, chưa được di chuyển trong code.

### Time/NetworkTime

`NetworkTime` lấy thời gian UTC từ network, cache lần lấy thành công gần nhất trong `RuntimeStorageData.Player.ExtensionData`, rồi trả kết quả đã đổi sang múi giờ local của thiết bị.

Yêu cầu:

- Project đã import `Cysharp.Threading.Tasks` (UniTask).
- `RuntimeStorageData.ReadData()` đã chạy. Trong scene bootstrap mặc định, `Manager.Awake()` thực hiện bước này.
- Thiết bị cần network để lấy thời gian mới. Khi request lỗi, service dùng thời gian cache; nếu chưa có cache hợp lệ thì dùng `DateTime.UtcNow` của thiết bị.

Cách dùng:

```csharp
using System;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class DailyRewardController : MonoBehaviour
{
    private void Start()
    {
        LoadNetworkTimeAsync().Forget();
    }

    private async UniTask LoadNetworkTimeAsync()
    {
        DateTime localTime = await NetworkTime.GetTimeAsync();
        Debug.Log($"Current local time: {localTime:o}");
    }
}
```

`GetTimeAsync()` bắt lỗi network, log warning và trả về thời gian fallback thay vì đẩy lỗi network ra caller.

#### Cache Và Save

Cache dùng key `NetworkTime_UTC` trong `Player.ExtensionData` và lưu UTC theo định dạng round-trip (`"o"`). Khi lấy giờ mạng thành công, `NetworkTime` chỉ cập nhật cache trong runtime, không tự gọi `RuntimeStorageData.SaveAllData()`.

`Manager` mặc định save khi application pause hoặc quit. Nếu project cần ghi cache xuống disk ngay lập tức:

```csharp
await NetworkTime.GetTimeAsync();
RuntimeStorageData.SaveAllData();
```

#### Giá Trị Trả Về

Method trả `DateTime` đã cộng offset local hiện tại của thiết bị và có `DateTimeKind.Unspecified`. Nếu gameplay cần so sánh deadline tuyệt đối giữa các múi giờ, project nên chuẩn hóa quy ước UTC riêng thay vì dựa vào `DateTime.Kind` của giá trị trả về.

Network time giúp giảm phụ thuộc vào đồng hồ thiết bị nhưng cache local vẫn chỉ là fallback. Không dùng service này làm nguồn xác thực duy nhất cho economy hoặc giao dịch có giá trị; quyết định quan trọng nên được kiểm tra ở server.

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
