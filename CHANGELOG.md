# Changelog

All notable changes to this project will be documented in this file.
The changelog format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/)

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
> #### Package will now be numbered starting with 0, to better reflect the current status in development (see the official semver information [here](https://semver.org/#spec-item-4)).
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
