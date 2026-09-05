# Foundation Basics

Sample scene tổng hợp cho package foundation.

## Nội dung

- Bootstrap bằng `Manager` và `Concurrency`.
- Tạo UI runtime bằng UGUI.
- Lưu điểm vào `RuntimeStorageData.PLAYER.ExtensionData`.
- Publish/subscribe `GameMessageEvent` qua `EventBus`.
- Gọi integration haptics và device torch.

## Mở scene

Mở `Scenes/FoundationBasics.unity`, nhấn Play, sau đó dùng các nút trên màn hình.

## Lưu ý

- Haptics chỉ có hiệu lực trên thiết bị hỗ trợ.
- Torch chỉ có hiệu lực trên platform có camera flash và manifest phù hợp.
- Sample dùng key `foundation_basics.score`; game thật nên đổi prefix.
