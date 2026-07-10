# Changelog

All notable changes to this package are documented in this file.

## [1.0.14] - 2026-07-10

### Changed

- Added generic `Set<T>` and `Get<T>` persistence APIs for player extension data.
- Added generic setting value support while preserving bool settings.
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
- Renamed Bacon UMP integration symbols/API references to Omni UMP with `ADMOB_UMP_OMNI`.
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
