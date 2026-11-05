// Native libraries are only loaded from System32, never from a folder an attacker could write to
using System.Runtime.InteropServices;

[assembly: DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
