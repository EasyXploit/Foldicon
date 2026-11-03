// Declarations of the functions of kernel32.dll used to read the resources of a library
using System.Runtime.InteropServices;

namespace Foldicon.Core.Interop;

internal static unsafe partial class Kernel32
{
    // Flags of LoadLibraryEx that map the file without running it
    internal const uint LoadLibraryAsDatafile = 0x00000002;
    internal const uint LoadLibraryAsImageResource = 0x00000020;

    // Resource type of the icon groups (MAKEINTRESOURCE(14))
    internal const nint RtGroupIcon = 14;

    [LibraryImport("kernel32.dll", EntryPoint = "LoadLibraryExW", StringMarshalling = StringMarshalling.Utf16, SetLastError = true)]
    internal static partial nint LoadLibraryEx(string fileName, nint file, uint flags);

    [LibraryImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static partial bool FreeLibrary(nint module);

    [LibraryImport("kernel32.dll", EntryPoint = "EnumResourceNamesW", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static partial bool EnumResourceNames(
        nint module,
        nint type,
        delegate* unmanaged<nint, nint, nint, nint, int> callback,
        nint state);
}
