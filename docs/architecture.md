# Architecture

Foldicon has three parts, shipped together by the installer or, for the Store, in one MSIX:

| Part | Folder | What it does |
| --- | --- | --- |
| Core | `src/Foldicon.Core` | Icon index, search and folder changes, without any interface. Covered by `tests/`. |
| App | `src/Foldicon.App` | WinUI 3 window of the picker. Started as `Foldicon.exe --pick "<folder>"`. |
| Context menu | `src/Foldicon.ContextMenu` | C++/WinRT `IExplorerCommand` that only starts the app. |

## Context menu

A package declares the command with `desktop4:FileExplorerContextMenus` for the `Directory` item type, and its COM
class with a `com:SurrogateServer`, so the DLL runs in a surrogate process and never inside Explorer. The command:

- shows itself only for exactly one folder of the file system (`GetState`);
- takes its text from the string table of its own resources, in English and Spanish;
- starts the app with the arguments `--pick "<folder>"`, doubling the backslashes before the closing quote so `C:\` is
  not read as an escaped quote. Inside a full MSIX it activates the app with `IApplicationActivationManager`; the
  installer has no manifest next to the DLL, so it runs `Foldicon.exe` from the same folder.

The installer does not use the MSIX: it copies the files to Program Files and registers a package with external
location (`packaging/windows/sparse`), which holds no code and points the same declarations to that folder.

## Languages

The app, the context menu and the installer are in Spanish when the language of the interface of the system is any
variant of Spanish, and in English otherwise. The app sets it from `AppLanguage`, whose default strings are in
`Strings/en-US`; the command of the context menu chooses between its two strings, and the installer uses
`LanguageDetectionMethod=uilanguage` with English as the first language.

## Icon index

1. `ResourceFileResolver` picks `C:\Windows\SystemResources\<library>.mun` when it exists, since recent builds of
   Windows keep the icons there and leave a stub in `System32`, and the library of `System32` otherwise.
2. `NativeGroupIconReader` maps the file with `LoadLibraryEx(LOAD_LIBRARY_AS_DATAFILE | LOAD_LIBRARY_AS_IMAGE_RESOURCE)`
   and lists its `RT_GROUP_ICON` resources with integer identifiers. Groups named with a string are skipped, because
   `IconResource` could only point to them by position.
3. `IconIndexer` reads again only the libraries whose size or write time changed, and `IconIndexStore` keeps the result
   in SQLite at `%LOCALAPPDATA%\Foldicon\index.db`. The file is a cache: a different schema version drops it, and
   deleting it only makes the next start slower.

On a build of Windows 11 from 2026 the nine libraries hold about 1,500 icon groups.

## Search

The text of the search box is split into words, lowercased and stripped of accents (`TextNormalizer`). Each word is
expanded with the groups of `Search/synonyms.json` that have a word starting with it (`SynonymTable`), so a word being
typed already finds its translation. An icon matches when, for every typed word, one of its names, its library or its
identifier starts with one of the expansions.

While nothing is being searched, the *All* tab starts with the folders of `imageres.dll`, follows with each library in
the order of the tabs and leaves the repeated and blank icons at the end (`IconBrowseOrder`). There is no ranking by
relevance: a search and the tab of a library list the matches by library and identifier.

The names of the icons come from `Search/icon-names.json`, loaded into the cache on every start. Each icon has its
English name, its Spanish name and a few keywords in both languages:
`{ "imageres.dll": { "108": ["Music folder", "Carpeta de música", "audio", "song", "canción"] } }`. The window shows
the name of the language of the interface, and the search uses the words of both names and of the keywords. Icons
whose pixels at 48 px are exactly the same as an earlier one also carry the keywords `duplicate` and `duplicado`.

The catalog was written by looking at every icon of a build of Windows 11 from 2026 with `tools/IconExport`, which
exports each icon to PNG, lists the identical ones in `duplicates.txt` and draws contact sheets with the identifier
under each icon (`dotnet run --project tools/IconExport -p:Platform=x64`; its output in `tools/IconExport/out` is not
versioned). Icons added by later builds of Windows are shown with their identifier until the catalog names them.

## Folder changes

`FolderIconService` works through two interfaces, so it is tested without the shell or the disk:

- **Apply** reads the current `IconResource`, which it returns, and calls `SHGetSetFolderCustomSettings` with
  `FCSM_ICONFILE`, `%SystemRoot%\System32\<library>` and `-<id>`. The shell merges the key into the existing
  `desktop.ini` and marks the folder as customized.
- **Restore** removes only `IconResource` from `[.ShellClassInfo]`, drops the section if nothing is left in it, and
  deletes the file, with the read-only and system marks of the folder, if the file is left empty. The file keeps its
  encoding (UTF-16 as the shell writes it, or its bytes unchanged otherwise).
- **Undo** writes the previous value again, or restores the default icon when there was none. The window offers it
  after *Restore default*; *Apply* closes the window as soon as the icon is written.

Each change ends with `SHChangeNotify(SHCNE_UPDATEITEM)` so Explorer redraws the folder.

## Still missing

- **Check on an installed copy:** the window and the folder changes were tried on the real system, and the shell writes
  `IconResource` with `%SystemRoot%` unexpanded and keeps the other keys, but the setup and the item of the context menu
  have not been tried installed yet.
- **Tests of the C++ command:** the quoting of `BuildPickArguments` mirrors `LaunchArguments`, which is tested, but the
  C++ project has no test project of its own yet.
- **Folders customized with `IconFile` and `IconIndex`:** *Restore* removes only `IconResource`, as intended, so such a
  folder keeps showing its old icon.
