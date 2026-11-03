// Declarations of the shell, user and GDI functions used to turn an icon of a library into pixels
using System.Runtime.InteropServices;

namespace Foldicon.Core.Interop;

internal static partial class NativeIcons
{
    // Format of GetDIBits for colors without a palette
    internal const uint DibRgbColors = 0;

    [StructLayout(LayoutKind.Sequential)]
    internal struct IconInfo
    {
        internal int IsIcon;
        internal int HotspotX;
        internal int HotspotY;
        internal nint MaskBitmap;
        internal nint ColorBitmap;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct BitmapInfoHeader
    {
        internal uint Size;
        internal int Width;
        internal int Height;
        internal ushort Planes;
        internal ushort BitCount;
        internal uint Compression;
        internal uint SizeImage;
        internal int XPelsPerMeter;
        internal int YPelsPerMeter;
        internal uint ColorsUsed;
        internal uint ColorsImportant;
    }

    [LibraryImport("shell32.dll", EntryPoint = "SHDefExtractIconW", StringMarshalling = StringMarshalling.Utf16)]
    internal static partial int SHDefExtractIcon(string iconFile, int index, uint flags, out nint large, nint small, uint size);

    [LibraryImport("user32.dll", EntryPoint = "GetIconInfo")]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static partial bool GetIconInfo(nint icon, out IconInfo info);

    [LibraryImport("user32.dll", EntryPoint = "DestroyIcon")]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static partial bool DestroyIcon(nint icon);

    [LibraryImport("gdi32.dll", EntryPoint = "CreateCompatibleDC")]
    internal static partial nint CreateCompatibleDC(nint dc);

    [LibraryImport("gdi32.dll", EntryPoint = "DeleteDC")]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static partial bool DeleteDC(nint dc);

    [LibraryImport("gdi32.dll", EntryPoint = "DeleteObject")]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static partial bool DeleteObject(nint handle);

    [LibraryImport("gdi32.dll", EntryPoint = "GetDIBits")]
    internal static unsafe partial int GetDIBits(nint dc, nint bitmap, uint start, uint lines, byte* bits, ref BitmapInfoHeader info, uint usage);
}
