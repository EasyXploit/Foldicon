# Architecture

Foldicon has three parts, packaged together in one MSIX:

| Part | Folder | What it does |
| --- | --- | --- |
| Core | `src/Foldicon.Core` | Icon index, search and folder changes, without any interface. Covered by `tests/`. |
| App | `src/Foldicon.App` | WinUI 3 window of the picker. Started as `Foldicon.exe --pick "<folder>"`. |
| Context menu | `src/Foldicon.ContextMenu` | C++/WinRT `IExplorerCommand` that only starts the app. |

## Context menu

The package declares the command with `desktop4:FileExplorerContextMenus` for the `Directory` item type, and its COM
class with a `com:SurrogateServer`, so the DLL runs in a surrogate process and never inside Explorer. The command:

- shows itself only for exactly one folder of the file system (`GetState`);
- takes its text from the string table of its own resources, in English and Spanish;
- activates the app of its package with `IApplicationActivationManager` and the arguments
  `--pick "<folder>"`, doubling the backslashes before the closing quote so `C:\` is not read as an escaped quote.

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

The names of the icons come from `Search/icon-names.json` (`{ "imageres.dll": { "3": ["folder", "carpeta"] } }`),
loaded into the cache on every start.

## Folder changes

`FolderIconService` works through two interfaces, so it is tested without the shell or the disk:

- **Apply** reads the current `IconResource` (kept for *Undo*) and calls `SHGetSetFolderCustomSettings` with
  `FCSM_ICONFILE`, `%SystemRoot%\System32\<library>` and `-<id>`. The shell merges the key into the existing
  `desktop.ini` and marks the folder as customized.
- **Restore** removes only `IconResource` from `[.ShellClassInfo]`, drops the section if nothing is left in it, and
  deletes the file, with the read-only and system marks of the folder, if the file is left empty. The file keeps its
  encoding (UTF-16 as the shell writes it, or its bytes unchanged otherwise).
- **Undo** writes the previous value again, or restores the default icon when there was none.

Each change ends with `SHChangeNotify(SHCNE_UPDATEITEM)` so Explorer redraws the folder.

## Still missing

- **Names of the icons:** `icon-names.json` is empty, so the grid names each icon *Icon <id>* and the search only
  finds icons by library and identifier until the catalog is filled in. The synonyms are ready.
- **Check on a real folder:** the package builds and its manifest validates, but applying an icon from the context
  menu has not been tried on an installed package yet. In particular, confirm that the shell writes `IconResource`
  (not `IconFile` and `IconIndex`) and keeps `%SystemRoot%` unexpanded.
- **Tests of the C++ command:** the quoting of `BuildPickArguments` mirrors `LaunchArguments`, which is tested, but the
  C++ project has no test project of its own yet.
- **Folders customized with `IconFile` and `IconIndex`:** *Restore* removes only `IconResource`, as intended, so such a
  folder keeps showing its old icon.
- **Real logos:** the images of `Assets/` are placeholders.
