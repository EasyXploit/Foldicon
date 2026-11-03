// Function to load an icon as an image of the interface, reading its pixels off the interface thread
using System.Runtime.InteropServices.WindowsRuntime;
using Foldicon.Core.Icons;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;

namespace Foldicon.App.Imaging;

internal static class IconImageLoader
{
    // Returns the icon at the size in physical pixels, or null when it cannot be read
    public static async Task<ImageSource?> LoadAsync(IconReference icon, int size)
    {
        // Reads the pixels in the background
        var pixels = await Task.Run(() => IconPixelReader.Read(icon, size));
        if (pixels is null)
        {
            return null;
        }

        // Builds the bitmap back on the interface thread
        var bitmap = new WriteableBitmap(size, size);
        using (var stream = bitmap.PixelBuffer.AsStream())
        {
            await stream.WriteAsync(pixels);
        }

        bitmap.Invalidate();
        return bitmap;
    }
}
