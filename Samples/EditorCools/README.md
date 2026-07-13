# EditorCools Sample

Sample này hướng dẫn dùng các attribute Inspector nằm trong `Libraries/EditorCools`.

Mở scene:

```text
Assets/_GameFoundation/Samples/EditorCools/Scenes/Demo.unity
```

Chọn GameObject có component `EditorCoolsDemo`, sau đó xem Inspector.

## Nội dung

`EditorCoolsDemo` minh họa:

- `[Dropdown]` lấy option từ static method.
- `[Dropdown]` lấy option từ instance method.
- `[MinMaxSlider]` ghi giá trị min/max vào `Vector2`.
- `[EditorCools.Button]` tạo button gọi method trong Inspector.
- Button cùng `row` được vẽ chung một hàng.

## Ví dụ ngắn

```csharp
[Dropdown(nameof(GetOptions))]
public string option;

[MinMaxSlider(0f, 100f)]
public Vector2 range;

[EditorCools.Button(name: "Run")]
private void Run()
{
    Debug.Log("Run clicked");
}

private string[] GetOptions()
{
    return new[] { "A", "B", "C" };
}
```

Các attribute này chỉ thay đổi Inspector, không tự thêm runtime behaviour.
