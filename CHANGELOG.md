# Changelog

The notable changes of each version. The format follows [Keep a Changelog](https://keepachangelog.com/en/1.1.0/) and
versions follow [Semantic Versioning](https://semver.org/).

## [Unreleased]

### Added

- Picker for Windows folders with the icons of nine system libraries and 224 Fluent icons, search in English and
  Spanish, and light and dark themes.
- Item in the context menu of folders, installer for Windows and package for the Microsoft Store.
- Repair of the folders that a sync client brought without their marks, with a notice the first time and a setting to
  turn it off at startup.
- Folder picker for when the app is opened from Start.
- Installers for x64 and ARM64.

### Changed

- Foldicon needs Windows 11, where Mica is available.
- The tab of the bundled icons is named Fluent.
- Screen readers read the name of each icon.
- Uninstalling removes the cache and the settings of the user.

### Fixed

- A very long text in the search box, or an unexpected error of the shell or of an icon, no longer closes the app.
- A long folder path no longer runs under the window buttons.
- A damaged cache is rebuilt instead of leaving the window with an error.
- The repair goes on through the folders inside one that fails, and tells copies apart in any case.
