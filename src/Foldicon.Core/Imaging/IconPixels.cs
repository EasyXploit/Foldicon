// Functions that fix the alpha of the BGRA pixels read from an icon
namespace Foldicon.Core.Imaging;

public static class IconPixels
{
    // Checks whether any pixel has a value of alpha
    public static bool HasAlpha(byte[] pixels)
    {
        ArgumentNullException.ThrowIfNull(pixels);
        for (var i = 3; i < pixels.Length; i += 4)
        {
            if (pixels[i] != 0)
            {
                return true;
            }
        }

        return false;
    }

    // Makes opaque the pixels where the mask is black, and transparent the rest; without a mask, all of them opaque
    public static void ApplyMask(byte[] pixels, byte[]? mask)
    {
        ArgumentNullException.ThrowIfNull(pixels);
        for (var i = 3; i < pixels.Length; i += 4)
        {
            pixels[i] = mask is null || mask[i - 1] == 0 ? (byte)255 : (byte)0;
        }
    }

    // Multiplies each color by its alpha, as the bitmaps of WinUI expect
    public static void Premultiply(byte[] pixels)
    {
        ArgumentNullException.ThrowIfNull(pixels);
        for (var i = 0; i < pixels.Length; i += 4)
        {
            var alpha = pixels[i + 3];
            pixels[i] = (byte)(pixels[i] * alpha / 255);
            pixels[i + 1] = (byte)(pixels[i + 1] * alpha / 255);
            pixels[i + 2] = (byte)(pixels[i + 2] * alpha / 255);
        }
    }
}
