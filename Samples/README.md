# Game Foundation Samples

Các sample này là ví dụ nhỏ để copy cách dùng API, không phải prefab production hoàn chỉnh.

## Danh sách

| Sample | Scene | Nội dung |
| --- | --- | --- |
| `EditorCools` | `EditorCools/Scenes/Demo.unity` | Dropdown, min-max slider, inspector button. |
| `ExtensionData` | `ExtensionData/Scenes/Demo.unity` | Lưu/đọc data mở rộng qua `RuntimeStorageData.PLAYER.ExtensionData`. |
| `ExtentionModules` | Không có scene | Code mẫu extension module có MonoBehaviour và không cần MonoBehaviour. |

## Cách dùng

1. Import sample từ Package Manager, hoặc mở trực tiếp thư mục `Assets/_GameFoundation/Samples`.
2. Với sample có scene, mở `Scenes/Demo.unity`.
3. Bấm Play.
4. Xem Console log và Inspector field.

## Lưu ý

- `ExtensionData` cần `Manager` chạy trước để `RuntimeStorageData.IsReady == true`.
- `ExtentionModules` giữ nguyên tên thư mục legacy đang có trong package.
- Khi copy code sang project thật, đổi key/prefix sample thành prefix riêng của game.
