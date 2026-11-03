// Function to write each icon as a 48 px PNG with straight alpha
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using Foldicon.Core.Icons;
using Foldicon.Core.Imaging;

namespace Foldicon.Tools.IconExport;

internal static class IconPngWriter
{
    // Size of the exported images
    public const int Size = 48;

    // Writes the icons that can be read and returns them with their pixels
    public static List<(IconReference Icon, byte[] Pixels)> WriteAll(IEnumerable<IconReference> icons, string folder)
    {
        Directory.CreateDirectory(folder);
        var exported = new List<(IconReference, byte[])>();
        foreach (var icon in icons)
        {
            // Skips the icons the shell cannot draw
            var pixels = IconPixelReader.Read(icon, Size);
            if (pixels is null)
            {
                continue;
            }

            using var bitmap = ToBitmap(pixels, Size);
            bitmap.Save(Path.Combine(folder, $"{icon.ResourceId}.png"), ImageFormat.Png);
            exported.Add((icon, pixels));
        }

        return exported;
    }

    // Copies BGRA pixels with straight alpha into a bitmap, whose memory layout is the same
    public static Bitmap ToBitmap(byte[] pixels, int size)
    {
        var bitmap = new Bitmap(size, size, PixelFormat.Format32bppArgb);
        var data = bitmap.LockBits(new Rectangle(0, 0, size, size), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
        for (var row = 0; row < size; row++)
        {
            Marshal.Copy(pixels, row * size * 4, data.Scan0 + row * data.Stride, size * 4);
        }

        bitmap.UnlockBits(data);
        return bitmap;
    }
}
