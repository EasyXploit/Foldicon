// Function to read an icon of a library as BGRA pixels with straight alpha, at the requested size
using Foldicon.Core.Icons;
using Foldicon.Core.Interop;

namespace Foldicon.Core.Imaging;

public static class IconPixelReader
{
    // Returns size × size pixels in BGRA, or null when the icon cannot be read
    public static byte[]? Read(IconReference icon, int size)
    {
        // Asks the shell for the icon group by its identifier, at the exact size
        var file = Environment.ExpandEnvironmentVariables(icon.LibraryPath());
        if (NativeIcons.SHDefExtractIcon(file, -icon.ResourceId, 0, out var handle, 0, (uint)size) != 0 || handle == 0)
        {
            return null;
        }

        var dc = NativeIcons.CreateCompatibleDC(0);
        NativeIcons.GetIconInfo(handle, out var info);
        try
        {
            // Skips monochrome icons, which have no color bitmap
            if (info.ColorBitmap == 0)
            {
                return null;
            }

            // Reads the colors, and builds the alpha from the mask for old icons without it
            var pixels = ReadBits(dc, info.ColorBitmap, size);
            if (pixels is not null && !IconPixels.HasAlpha(pixels))
            {
                IconPixels.ApplyMask(pixels, ReadBits(dc, info.MaskBitmap, size));
            }

            return pixels;
        }
        finally
        {
            // Frees every handle of the icon
            NativeIcons.DeleteObject(info.ColorBitmap);
            NativeIcons.DeleteObject(info.MaskBitmap);
            NativeIcons.DestroyIcon(handle);
            NativeIcons.DeleteDC(dc);
        }
    }

    // Copies a bitmap into 32-bit top-down pixels
    private static unsafe byte[]? ReadBits(nint dc, nint bitmap, int size)
    {
        if (bitmap == 0)
        {
            return null;
        }

        var header = new NativeIcons.BitmapInfoHeader
        {
            Size = (uint)sizeof(NativeIcons.BitmapInfoHeader),
            Width = size,
            Height = -size,
            Planes = 1,
            BitCount = 32,
        };
        var pixels = new byte[size * size * 4];
        fixed (byte* bits = pixels)
        {
            return NativeIcons.GetDIBits(dc, bitmap, 0, (uint)size, bits, ref header, NativeIcons.DibRgbColors) == size ? pixels : null;
        }
    }
}
