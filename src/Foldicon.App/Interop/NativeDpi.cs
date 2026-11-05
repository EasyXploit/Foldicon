// Declaration of the user32 function that reads the DPI of a window's monitor
using System.Runtime.InteropServices;

namespace Foldicon.App.Interop;

internal static partial class NativeDpi
{
    [LibraryImport("user32.dll", EntryPoint = "GetDpiForWindow")]
    [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    internal static partial uint GetDpiForWindow(nint window);
}
