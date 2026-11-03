// Declarations of the functions of shell32.dll used to customize a folder and refresh Explorer
using System.Runtime.InteropServices;

namespace Foldicon.Core.Interop;

internal static partial class Shell32
{
    // Mask and mode of SHGetSetFolderCustomSettings
    internal const uint FcsmIconFile = 0x00000010;
    internal const uint FcsForceWrite = 0x00000002;

    // Event and flags of SHChangeNotify that redraw one item from its path
    internal const int ShcneUpdateItem = 0x00002000;
    internal const uint ShcnfPathW = 0x0005;
    internal const uint ShcnfFlushNoWait = 0x3000;

    [LibraryImport("shell32.dll", EntryPoint = "SHGetSetFolderCustomSettings", StringMarshalling = StringMarshalling.Utf16)]
    internal static partial int SHGetSetFolderCustomSettings(ref ShFolderCustomSettings settings, string path, uint mode);

    [LibraryImport("shell32.dll", EntryPoint = "SHChangeNotify", StringMarshalling = StringMarshalling.Utf16)]
    internal static partial void SHChangeNotify(int eventId, uint flags, string item1, nint item2);
}
