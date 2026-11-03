# Foldicon

A small Windows app that changes the icon of a folder with the icons that ship with Windows. Right-click a folder,
choose **Change folder icon…**, search the icon, and apply it.

> **Status:** early development (version 0.1.0). There are no published releases yet.

## Features

- **Context menu item** for folders, in the modern menu of Windows 11 and in the classic one.
- **Compact picker window** with Mica, search box, one tab per system library and an *All* tab, a virtualized grid of
  icons, and a preview at 16, 24, 32, 48 and 256 px with the name and the `library,-id` reference of the icon.
- **Search** by name in English and Spanish, with synonyms (`carpeta` also finds `folder`), and by library or
  identifier (`shell32 4`).
- **Apply and restore:** *Apply* writes the icon and closes the window; *Restore default* removes it, and the bar that
  confirms it offers *Undo*.
- **Keyboard:** <kbd>Ctrl</kbd>+<kbd>F</kbd> goes to the search box, the arrows move in the grid,
  <kbd>Enter</kbd> or a double click applies, <kbd>Esc</kbd> closes.
- **Icons of** `imageres.dll`, `shell32.dll`, `ddores.dll`, `pifmgr.dll`, `moricons.dll`, `wmploc.dll`,
  `netshell.dll`, `mmcndmgr.dll` and `setupapi.dll`.

Foldicon does only this. It has no favorites, recent icons, sync or list of folders, and it keeps no settings: the only
file it writes besides the `desktop.ini` of the folder you change is a cache of the icon index in
`%LOCALAPPDATA%\Foldicon`, which it rebuilds whenever it is missing or stale. It makes no network connections.

## How it works

The icon is written as `IconResource=%SystemRoot%\System32\<library>,-<id>` in the `desktop.ini` of the folder,
through `SHGetSetFolderCustomSettings`, which keeps every other setting of the file. Restoring removes only that key.
See [docs/architecture.md](docs/architecture.md).

## Platforms

| Platform | Status | Minimum version |
| --- | --- | --- |
| Windows (x64, ARM64) | In development | Windows 10 2004 (build 19041) |

## Building from source

You need Windows, the .NET 10 SDK and Visual Studio 2026 with the *.NET desktop development* and *Desktop development
with C++* workloads (the C++ part builds the context menu extension). Every dependency is free and comes from
nuget.org.

```powershell
# Builds everything, including the C++ extension
& "$env:ProgramFiles\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\MSBuild.exe" Foldicon.slnx -restore -p:Configuration=Debug -p:Platform=x64

# Runs the unit tests of the core
dotnet test tests\Foldicon.Core.Tests -p:Platform=x64
```

To build a local installer, run `scripts\Build-Installer.ps1`: it leaves in `dist\Foldicon-<version>\` a signed MSIX
with an `Install.cmd` that installs it on any PC. See [docs/building.md](docs/building.md).

## Distribution

Foldicon will be published in the **Microsoft Store** with a free individual developer account. The Store signs the
packages it distributes, so no code signing certificate has to be bought; the development certificate of `scripts/`
is only for local testing.

## Contributing

See [CONTRIBUTING.md](CONTRIBUTING.md). Security problems are reported as described in [SECURITY.md](SECURITY.md).

## License

Copyright (c) 2026 Sencimo Labs. Licensed under the [GNU General Public License, version 3](LICENSE.md)
(`GPL-3.0-only`).
