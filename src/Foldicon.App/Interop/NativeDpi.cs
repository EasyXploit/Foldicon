// Declaration of the user function that reads the DPI of the monitor of a window
using System.Runtime.InteropServices;

namespace Foldicon.App.Interop;

internal static partial class NativeDpi
{
    [LibraryImport("user32.dll", EntryPoint = "GetDpiForWindow")]
    internal static partial uint GetDpiForWindow(nint window);
}
