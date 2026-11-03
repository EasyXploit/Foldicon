// Function to read an icon of a system library as premultiplied BGRA pixels, at the requested size
using Foldicon.App.Interop;
using Foldicon.Core.Icons;

namespace Foldicon.App.Imaging;

internal static class IconPixelReader
{
    // Returns size × size pixels in premultiplied BGRA, or null when the icon cannot be read
    public static byte[]? Read(IconReference icon, int size)
    {
        // Asks the shell for the icon group by its identifier, at the exact size
        var file = Environment.ExpandEnvironmentVariables(IconResourceFile(icon));
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
            if (pixels is not null && !HasAlpha(pixels))
            {
                ApplyMask(pixels, ReadBits(dc, info.MaskBitmap, size));
            }

            if (pixels is not null)
            {
                Premultiply(pixels);
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

    // Returns the library part of the IconResource of the icon
    private static string IconResourceFile(IconReference icon)
    {
        var value = icon.ToIconResource();
        return value[..value.LastIndexOf(',')];
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

    // Checks whether any pixel has a value of alpha
    private static bool HasAlpha(byte[] pixels)
    {
        for (var i = 3; i < pixels.Length; i += 4)
        {
            if (pixels[i] != 0)
            {
                return true;
            }
        }

        return false;
    }

    // Makes opaque the pixels where the mask is black, and transparent the rest
    private static void ApplyMask(byte[] pixels, byte[]? mask)
    {
        for (var i = 3; i < pixels.Length; i += 4)
        {
            pixels[i] = mask is null || mask[i - 1] == 0 ? (byte)255 : (byte)0;
        }
    }

    // Multiplies each color by its alpha, as WriteableBitmap expects
    private static void Premultiply(byte[] pixels)
    {
        for (var i = 0; i < pixels.Length; i += 4)
        {
            var alpha = pixels[i + 3];
            pixels[i] = (byte)(pixels[i] * alpha / 255);
            pixels[i + 1] = (byte)(pixels[i + 1] * alpha / 255);
            pixels[i + 2] = (byte)(pixels[i + 2] * alpha / 255);
        }
    }
}
