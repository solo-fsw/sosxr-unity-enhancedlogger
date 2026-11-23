# Changelog

All notable changes to this project will be documented in this file.
The changelog format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/)

## [1.2.4] - 2026-11-23

### Fixed

- The package did not compile on Unity 6000.0 and 6000.1, which `package.json` supports: the default colors used `Color.softRed`, `Color.orange`, `Color.cornflowerBlue`, `Color.khaki`, `Color.softGreen` and `Color.hotPink`, which exist from 6000.2. The defaults are now the same colors written as RGB values.
- Drawing the Scene view buttons created `Assets/_SOSXR/Settings/EnhancedLoggerSettings.asset` in a project that had none, and read the log level seven times per GUI event. The buttons now read the level once, from the loaded or existing asset, and never create it.
- The Scene view buttons reset the background color to white instead of the previous color.

### Added

- Menu item `SOSXR > EnhancedLogger > Show Scene View Buttons`, and `LogButtons.ShowSceneViewButtons` (EditorPrefs, default on).
- EditMode tests: `EnhancedLoggerSettingsDefaultsTests`.

### Changed

- `LogButtons` is a static class instead of a `MonoBehaviour` with `[ExecuteAlways]`. It is in an Editor-only assembly, so as a component it would be missing in builds.

## [1.2.3] - 2026-11-18

### Fixed

- The package no longer ships `Resources/EnhancedLoggerSettings.asset`. In the Editor, `Resources.Load` found that copy inside the immutable package, and changing the log level modified it (Unity warning: "assets located in immutable packages were unexpectedly altered").

### Changed

- The settings asset lives at `Assets/_SOSXR/Settings/EnhancedLoggerSettings.asset` and is created there when missing.
- The Editor loads the settings only from `Assets/_SOSXR/Settings/EnhancedLoggerSettings.asset`. It never uses `Resources.Load`.
- Players load the settings as a Preloaded Asset (`EnhancedLoggerSettings.Instance`, set in `OnEnable`). Players no longer use `Resources.Load`.
- Upgrading: the old `Resources/EnhancedLoggerSettings.asset` is not used or moved. A new asset with default values is created; set the log level again.

### Added

- Settings in **Preferences > SOSXR > EnhancedLogger** (`EnhancedLoggerPreferences`), and menu item `SOSXR > EnhancedLogger > Preferences`.
- `EnhancedLoggerSettingsBuild`: before each build, adds the settings asset to the Preloaded Assets in Player Settings, and removes other `EnhancedLoggerSettings` entries.
- README: "Settings Location".

### Tests

- `EnhancedLoggerSettingsLocationTests`: settings path, no settings asset in the package, no settings loaded from `Packages/`, Preferences path, and the Preloaded Assets list update.

## [1.2.2] - 2026-11-18

### Fixed

- `StackTraceInBuild` no longer sets the Log stack trace to Full in the Editor. Full stack traces stopped Console double-click from opening the calling code. The Editor now keeps the Console / Player Settings value. Development builds still use Full; Release builds still use None.

### Added

- README: "Stack Trace Setting" under "Console Double-Click".

### Tests

- `StackTraceInBuildTests`: Editor leaves the setting unchanged; Development builds use Full; Release builds use None.

## [1.2.1] - 2026-11-18

### Added

- Double-clicking a logger message in the Console opens the code that called the logger, not the `Debug.Log` call in `Log.cs`. Works with the "Script Only" and "Full" stack trace logging options. See `Editor/OpenLogCaller.cs`.
- README section "Console Double-Click".
- `[HideInCallstack]` on the logging methods. With "Strip logging callstack" on in the Console menu, the logger frames are hidden from the stack trace.

### Changed

- Development-build checks use `Debug.isDebugBuild` at runtime instead of the `DEVELOPMENT_BUILD` symbol. Unity 6.6 deprecates that symbol (warning UAC0009) and removes it in 6.8. Affects `Log.ReportLogLevel`, the errors-only filter in non-development players, and `StackTraceInBuild`. Behaviour is unchanged and works on every supported Unity version.
- New internal `Log.PlayerAllowsLog(LogLevel, bool)` holds the player filter, so it can be tested in EditMode.

### Fixed

- Doc comments on `Log.Static` no longer say double-click to the calling line is lost.
- In the editor, when the settings asset exists on disk but cannot be loaded yet (first import, Library rebuild), the temporary settings are no longer kept for the whole domain. Loading the asset is retried at most once per second.
- Messages logged with `LogLevel.None` are never shown. They used to pass the filter at every log level, including `None`.

### Tests

- `LogCallerParserTests`: finds the caller in both stack trace formats.
- `OpenLogCallerTests`: fails when Unity renames the private Console members that the double-click redirect reads.
- `MessageAtLevelNone_IsNeverShown`.

- `PlayerLogFilterTests`: errors-only in production players, all levels in development players.
- Moved all tests from PlayMode (`Tests/Runtime/`) to EditMode (`Tests/Editor/`); the test assembly is now Editor-only
- Converted `[UnityTest]` coroutine tests to plain `[Test]` methods; logging is synchronous, so the one-frame wait was not needed

## [1.1.0] - 2026-04-24

### Performance

- Replaced `Debug.LogFormat` with `Debug.Log` to eliminate `object[]` allocation per log message
- Removed unnecessary brace escaping (`{{`/`}}`) now that `LogFormat` is no longer used
- Replaced `StringBuilder` in `StringCombiner` with `string.Concat` overloads
- Replaced interpolated strings with nested `string.Concat` calls in hot path to avoid `string[]`/`object[]` allocations
- Changed `WriteToFile.LogEntry` from `class` to `struct` to remove per-unique-message heap allocation
- Fixed `Int32` boxing in location string building by avoiding mixed-type `string.Concat`

### Documentation

- Updated README with `Lifecycle` section describing `Application.quitting` delegate subscription/unsubscription
- Improved XML documentation on `WriteFinalFile` explaining delegate unsubscribe behavior
- Updated `Init` comments to mention event delegate registration

## [1.0.2] - 2026-04-01

### Fixed

- Bugfix: unsubscribe the WriteFile method
- Better default settings

## [1.0.1] - 2026-03-30

### Fixed

- Build tests

## [1.0.0] - 2026-03-30

### Added

- Comprehensive test suite with 25+ tests covering all log levels, static methods, multiple message overloads, and edge cases
- Test isolation with SetUp/TearDown methods for proper cleanup
- Static method tests for `Log.Static()` API
- Multiple message overload tests for 2-4 parameter variants
- Edge case tests for null messages, empty strings, and brace escaping

### Fixed

- **Critical**: Fixed null reference exceptions in production builds when settings asset is missing
- Added null-safe fallbacks for `CurrentLogLevel`, `GetPrefix()`, and `GetColor()` with sensible defaults
- Fixed null message handling in `Console()` method
- Fixed test expectations to match actual output format
- Replaced non-deterministic `Random.Range` with deterministic test data
- Enhanced markdown escaping in file logging to prevent injection attacks
- Fixed test naming mismatches (e.g., `Info_ShowsInfoLogs` now correctly tests Info level)

### Security

- Improved `EscapeMarkdown()` to wrap messages in code blocks, preventing markdown injection
- All markdown special characters now safely rendered in log files

### Changed

- Renamed test fixtures for clarity: `ShowOwnLogs`, `ShowHigherPriorityLogs`, `SuppressLowerPriorityLogs`
- Tests now properly clean up GameObjects after each test

### Breaking Changes

- None. This is the first stable release.

### Migration Guide

- **From 0.4.0 or earlier**: No migration required. All changes are backward compatible.

## [0.4.0] - 2025-03-29

### Fixed

- Added assembly definition to Samples
- Fixed string formatting issue when working with char types

## [0.3.0] 24-06-2025

### Complete overhaul

- LogLevel `Debug` is now higher than `Info`. (order: none, error, warning, debug, info, success, verbose)
- Support GameObject pinging in hierarchy
- Supports line number pinging in IDE
- Cleaner code with just 2 overloads: one for 1 string message, another for multiple (up to four)
- Can write logs to `.md` file
- Customize settings in ScriptableObject
- Supports Unity 6
- Build LogLevel now follows the last-set Editor LogLevel

## [0.2.2] 02-04-2025

### Fixed

- Bug where Verbose / Info were mixed up in
-

## [0.2.1]

> ### Package Numbering Change
>
> #### Package will now be numbered starting with 0, to better reflect the current status in development (see the official semver information [here](https://semver.org/#spec-item-4))
>
> If any issues arise when updating from previous (and higher numbered versions), please delete the old version before updating to this version.

### Changed

- Info from Pink to White since pink wasn't working
- Renamed `Info` to `Verbose` since it's the one that's most talkative
- Renamed `Solid` to `Info` since it's more clear

### Updated

- Updated the README.md

### Added

- Roadmap file

## [2.1.0] - 2025-01-31

### Added

- Added samples as a separate thing: download them through the 'Samples' button in the package manager

## [2.0.0] - 2025-01-31

### Changed

- Changed from GNU GPL 3 license to MIT license

## [1.1.0] - 2024-09-25

### Added

- New loglevel called 'Solid', just between Debug and Warning
- Clickable GameObjects from the Console based on [Warped Imagination](https://youtu.be/wykshtqwZSA?si=jMmUvg-NVEAgZzhY)

### Changed

- Updated the README.md

## [1.0.4] - 2024-09-24

### Fixed

- Fixed error where the demo scripts gave warning for not being included into an assembly

### Changed

- Updated the README.md

## [1.0.3] - 2024-09-19

### Changed

- Moved Menu Items to SOSXR/EnhancedLogger/
- Case change in LogButtons.cs

## [1.0.2] - 2024-09-13

### Changed

- Changed SemVer to keep in line with Unity standard
- Removed the URLs to the changelog and licence since they are included here anyway
- Changed licence to be GNU instead of MIT
- Changed namespace from mrstruijk to SOSXR

## [0.0.1] - 2024-01-11

### Added

-

### Fixed

-

### Changed

-

### Removed

-
