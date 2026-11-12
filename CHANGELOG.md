# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Added
- **Display Name** module (`NineSixFour.GameDevelopmentToolkit.DisplayName`): configurable Unicode-aware validation, options/profiles/configuration, and result/error reporting.
- **Timer** module (`NineSixFour.GameDevelopmentToolkit.Timer`): countdown/count-up timers, `TimerManager`, `Stopwatch`, time providers (`ITimeProvider`, `SystemTimeProvider`, `ManualTimeProvider`), runtime adjustments/pending time, and lifecycle events.
- **Settings** module (`NineSixFour.GameDevelopmentToolkit.Settings`): `SettingsStore` with typed access, defaults, change events, and `ISettingsStorage` abstraction (`InMemorySettingsStorage`).
- **Save Data** module (`NineSixFour.GameDevelopmentToolkit.SaveData`): `SaveManager` with slot metadata envelope, storage/serializer abstractions, load limits, rename/delete, `Try*`/result APIs, and events.
- **CSMusicPlayer** module (`NineSixFour.GameDevelopmentToolkit.CSMusicPlayer`): core-driven playback logic via `IAudioBackend`, playlists/shuffle/loop/favorites, game-state mappings, and tag read/write abstractions with in-memory metadata editing.
- Automated and manual testers for all implemented modules (JSON + TXT logs).
- Documentation: `docs/installation.md`, `docs/guidelines.md`, and per-module docs in `docs/modules/`.
- README rewritten as documentation/index landing page.

### Changed
- Test log output standardized to the repository `logs\` directory (resolved from solution root) for automated and manual testers.

## [0.1.0] - 2026-10-30

### Added
- Project foundation and repository scaffolding
- MIT License and NOTICE for attribution
- UPM `package.json` for Unity Package Manager
- Solution structure (`src`, `tests`, `samples`, `docs`, `integrations`)
- Shared build configuration
