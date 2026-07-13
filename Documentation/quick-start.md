# Quick Start

## Import

Add the package by Git URL in `Packages/manifest.json` or through Unity Package Manager.

```json
"com.dann.gamefoundation": "https://github.com/PocketPlay-Interactive/_GameFoundation.git"
```

After import, add a `Manager` prefab or GameObject with `Manager` to the first scene.

## Minimal Scene

1. Add `Manager`.
2. Add `LoadingCanvas` if the scene needs the built-in loading flow.
3. Keep SDK defines disabled until the matching SDK is imported.
4. Press Play and check the Console for `Manager` startup logs.

## Common Calls

```csharp
RuntimeStorageData.SaveAllData();
SoundService.Get().PlayOnShot(SoundService.Sound.BUTTON);
MusicService.Get().PlayMusic(MusicService.Music.NONE);
EventBus.Publish(new GameMessageEvent { Key = "hello", Value = null });
```

## Package Layout

- `Runtime`: core runtime, services, features, integrations.
- `Advertising`: AdMob and native ad adapters.
- `Analytics`: Firebase and AppsFlyer adapters.
- `Libraries`: bundled libraries and editor helper packages.
- `Samples`: example scenes and scripts.
