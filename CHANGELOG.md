# Changelog

All notable changes to this package are documented in this file.

## [1.0.10] - 2026-07-10

### Changed

- Renamed `ThirdParty` to `Libraries` and `Third Party Notices.md` to `Library Notices.md`.
- Moved analytics integrations to root `Analytics/Firebases` and `Analytics/AppsFlyer`.
- Renamed Bacon UMP integration symbols/API references to Omni UMP with `ADMOB_UMP_OMNI`.
- Renamed haptics facade from `Vibrate` to `HapticFeedback`.
- Renamed visible package folders from `Documentation~` and `Samples~` to `Documentation` and `Samples`.

### Added

- Added package upgrade menu at `Packages/Upgrade GameFoundation`.
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
