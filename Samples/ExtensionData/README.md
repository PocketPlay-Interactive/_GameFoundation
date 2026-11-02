# ExtensionData Sample

Sample này hướng dẫn lưu data riêng của project vào `RuntimeStorageData.PLAYER.ExtensionData`.

API ngắn hơn: xem mục `RuntimeStorageExtension` ở cuối.

Mở scene:

```text
Samples/ExtensionData/Scenes/Demo.unity
```

Bấm Play, sau đó xem Console.

## Flow

`ExtensionDataDemo` đợi:

```csharp
yield return new WaitUntil(() => RuntimeStorageData.IsReady);
```

Sau đó sample:

- Lưu `string`.
- Lưu `int`.
- Lưu `float`.
- Lưu `bool`.
- Lưu object bằng `JsonUtility.ToJson`.
- Đọc lại data bằng `Get<T>()`.
- Gọi `RuntimeStorageData.SaveAllData()`.

## API chính

```csharp
ExtensionDataClass extensionData = RuntimeStorageData.PLAYER.ExtensionData;

extensionData.Set("project.player_name", "Player One");
extensionData.Set("project.score", 1234);
extensionData.Set("project.move_speed", 2.5f);
extensionData.Set("project.is_vip", true);

string playerName = extensionData.Get("project.player_name", "Unknown");
int score = extensionData.Get("project.score", 0);
float moveSpeed = extensionData.Get("project.move_speed", 0f);
bool isVip = extensionData.Get("project.is_vip", false);

RuntimeStorageData.SaveAllData();
```

## Lưu object

```csharp
string json = JsonUtility.ToJson(playerProfile);
RuntimeStorageData.PLAYER.ExtensionData.Set("project.profile_json", json);

string savedJson = RuntimeStorageData.PLAYER.ExtensionData.Get<string>("project.profile_json");
PlayerProfileData profile = JsonUtility.FromJson<PlayerProfileData>(savedJson);
```

## Quy ước

- Key nên có prefix riêng, ví dụ `my_game.score`.
- Chỉ đọc sau khi `RuntimeStorageData.IsReady == true`.
- Gọi `RuntimeStorageData.SaveAllData()` sau khi ghi data quan trọng.
- `JsonUtility` serialize field, không serialize property.

## RuntimeStorageExtension

`RuntimeStorageExtension` là wrapper ngắn cho `RuntimeStorageData.PLAYER.ExtensionData`. `Set()` tự gọi `RuntimeStorageData.SaveAllData()`.

```csharp
RuntimeStorageExtension.Set("coin", 100);
int coin = RuntimeStorageExtension.Int("coin");

RuntimeStorageExtension.Set("remove_ads", true);
bool removeAds = RuntimeStorageExtension.Bool("remove_ads");

RuntimeStorageExtension.Set("player_name", "Player One");
string playerName = RuntimeStorageExtension.String("player_name");

RuntimeStorageExtension.Delete("coin");
```

### Lưu object

Object cần tương thích `JsonUtility`: có `[System.Serializable]`, dùng field thay vì property.

```csharp
[System.Serializable]
public class ProfileData
{
    public string name;
    public int level;
}

RuntimeStorageExtension.Set("profile", new ProfileData { name = "Player One", level = 10 });
ProfileData loaded = RuntimeStorageExtension.Object<ProfileData>("profile");
```
