# Architecture

Foldicon has four parts, shipped together by the installer or, for the Store, in one MSIX:

| Part | Folder | What it does |
| --- | --- | --- |
| Core | `src/Foldicon.Core` | Icon index, search and folder changes, without any interface. Covered by `tests/`. |
| App | `src/Foldicon.App` | WinUI 3 window of the picker. Started as `Foldicon.exe --pick "<folder>"`. |
| Context menu | `src/Foldicon.ContextMenu` | C++/WinRT `IExplorerCommand` that only starts the app. |
| Fluent icons | `src/Foldicon.FluentIcons` | `fluenticons.dll`, a library with only icon resources and no code. |

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

## Fluent icons

The only icons that are not part of Windows are the 224 of `fluenticons.dll`, taken from
[Fluent UI System Icons](https://github.com/microsoft/fluentui-system-icons). A library of icon groups, instead of loose
`.ico` files, keeps them on the same path as the system icons: `IconReference`, the index, the catalog, the search, the
tabs and `IconResource` work the same way, and `BundledIconLibrary` only says that the file is next to the app and not
in `System32`. The library has no entry point and no code, so nothing runs when the shell loads it.

`src/Foldicon.FluentIcons/pack.json` is the list of the pack: the commit of the source and, for each icon, its identifier,
its asset and its style. `scripts/Render-FluentIcons.ps1` turns it into the `.ico` files of `icons/`, with frames of 16,
24, 32, 48, 64 and 256 px, and into the resource script. Each frame comes from the drawing made for that size, or for
the closest larger one, because the small drawings are adjusted to their grid of pixels. The folders of the source are
drawn in one dark tone that a dark Explorer would hide, so the script paints them in gold.

Identifiers end up in the `desktop.ini` of the folders of the users, so an icon keeps its identifier forever, and a
removed one leaves a gap. Folders are numbered from 1 and the color icons from 101, so the tab opens with the folders.

## Search

The text of the search box is split into words, lowercased and stripped of accents (`TextNormalizer`). Each word is
expanded with the groups of `Search/synonyms.json` that have a word starting with it (`SynonymTable`), so a word being
typed already finds its translation. An icon matches when, for every typed word, one of its names, its library or its
identifier starts with one of the expansions.

While nothing is being searched, the *All* tab starts with the folders of `fluenticons.dll` and `imageres.dll`, follows
with each library in the order of the tabs (`IconLibraryTabs`) and leaves the repeated and blank icons at the end
(`IconBrowseOrder`). There is no ranking by relevance: a search and the tab of a library list the matches by library and
identifier, so the Fluent icons come first.

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
  `desktop.ini` and marks the folder as customized. A Fluent icon first gets its copy in the folder, as described below,
  and any copy the folder no longer uses is deleted.
- **Restore** removes `IconResource` from `[.ShellClassInfo]`, drops the section if nothing is left in it, and deletes
  the file, with the read-only and system marks of the folder, if the file is left empty, and then deletes the copies of
  the icon. The file keeps its encoding (UTF-16 as the shell writes it, or its bytes unchanged otherwise).
- **Undo** writes the previous value again, with the copy that *Restore* kept in memory, or restores the default icon
  when there was none. The window offers it after *Restore default*; *Apply* closes the window as soon as the icon is
  written.

Each change ends with `SHChangeNotify(SHCNE_UPDATEITEM)` so Explorer redraws the folder.

### Copies of the Fluent icons

A folder must not depend on the path of the app: it would lose its icon when Foldicon is uninstalled or the folder goes
to another PC, and the path of an MSIX holds its version, so an update would break it. So `NativeBundledIconSource`
rebuilds the `.ico` file from the resources of `fluenticons.dll` (`IconFileBuilder`), `FolderIconFiles` writes it in the
folder, and `IconResource` holds only its name, `.foldicon-<id>.ico,0`, which the shell resolves from the folder.

- The name holds the identifier because Explorer caches an icon by its path and index, so a single name would keep
  showing the previous icon. It starts with a dot so it also stays hidden on a NAS or on macOS, where the hidden and
  system marks of Windows may not survive a sync.
- When the copy cannot be written, in a read-only folder for example, the change stops before touching `desktop.ini`,
  and the window shows the usual error. When the shell refuses the icon, a copy that was new is removed.
- A drive that refuses the marks, as a network drive can, keeps the copy visible, since the icon still works.

### Repair of synced folders

A sync client such as Synology Drive carries the files of a folder but may drop the read-only mark of the folder and the
hidden and system marks of its files, and Explorer then ignores `desktop.ini`. `FolderIconRepair` puts them back. A
folder is only touched when it holds a copy: without a `desktop.ini`, a single copy says which icon to write again, and a
folder that points to another icon stays as it is.

There is no list of folders, which would be empty on a second PC anyway: every start of the app walks the folder of the
user in the background, at low priority, breadth first and with the limits of `FolderScanLimits` (depth 6, 20,000
folders). *Repair all icons*, in the **···** button of the window, also walks every fixed drive with far wider limits.
Both skip links, hidden folders and the folders of `FolderScanRules`, but not the placeholders of sync clients, which
are reparse points too, and enumerating them does not download anything. The walk checks each folder with one listing
of `.foldicon-*.ico`, and reads no file of a folder that has none.

## Still missing

- **Tests of the C++ command:** the quoting of `BuildPickArguments` mirrors `LaunchArguments`, which is tested, but the
  C++ project has no test project of its own yet.
- **Folders customized with `IconFile` and `IconIndex`:** *Restore* removes only `IconResource`, as intended, so such a
  folder keeps showing its old icon.
