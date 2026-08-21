# Changelog

All notable changes to this package are documented in this file.

## [Unreleased]

### Fixed

- Removed the invalid Git URL from `package.json` dependency metadata. Unity UPM accepts semantic versions there; UniTask must be installed as a direct project Git dependency.

## [1.0.18] - 2026-07-18

### Added

- Added `AnalyticsBinder` support for `paid_ad_impression_value` events with game-provided level and placement context.
- Added AdMob paid impression tracking from official `OnAdPaid` callbacks across banner, MREC, native, interstitial, rewarded, rewarded interstitial, and app open providers.
- Added paid ad impression integration notes under `Documentation/paid-ad-impression-value.md`.
- Added string-key playback support for `SoundService` and `MusicService`, plus a `ButtonSound` sound-key dropdown inspector.

### Changed

- Changed `AnalyticsService` provider setup from prefab instantiation to directly referenced provider objects.

## [1.0.17] - 2026-07-16

### Added

- Added detailed extension module sample documentation with MonoBehaviour and runtime-registered module examples.
- Added `RuntimeStorageExtension` convenience APIs: `Set`, `Get`, `String`, `Int`, `Float`, `Bool`, and `Object`.
- Added `RuntimeStorageExtention` legacy alias forwarding to `RuntimeStorageExtension` to preserve existing project code.
- Added `RuntimeStorageExtension` sample README under `Samples/ExtensionData`.
- Added `ExtensionDataClass.Remove` and wired `RuntimeStorageExtension.Delete` to remove keys and save immediately.

### Changed

- Expanded the README architecture diagram and restored the visual package header.
- Updated README and documentation references from legacy `AdManager`, `FirebaseManager`, `SoundManager`, and `MusicManager` APIs to current provider-based `AdvertisingService`, `AnalyticsService`, `SoundService`, and `MusicService` APIs.
- Updated sample documentation to use `RuntimeStorageExtension` instead of the removed temporary `GameSave` wrapper.
- Updated `SoundService` initialization log label.

### Fixed

- Removed stale `AdManager` resume comment from `ManagerCoordinator`.
- Corrected quick-start EventBus sample to use `GameMessageEvent.Key` and `Value`.

## [1.0.16] - 2026-07-13

### Added

- Added prefab-backed `AdvertisingService` with per-ad controllers for banner, interstitial, rewarded, rewarded interstitial, app open, native, MREC, and startup promise ads.
- Added generic advertising provider/controller APIs with waterfall and parallel mediation modes.
- Added AdMob banner and interstitial providers under `Advertising/Admob`.
- Added UMP consent helper behind `ADMOB_UMP`.
- Added prefab-backed `AnalyticsService` with provider fan-out, revenue tracking, user identity, user properties, and flush support.
- Added Firebase and AppsFlyer analytics providers, plus a separate AppsFlyer assembly definition.
- Added `ManagerCoordinator` to initialize analytics, UMP, and advertising through SDK startup events.
- Added extension module notice attributes and editor support.
- Added compact duration formatting and weekday-end countdown helpers to `UtilsTimerHelper`.

### Changed

- Reworked advertising from the legacy `GoogleMobileAds`/`JKit` managers into provider-based `Advertising`, `Admob`, and `Module` folders.
- Moved built-in service prefabs under `Runtime/Bootstrap/Prefabs`, including advertising and analytics prefabs.
- Renamed audio services from `MusicManager` and `SoundManager` to `MusicService` and `SoundService`.
- Moved `SoundButton` to `Runtime/Services/Audio/Extentions/ButtonSound`.
- Updated `Manager` startup flow to delegate SDK initialization through `ManagerCoordinator`.
- Updated SDK define documentation and editor define labels for AdMob, UMP, Firebase, AppsFlyer, and Unity IAP.
- Updated extension data sample and runtime storage editor behavior for current APIs.

### Fixed

- Fixed interstitial completion flow for AdMob interstitial ads.
- Fixed AdMob banner and interstitial provider initialization/load handling.
- Fixed startup promise ad timeout handling.
- Fixed analytics provider initialization errors so editor logging is optional.
- Fixed Unity IAP integration calls against current runtime storage APIs.

### Removed

- Removed legacy `Advertising/GoogleMobileAds` and `Advertising/JKit` ad manager implementations.
- Removed legacy `FirebaseManager` and `AppflyerEventTracking` in favor of provider-based analytics.

## [1.0.15] - 2026-07-13

### Changed

- Moved Unity IAP integration into a separate optional assembly gated by `IAPPURCHASE_ENABLE`.
- Registered Unity IAP initialization through `GameFoundationModuleRegistry` instead of a direct `Manager` reference.
- Required `com.unity.purchasing` to exist before compiling the optional Unity IAP assembly.
- Updated the Unity IAP assembly reference to `Unity.Purchasing` for Unity IAP 5.x.

### Fixed

- Prevented projects without Unity IAP from compiling `UnityEngine.Purchasing` references when IAP is disabled.
- Updated Unity IAP integration calls to current singleton and runtime storage APIs.

## [1.0.14] - 2026-07-10

### Changed

- Added generic `Set<T>` and `Get<T>` persistence APIs for player extension data.
- Added generic setting value support while preserving bool settings.
- Added shared `DeviceMotionEvent` for shake, tilt, and inertia-style device motion.
- Refactored player persistence obfuscation/checksum helpers to reduce duplicate code.

### Fixed

- Fixed setting defaults being overwritten by implicit `false` values during reads.
- Ensured rating state writes are persisted immediately.

## [1.0.12] - 2026-07-10

### Changed

- Moved EditorCools runtime attributes and editor drawers under `Libraries/EditorCools`.
- Updated GameFoundation editor menu paths and removed custom Git URL configuration.
- Refreshed the SDK Defines editor window UI.

## [1.0.10] - 2026-07-10

### Changed

- Renamed `ThirdParty` to `Libraries` and `Third Party Notices.md` to `Library Notices.md`.
- Moved analytics integrations to root `Analytics/Firebases` and `Analytics/AppsFlyer`.
- Renamed Bacon UMP integration symbols/API references to UMP with `ADMOB_UMP`.
- Renamed haptics facade from `Vibrate` to `HapticFeedback`.
- Renamed visible package folders from `Documentation~` and `Samples~` to `Documentation` and `Samples`.

### Added

- Added package upgrade menu at `GameFoundation/Packages/Upgrade Game Foundation`.
- Added documentation pages for quick start, SDK defines, and samples.
- Added README files for package samples.

### Removed

- Removed `Runtime/Configuration`.
- Removed legacy `GameEvent` in favor of `EventBus` events.
- Removed unused `SystemControlEvent` and `SystemType`.

## [1.0.0] - 2026-07-06

### Added

- Initial Unity Package Manager release.
- Runtime foundation utilities, services, features, integrations, and security helpers.
- Editor tooling and importable samples.
- Runtime, editor, and test assembly definitions.
