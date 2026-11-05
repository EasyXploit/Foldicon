# Engineering guidelines

Conclusions of the review of November 2026. Read this before changing the code: every rule comes from a problem that was
found and fixed, and new work has to keep them. [code-style.md](code-style.md) covers the format of the code; this file
covers the decisions behind it.

## Behaviour

- A refactor changes no behaviour and no pixel of the window. A change of behaviour is a decision of the maintainer, and
  it goes in its own commit, with its own entry in [CHANGELOG.md](../CHANGELOG.md).
- A folder is user data. Nothing writes in a folder unless the user chose it, or it already carries a copy of an icon of
  Foldicon. Every action that writes in a folder is described in [SECURITY.md](../SECURITY.md) and in the README.
- Identifiers of `pack.json`, the name `.foldicon-<id>.ico` and the format of `IconResource` live in the folders of the
  users, so they never change.

## Structure

- The window only draws and forwards events. A decision that can be taken without a control belongs in `Foldicon.Core`,
  with its tests: `IconBrowser` answers the search, `IconIndexer.RefreshAll` prepares the index and
  `FolderIconRepair.RepairAllInBackground` runs the repair.
- Each fact lives in one place: a name of a file, a flag of a native call or a step that repeats. Examples are
  `DesktopIniFile.FileName`, `ResourceModule`, `MarkedFile`, `IconReference.IsBundled`, `scripts/Common.ps1` and
  `ModulePath` in C++. Look for the existing helper before writing a second copy.
- Data is passed as typed values, not written into a string and parsed back.
- Native resources are owned by a type with `Dispose`, or by RAII in C++, instead of a `try`/`finally` at every use.
- A list that is looked up inside a sort is a dictionary built once.

## Security

- Everything read from a folder is untrusted, because a sync client or a download can bring it. A name taken from a
  `desktop.ini` is only used once `FolderIconCopyName.IsCopy` accepts it, and the walk never follows a link.
- Libraries are mapped with `LoadLibraryEx` as image resources only, so their code never runs. `DllSearch.cs` limits the
  native imports to System32.
- Connection strings are built with `SqliteConnectionStringBuilder`, and SQL only takes values through parameters.
- The installer starts every tool by its full path, since it can run from the Downloads folder next to anything.
- The private key of a certificate is created as not exportable and never leaves the store.
- Workflows run with read-only permissions, and their actions are pinned to a commit.
- There are no network connections, telemetry or third-party services in the app. Foldicon holds no credentials and has
  no cryptography of its own; if that ever changes, it uses a maintained library, never code of its own.

## Dependencies

- Only well-known, maintained packages, versioned in `Directory.Packages.props`. A new one needs a reason, and a line in
  the credits of the README when it ships with the app.
- Before a release, check them with
  `dotnet list src/Foldicon.App package --vulnerable --include-transitive`.

## Tests

- Every change of code comes with its tests. The goal is every line of `Foldicon.Core`.
- Fakes are for logic. The classes that talk to the disk or the shell are tested for real in
  `tests/Foldicon.Core.Tests/RealSystem`, always inside a `TemporaryFolder`, and never open a window.
- C++ logic that needs no shell lives apart from the COM code and is tested in `tests/Foldicon.ContextMenu.Tests`.

## Writing

- The repository is in English; Spanish only exists in the strings of the app, the installer and the catalog of icons.
- Comments say why something is done or what a block is for, in plain English. Write "folder icons", not "the icons of
  the folders", and do not repeat the code in words.
- Commit messages are one short sentence in the imperative mood. One change, one commit.
- The documentation says what the code does today. When a feature changes what the app writes, what it touches or how
  it is installed, update the README, `docs/architecture.md` and `SECURITY.md` in the same commit.
- Personal data stays out of the history: commits use the `noreply` address of GitHub.
