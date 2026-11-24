<p align="center">
  <img src="Documentation/Images~/banner_1.jpg" alt="Game Foundation Banner" width="100%">
</p>

<h1 align="center">⚡ Game Foundation</h1>

<p align="center">
  <strong>Nền tảng Unity dùng chung cho hệ sinh thái game của PocketPlay Interactive.</strong>
  <br/>
  Tái sử dụng nhanh hơn. Mở rộng sạch hơn. Khởi động dự án mới gọn hơn.
</p>

<p align="center">
  <img src="https://img.shields.io/badge/Unity-2022.3+-000000?logo=unity&logoColor=white&style=flat-square">
  <img src="https://img.shields.io/badge/UPM-Git%20Package-2C8EBB?style=flat-square">
  <img src="https://img.shields.io/badge/Package-com.dann.gamefoundation-6A5ACD?style=flat-square">
  <img src="https://img.shields.io/badge/Status-Active-2EA44F?style=flat-square">
  <img src="https://img.shields.io/badge/Owner-PocketPlay-FF6B6B?style=flat-square">
  <a href="CHANGELOG.md"><img src="https://img.shields.io/badge/Changelog-1.0.24-0366D6?style=flat-square"></a>
</p>

---

Core Unity dùng chung: bootstrap, lưu dữ liệu, audio, pooling, UI và các tiện ích nền. Code gameplay, giao diện và cấu hình riêng đặt trong project game.

**Yêu cầu:** Unity 2022.3 trở lên. Package ID: `com.dann.gamefoundation`.

## Bắt đầu nhanh

1. Trong Unity, mở **Window → Package Manager → + → Add package from git URL...** và nhập:

   ```text
   https://github.com/PocketPlay-Interactive/_GameFoundation.git
   ```

   Repository private cần Git credentials phù hợp. Với bản local, chọn **Add package from disk...** và chọn `package.json` của Game Foundation.
2. Mở scene đầu tiên, chọn **GameFoundation → Scene → Create Minimal Setup**. Menu tạo `Manager`, `ManagerCoordinator` và `Concurrency` trong scene. Chọn **Create Full Setup** nếu cần thêm loading, UI manager và audio.
3. Thêm gameplay và UI của game vào scene. Nếu cần khởi tạo data riêng, thêm một [module của game](#module-của-game).
4. Lưu scene và đưa scene vào build settings.

`Manager` đọc save khi khởi động, khởi tạo các module rồi phát `OnFoundationReady`. `ManagerCoordinator` mặc định có thời gian loading tối thiểu 15 giây; chỉnh `Completion Delay Seconds` trong Inspector theo game. Nếu tắt `Auto Complete On Timeout`, game cần gọi `CompleteLoading()` để kết thúc loading.

## Mở rộng từng phần

<details>
<summary><strong>Module của game</strong> — khởi tạo logic riêng mà không sửa core</summary>

Đặt script trong project game, ví dụ `Assets/_Project/ProjectBootstrapModule.cs`:

```csharp
using System.Collections;

public class ProjectBootstrapModule : GameFoundationModuleBehaviour
{
    public override IEnumerator Initialize()
    {
        // Khởi tạo data và service riêng của game tại đây.
        yield break;
    }
}
```

Gắn component vào scene, kéo component vào **Manager → Extension Modules**. Module chạy theo `Order`, sau khi save đã được đọc. Cũng có thể đăng ký object implement `IGameFoundationModule` qua `GameFoundationModuleRegistry.Register(...)` trước bước khởi tạo module của `Manager`.

</details>

<details>
<summary><strong>Lưu dữ liệu</strong> — thêm data riêng cho game</summary>

Sau khi `Manager` đọc save, dùng `RuntimeStorageExtension` với key riêng cho project:

```csharp
RuntimeStorageExtension.Set("project.best_score", 1200);
int bestScore = RuntimeStorageExtension.Int("project.best_score", 0);
```

`Set` gọi lưu sau khi cập nhật. Có thể dùng `String`, `Float`, `Bool` hoặc `Object<T>` để đọc các kiểu khác; object cần tương thích với `JsonUtility`. `Manager` cũng lưu khi ứng dụng pause hoặc quit.

Nếu file save không đọc được, hệ thống cố tạo bản sao khôi phục trước khi ghi dữ liệu mặc định. Không dùng local save làm bằng chứng bảo mật hoặc bằng chứng giao dịch.

</details>

<details>
<summary><strong>Screen và popup</strong> — dùng UI manager có sẵn</summary>

1. Tạo screen từ `ScreenCanvas`, popup từ `PopupCanvas`.
2. Gắn `CanvasManager` vào scene hoặc dùng **Create Full Setup**. Trong Inspector, thêm ID và component tương ứng vào `Canvas Roots` và `Popup Roots`.
3. Khi dùng chế độ tự đổi mặc định, gửi event:

```csharp
EventBusExtensions.PublishCanvas("Home");
EventBusExtensions.PublishPopup("Settings");
```

Chuyển screen sẽ ẩn screen và popup đã đăng ký trước khi hiện screen mới. Mở popup giữ screen phía sau và ẩn các popup khác. Khi tắt `Auto Change Canvas` hoặc `Auto Change Popup`, gọi `CanvasManager.Get().OnCanvas(id)` hoặc `OnPopup(id)` tương ứng. Gửi canvas event từ Unity main thread.

</details>

<details>
<summary><strong>Audio và pooling</strong> — dùng service có sẵn</summary>

**Create Full Setup** thêm `SoundService` và `MusicService` vào scene và đăng ký chúng với `Manager`. Cấu hình audio và key theo nhu cầu trong Inspector.

```csharp
SoundService.Get().PlayOnShot("click");
MusicService.Get().PlayMusic("background");

GameObject instance = Pooling.Get().GetPrefab(prefab);
Pooling.Get().PushToPool(instance);
```

`prefab` là một `GameObject` do game cung cấp. Các key audio phải khớp cấu hình của service.

</details>

<details>
<summary><strong>Unity IAP</strong> — tích hợp mua hàng tùy chọn</summary>

Cài Unity IAP qua **GameFoundation → Packages → Install InappPurchase**, sau đó bật `IAPPURCHASE_ENABLE` tại **GameFoundation → Configuration → SDK Defines**. Assembly IAP yêu cầu Unity Purchasing từ phiên bản 5.0.0 và define này; khi thiếu một trong hai, assembly không được biên dịch.

**Giới hạn hiện tại:** quyền lợi được xử lý sau khi store xác nhận đơn. Nếu ứng dụng dừng trước khi thưởng được lưu, consumable có thể bị mất; subscription/battle pass cần xử lý quyền lợi riêng. Chưa nên dùng luồng này cho giao dịch thật khi chưa hoàn thiện và kiểm tra cơ chế giao thưởng của game.

</details>

<details>
<summary><strong>Cài đặt và âm thanh</strong> — setting, sound, music, nút bấm</summary>

**Setting (bật/tắt nhạc, âm thanh, rung)** nằm trong `RuntimeStorageData.SETTING`. Ba khóa mặc định là `MUSIC`, `SOUND`, `VIBRATE` (không phân biệt hoa thường), mặc định bật:

```csharp
RuntimeStorageData.SETTING.Set("music", false);
bool soundOn = RuntimeStorageData.SETTING.Get("sound", true);
RuntimeStorageData.SaveAllData();
```

`SETTING.Set` chỉ đổi trong bộ nhớ; gọi `SaveAllData()` để ghi xuống file. `SETTING` và `PLAYER` chỉ dùng được sau khi `Manager` đọc save.

**Dữ liệu người chơi** có sẵn trong `RuntimeStorageData.PLAYER`: `GoldBalance`, `CurrentLevelIndex`, `LoginDayCount`, `LastLogin`, `SelectedLanguage`, `OwnedProductIds`. Các giá trị số được che và kiểm tra checksum trong bộ nhớ để chống sửa tay file save. Đây là che giấu chứ không phải mã hóa chống tấn công có chủ đích.

**SoundService** phát hiệu ứng theo key. Cấu hình bảng `Sounds` (key và danh sách clip) trong Inspector; nhiều clip cùng key sẽ được chọn ngẫu nhiên.

```csharp
SoundService.Get().PlayOnShot("click", 0.8f);
SoundService.Get().PlayOnShot(audioClip);
SoundService.Get().PlaySpecialSound("win");
SoundService.Get().StopSpecialSound();
```

**MusicService** phát nhạc nền theo key trong bảng `Musics`:

```csharp
MusicService.Get().PlayMusic("home", volume: 1f, loop: true);
MusicService.Get().SetVolume(0.3f, duration: 1f);
MusicService.Get().StopMusic();
```

`SetMute()` trên cả hai service áp dụng lại giá trị trong setting `MUSIC` hoặc `SOUND`. Gắn component `ButtonSound` vào nút bấm và nhập `soundKey` (hoặc gán `fallbackClip`) để nút tự phát âm thanh khi nhấn xuống.

**Chống bấm nút liên tục:** `ButtonSafe` (cần `Button`) khóa nút trong thời gian `Cooldown` (mặc định 0.5 giây) sau mỗi lần bấm. `SmartButton` thêm hiệu ứng nhấn bằng DOTween và tự yêu cầu `ButtonSafe`; chọn kiểu chuyển động trong Inspector.

</details>

<details>
<summary><strong>Sự kiện, singleton và tiện ích nền</strong> — EventBus, Concurrency, ServiceLocator, log</summary>

**EventBus** gửi sự kiện giữa các thành phần không biết nhau. Đăng ký ở `OnEnable`, hủy ở `OnDisable`:

```csharp
void OnEnable()  => EventBus.Subscribe<TouchBeganEvent>(OnTouch);
void OnDisable() => EventBus.Unsubscribe<TouchBeganEvent>(OnTouch);
void OnTouch(TouchBeganEvent e) { /* e.Position */ }
```

Sự kiện có sẵn: `GameMessageEvent` (gửi khóa và giá trị tùy ý qua `EventBusExtensions.PublishGameMessage(key, value)`), `CanvasEvent`, `PopupEvent`, `TouchBeganEvent`, `TouchHoldEvent`, `TouchEndedEvent`, `DeviceMotionEvent` và `PurchaseCompletedEvent`. Có thể tự định nghĩa class sự kiện riêng. Lỗi trong một callback được ghi log và không chặn các callback còn lại.

**Singleton**
- `Singleton<T>`: `MonoBehaviour`, một bản trong scene, hủy khi đổi scene. Dùng `T.Get()`.
- `SingletonGlobal<T>`: `MonoBehaviour`, giữ qua các scene (`DontDestroyOnLoad`). Đặt `SingletonGlobal<T>.IsAutoCreate = true` nếu muốn tự tạo khi chưa có.
- `GenericSingleton<T>`: class thường, tạo khi gọi `T.Get()`.

**Concurrency** đưa việc về main thread, an toàn khi gọi từ luồng khác:

```csharp
Concurrency.Instance().Enqueue(() => Debug.Log("main thread"));
Concurrency.Instance().Enqueue(MyCoroutine(), delaySeconds: 0.5f);
await Concurrency.Instance().EnqueueAsync(() => DoSomething());
```

**ServiceLocator** đăng ký và lấy service theo kiểu: `ServiceLocator.Register<IMyService>(impl)`, `ServiceLocator.Get<IMyService>()` (trả `null` nếu chưa đăng ký), `Unregister<T>()`.

**Khác:** `GlobalCoroutineRunner.Run(routine)` chạy coroutine mà không cần `MonoBehaviour`; `ObjectPool<T>` là pool đơn giản cho prefab; `LogSystem` có `LogSuccess`, `LogWarning`, `LogError` in log có màu.

</details>

<details>
<summary><strong>Tiện ích: extension, thời gian, JSON, file</strong></summary>

**Thời gian** — `UtilsTimerHelper` làm việc theo giây từ mốc 1970: `CurrentTimeInSeconds()`, `TimeInSecondsAfterMinutes(m)`, `SecondsToTarget(dateTime)`, `EndOfDayInSeconds()`, `ToTimeString(seconds)`, `FormatCompactDuration(timeSpan)`. Đây là giờ của thiết bị; cần giờ mạng thì dùng `NetworkTime.GetTimeAsync()`.

**Giờ mạng** — `NetworkTime.GetTimeAsync()` lấy giờ từ mạng và dùng giá trị cache gần nhất khi mất mạng. Không dùng làm nguồn xác thực duy nhất cho giao dịch hay kinh tế game.

**Extension hay dùng**
- `GameObject`, `Transform`, `Component`: `Show()`, `Hide()`, `SetActiveSafe(bool)`.
- Hierarchy: `FindChildRecursive(name)`, `FindDirectChild(name)`, `HasDirectChild(name)`, `GetChildAt(index)`, `FindNearest(list)`.
- Chuỗi: `AddSpaceBeforeUppercase()`, `ToTitleCase()`, `ToEnum(defaultValue)`.

**Tài nguyên** — `ResourceHandle.Get()` đọc và cache asset từ `Resources`: `Load<T>(path)`, `LoadPrefab(name)` (thư mục `Resources/Prefabs`), `LoadData<T>(name)` (thư mục `Resources/Data`), `Unload(path)`, `UnloadAll()`. `ImageHandler.Get().DownloadImageAsync(url, fileName)` tải ảnh về thư mục `Assets` và chỉ dùng trong Editor.

**JSON và file** — `UtilsJsonHelper` chuyển mảng và danh sách bằng `JsonUtility`. `FileHandler` đọc/ghi file theo đường dẫn `Application.dataPath`, dành cho công cụ Editor; lưu dữ liệu người chơi nên dùng `RuntimeStorageExtension`.

**Bảo mật nhẹ** — `MemoryObfuscation` (`EncryptInt`, `ObfuscateString`) và `MemoryChecksum` (`GenerateChecksum`, `VerifyChecksum`) giúp giá trị trong bộ nhớ khó bị sửa bằng công cụ phổ thông. Không dùng làm mã hóa bảo mật. `HashLib` chỉ để tương thích định dạng save cũ.

</details>

<details>
<summary><strong>Input, chạm, raycast và lắc</strong></summary>

Cần `InputManager` trong scene để các hệ thống bên dưới hoạt động. `InputManager` dùng Input System, tạo và bật `InputActions` khi khởi động. Bật `Is Debug Input` trong Inspector để ghi log input; mặc định tắt vì log mỗi frame gây cấp phát.

**TouchSystem** phát `TouchBeganEvent`, `TouchHoldEvent`, `TouchEndedEvent` qua `EventBus`. Chạm lên UI không phát sự kiện gameplay; nếu một lần chạm đang diễn ra mà con trỏ chuyển lên UI, hệ thống phát `TouchEndedEvent` để các nơi nhận được cặp Began/Ended đầy đủ.

**RaycastSystem** bắn tia từ vị trí chạm. Chọn `Mode2D` hoặc `Mode3D`, gán `Raycast Camera` (mặc định camera chính) và `Ignore Raycast Layer`. Component nhận kết quả triển khai `IRaycastEventsListener`:

```csharp
public class Picker : MonoBehaviour, IRaycastEventsListener
{
    void OnEnable()  => RaycastSystem.RegisterListener(this);
    void OnDisable() => RaycastSystem.UnregisterListener(this);

    public void OnRaycast3DBegan(RaycastHit hit) { /* trúng collider 3D */ }
    public void OnRaycast2DBegan(RaycastHit2D hit) { /* trúng collider 2D */ }
    public void OnRaycastBeganPosition(Vector2 position) { }
    public void OnRaycastDrag(Vector2 position) { }
    public void OnRaycastEnded(Vector2 position) { }
}
```

**ShakeSystem** đọc gia tốc kế và phát `DeviceMotionEvent` với `DeviceMotionType.Shake`. Có thể bật/tắt hoặc đổi ngưỡng bằng `SetShakeDetection(bool)`, `SetShakeThreshold(float)` hoặc gửi `GameMessageEvent` với khóa `ShakeSystem.DetectionKey` / `ShakeSystem.ThresholdKey`. Trong Editor, nhấn phím test hoặc dùng menu chuột phải **Test Shake** trên component.

</details>

<details>
<summary><strong>Hiệu ứng UI và hành vi dùng chung</strong></summary>

Các component trong `Runtime/Features/CommonBehaviours` dùng DOTween, gắn trực tiếp vào đối tượng:

| Component | Công dụng |
| --- | --- |
| `SmartButton` | Hiệu ứng nhấn/hover cho nút; kiểu chuyển động chọn trong Inspector |
| `SmartPopup` | Chạy hiệu ứng mở popup khi bật đối tượng; có `DWCompleteEvent` |
| `SmartCanvas` | `FadeIn()` / `FadeOut()` cho `CanvasGroup`; có tự fade khi bật |
| `SmartMove` | `MoveX(target)`, `FadeTo(alpha)` kèm `OnComplete` |
| `SmartResetObject` | Gọi `OnResetEvent` khi đối tượng bị tắt |
| `SmartRotation`, `SmartProcess`, `SmartDropdown`, `SmartImageEffect`, `MoveFoward` | Xoay, tiến trình, thả xuống, hiệu ứng ảnh và di chuyển thẳng |

Component dùng `Concurrency` (như `SmartPopup`, `SmartCanvas`) cần `Concurrency` có trong scene. Các lớp `ScreenCanvas` và `PopupCanvas` xem mục [Screen và popup](#mở-rộng-từng-phần); `PopupCanvas` yêu cầu implement `Initialized(primaryCallback, secondaryCallback, msg)` để truyền callback và nội dung vào popup.

</details>

<details>
<summary><strong>Combat text, haptics, torch</strong> — tính năng tùy chọn</summary>

**Combat text** hiển thị số sát thương bay lên. Tạo prefab `FloatingDamageInstance` (cấu hình sprite chữ số), gán vào `FloatingDamageSprites.damageInstancePrefab`, chọn `moveType`, rồi gọi:

```csharp
floatingDamage.DisplayDamage(damage: 250, pos: hitPoint, crit: true);
```

**Haptics (rung)** cần assembly Haptics và NiceVibrations. Lệnh chỉ chạy khi `HapticFeedback.IsEnabled` bật và setting `VIBRATE` bật:

```csharp
HapticFeedback.Get().Selection();     // chạm nhẹ khi chọn
HapticFeedback.Get().Success();       // thành công
HapticFeedback.Get().HeavyImpact();   // va chạm mạnh
HapticFeedback.Get().PlayContinuous(intensity: 0.8f, sharpness: 0.5f, duration: 0.3f);
```

**Torch (đèn pin)** cho Android và iOS: `Torch.Initialize()`, `Torch.TurnOn()`, `Torch.TurnOff()`, `Torch.Blink(onDuration, offDuration, count)`, `Torch.StopBlink()`, `Torch.Cleanup()`. Torch chỉ hoạt động trên thiết bị thật; trong Editor các lệnh không có tác dụng. Android cần khai báo trong manifest:

```xml
<uses-permission android:name="android.permission.CAMERA" />
<uses-feature android:name="android.hardware.camera" android:required="false" />
<uses-feature android:name="android.hardware.camera.flash" android:required="false" />
```

</details>

<details>
<summary><strong>Unity IAP — cấu hình và API</strong></summary>

1. Tạo danh sách sản phẩm: **Create → IAP → InappProductList**, đặt tên `InappProductList` và để trong `Resources/Data/`.
2. Thêm từng sản phẩm: `ID`, `ProductId` (khớp với store), `ProductName`, `ProductPrice`, `Type`, `Subscription` (để `None` nếu không phải gói đăng ký).
3. Khi `IAPPURCHASE_ENABLE` bật, `InappPurchaseModule` tự khởi tạo store lúc khởi động và chờ tối đa 10 giây.

```csharp
InappPurchase.Get().BuyProduct("com.game.remove_ads", success =>
{
    if (success) Debug.Log("Mua thành công");
});

string price = InappPurchase.Get().GetLocalizedPrice("com.game.remove_ads");
InappPurchase.Get().RestorePurchases();
```

Gắn xử lý quyền lợi vào `PurchaseCompletedEvent`:

```csharp
EventBus.Subscribe<PurchaseCompletedEvent>(e => Grant(e.ProductId));
```

Khi mở game, hệ thống tự kiểm tra lại với store và khôi phục sản phẩm non-consumable đã mua; nếu không có mạng thì dùng dữ liệu đã lưu trên máy. Đọc phần giới hạn ở mục [Unity IAP](#mở-rộng-từng-phần) trước khi dùng cho giao dịch thật.

</details>

<details>
<summary><strong>Công cụ Editor</strong> — menu <em>GameFoundation</em></summary>

| Menu | Công dụng |
| --- | --- |
| Scene → Create Minimal / Full Setup | Dựng bootstrap vào scene đang mở |
| Configuration → SDK Defines | Bật/tắt define SDK theo build target |
| Packages → Install Dependencies / Install InappPurchase / Upgrade GameFoundation | Cài package phụ thuộc, Unity IAP, cập nhật Game Foundation |
| Data Tables → Runtime Storage | Xem và sửa dữ liệu save khi chạy |
| Build → Build & Store Settings | Đặt tên, version, Application ID và thông tin ký app cho Android/iOS |
| Tools → Delete Saved Data | Xóa dữ liệu save của package |
| Tools → Capture Camera / PNG Resizer / Repair Missing Scripts | Chụp camera ra PNG, đổi kích thước ảnh, sửa script bị thiếu |
| GameObject → Copy Relative Path | Sao chép đường dẫn tương đối của đối tượng |

Menu **Tools → Delete Saved Data** xóa file save của package và cả toàn bộ `PlayerPrefs` của project; sao lưu trước khi dùng.

</details>

<details>
<summary><strong>Thành phần tùy chọn và assembly</strong></summary>

| Thành phần | Assembly | Phụ thuộc chính |
| --- | --- | --- |
| Core và service | `Dann.GameFoundation` | Input System, TextMesh Pro, UGUI; DOTween và EditorCools đóng gói trong `Libraries` |
| Combat text | `Dann.GameFoundation.CombatText` | Core, DOTween, EditorCools |
| Haptics | `Dann.GameFoundation.Haptics` | Core, NiceVibrations |
| Unity IAP | `Dann.GameFoundation.UnityIAP` | Core, Unity Purchasing và define IAP |

Các assembly tùy chọn vẫn nằm trong cùng package và được Unity tự tham chiếu khi có mặt; tách assembly không tự loại chúng khỏi package. Assembly riêng của game cần khai báo reference đến assembly tính năng mà nó dùng. `Samples/FoundationBasics` có gọi Haptics, nên cần giữ Haptics nếu dùng sample đó.

DOTween, EditorCools và NiceVibrations là thư viện bên thứ ba; xem [Library Notices.md](Library%20Notices.md) trước khi phân phối.

</details>

<details>
<summary><strong>Samples, cấu trúc và xử lý lỗi</strong></summary>

Chọn Game Foundation trong Package Manager rồi import sample từ mục **Samples**. Với package được đặt trực tiếp trong `Assets`, xem các scene trong `Samples`.

| Thư mục | Nội dung |
| --- | --- |
| `Runtime/Core` | Pattern, event, module, threading và tiện ích |
| `Runtime/Services` | Save, audio, asset, pooling và network time |
| `Runtime/Features` | UI, interaction và các tính năng tùy chọn |
| `Runtime/Integrations` | IAP, haptics và device API |
| `Editor` | Công cụ setup và cấu hình |
| `Libraries`, `Tests`, `Samples` | Thư viện đi kèm, test và ví dụ |

Nếu scene báo thiếu `Concurrency`, chạy **Create Minimal Setup** trong scene bootstrap. Nếu có lỗi compile sau khi bật SDK define, kiểm tra SDK tương ứng đã được cài. Giữ các file `.meta` trong Git để bảo toàn GUID của asset và reference.

</details>
