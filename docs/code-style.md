# Code style

Foldicon follows the same way of programming as the other projects of EasyXploit, adapted to C# and C++.

- **Comments in English before each step**, in the third person and present tense: "Stores the…", "Checks
  whether…", "If the file does not exist". Each logical block is separated by a blank line.
- **One type per file**, grouped in folders by scope (`Indexing/`, `Storage/`, `Search/`, `Folders/`…). Native
  declarations live in `Interop/`.
- **First line of each file:** a comment that says what it holds ("// Function to find the file that really holds the
  resources of a system library").
- **Names in English:** `PascalCase` for types and members and `camelCase` for locals in C#, as in .NET; the same in
  C++, inside the `Foldicon` namespace.
- **4-space indentation** (2 in XML, JSON and YAML). See `.editorconfig`. C# uses file-scoped namespaces.
- **Warnings are errors:** `Directory.Build.props` turns on the recommended .NET analyzers and treats warnings as
  errors; the C++ project builds with `/W4 /WX`.
- **Errors:** each operation that touches the system throws a typed exception, and the entry points of the window catch
  the expected ones, trace them and show a translated message to the user, never the raw text of the system.
- **Interface texts** only in `src/Foldicon.App/Strings/<language>/Resources.resw` (`en-US`, the default, and `es`) and in the
  string table of `Foldicon.ContextMenu.rc`, never written in the code.

## Tests

- Every change of code comes with unit tests of the new or changed logic. Code that talks to the system is kept behind
  an interface (`IGroupIconReader`, `IFolderShell`, `IDesktopIniFile`), so the logic around it is tested with fakes.
- Tests live in `tests/Foldicon.Core.Tests`, in folders that mirror the ones of the library, and use MSTest. The C++ code
  that needs no shell is tested by the console program of `tests/Foldicon.ContextMenu.Tests`.
- Each test is named after what it checks, in snake case (`skips_libraries_whose_file_did_not_change`), not after the
  function.
- The normal case, the edges and the errors are tested.
- No real network. Logic uses in-memory SQLite (`Data Source=:memory:`) and fakes, and the classes behind those
  interfaces are tested for real in `RealSystem/`, inside a `TemporaryFolder` and without opening a window.
- Run only the tests of what you change: `dotnet test tests/Foldicon.Core.Tests -p:Platform=x64 --filter <filter>`.
