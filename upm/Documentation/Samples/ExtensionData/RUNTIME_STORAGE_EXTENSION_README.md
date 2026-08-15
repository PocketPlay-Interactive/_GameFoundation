# RuntimeStorageExtension Sample

`RuntimeStorageExtension` là wrapper ngắn cho `RuntimeStorageData.PLAYER.ExtensionData`.

Dùng khi project cần lưu data riêng nhanh mà không muốn gọi API dài.

## API nhanh

```csharp
RuntimeStorageExtension.Set("coin", 100);
int coin = RuntimeStorageExtension.Int("coin");

RuntimeStorageExtension.Set("remove_ads", true);
bool removeAds = RuntimeStorageExtension.Bool("remove_ads");

RuntimeStorageExtension.Set("player_name", "Player One");
string playerName = RuntimeStorageExtension.String("player_name");

RuntimeStorageExtension.Delete("coin");
RuntimeStorageData.SaveAllData();
```

## Lưu object

Object cần tương thích `JsonUtility`: có `[System.Serializable]`, dùng field thay vì property.

```csharp
[System.Serializable]
public class ProfileData
{
    public string name;
    public int level;
}

var profile = new ProfileData
{
    name = "Player One",
    level = 10
};

RuntimeStorageExtension.Set("profile", profile);
ProfileData loaded = RuntimeStorageExtension.Object<ProfileData>("profile");
```

## Lưu ý

- Chỉ dùng sau khi `RuntimeStorageData.IsReady == true`.
- `Set()` tự gọi save xuống disk qua `RuntimeStorageData.SaveAllData()`.
- Key nên có prefix riêng của game, ví dụ `my_game.coin`.
