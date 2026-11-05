// Function to load an icon as an image of the interface, reading its pixels off the interface thread
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.WindowsRuntime;
using Foldicon.Core.Icons;
using Foldicon.Core.Imaging;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;

namespace Foldicon.App.Imaging;

internal static class IconImageLoader
{
    // Returns the icon at the size in physical pixels, or null when it cannot be read, so its cell stays empty
    public static async Task<ImageSource?> LoadAsync(IconReference icon, int size)
    {
        try
        {
            return await ReadAsync(icon, size);
        }
        catch (Exception exception) when (exception is COMException or IOException or ArgumentException or InvalidOperationException)
        {
            Trace.TraceWarning($"The icon {icon} could not be loaded: {exception.Message}");
            return null;
        }
    }

    // Reads the pixels of the icon and builds its image
    private static async Task<ImageSource?> ReadAsync(IconReference icon, int size)
    {
        // Reads the pixels in the background, premultiplied as WriteableBitmap expects
        var pixels = await Task.Run(() =>
        {
            var read = IconPixelReader.Read(icon, size);
            if (read is not null)
            {
                IconPixels.Premultiply(read);
            }

            return read;
        });
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
