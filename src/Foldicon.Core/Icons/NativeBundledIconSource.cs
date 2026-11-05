// Source that reads the icons of the bundled library as resources, without running any of its code
using System.Runtime.InteropServices;
using Foldicon.Core.Interop;

namespace Foldicon.Core.Icons;

public sealed class NativeBundledIconSource : IBundledIconSource
{
    // Rebuilds the .ico file from the group and its images
    public byte[]? ReadIconFile(int resourceId)
    {
        // Loads the file only as a source of resources
        var module = Kernel32.LoadLibraryEx(BundledIconLibrary.FilePath, 0, Kernel32.LoadLibraryAsDatafile | Kernel32.LoadLibraryAsImageResource);
        if (module == 0)
        {
            return null;
        }

        try
        {
            var group = ReadResource(module, Kernel32.RtGroupIcon, resourceId);
            return group is null ? null : IconFileBuilder.Build(group, id => ReadResource(module, Kernel32.RtIcon, id));
        }
        finally
        {
            Kernel32.FreeLibrary(module);
        }
    }

    // Copies a resource out of the mapped file, or returns null when it does not exist
    private static byte[]? ReadResource(nint module, nint type, int id)
    {
        var resource = Kernel32.FindResource(module, id, type);
        var handle = resource == 0 ? 0 : Kernel32.LoadResource(module, resource);
        var data = handle == 0 ? 0 : Kernel32.LockResource(handle);
        if (data == 0)
        {
            return null;
        }

        var bytes = new byte[Kernel32.SizeofResource(module, resource)];
        Marshal.Copy(data, bytes, 0, bytes.Length);
        return bytes;
    }
}
