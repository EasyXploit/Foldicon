// Library mapped only as a source of resources, so none of its code runs
namespace Foldicon.Core.Interop;

internal sealed class ResourceModule : IDisposable
{
    private ResourceModule(nint handle) => Handle = handle;

    public nint Handle { get; }

    // Maps the file, or returns null when it cannot be read
    public static ResourceModule? Open(string path)
    {
        var handle = Kernel32.LoadLibraryEx(path, 0, Kernel32.LoadLibraryAsDatafile | Kernel32.LoadLibraryAsImageResource);
        return handle == 0 ? null : new ResourceModule(handle);
    }

    // Unmaps the file
    public void Dispose() => Kernel32.FreeLibrary(Handle);
}
