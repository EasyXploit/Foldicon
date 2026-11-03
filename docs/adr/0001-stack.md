# 0001 · Stack: WinUI 3 in C#, a C++/WinRT context menu command and an MSIX package

- **Date:** 3 Nov 2026 · **Status:** accepted

**Context:** the app only runs on Windows, must look like Windows 11 (Mica, Fluent controls), start fast from the
context menu of Explorer, and be published in the Microsoft Store at no cost.

**Decision:** WinUI 3 (Windows App SDK) on .NET 10 for the window, with the logic in a separate class library that is
tested without any interface; an `IExplorerCommand` in C++/WinRT for the context menu, since Explorer loads it through
COM and a .NET COM server would bring the whole runtime into the surrogate process; one MSIX package with both, which
gives the package identity the modern context menu of Windows 11 requires. C++/WinRT comes from the headers of the
Windows SDK, so the C++ project needs no package. The cache is SQLite through `Microsoft.Data.Sqlite`.

**Ruled out:** WPF and Windows Forms (no Mica or Fluent controls without third-party libraries), a shell extension
registered in the registry (not allowed in the Store and absent from the modern menu), and a JSON file as the index
(the search with synonyms is simpler in SQL).
