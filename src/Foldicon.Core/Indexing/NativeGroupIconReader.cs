// Reader of icon groups that loads the library as an image resource, without running any of its code
using System.Runtime.InteropServices;
using Foldicon.Core.Interop;

namespace Foldicon.Core.Indexing;

public sealed class NativeGroupIconReader : IGroupIconReader
{
    // Lists the integer identifiers of the RT_GROUP_ICON resources of the file
    public IReadOnlyList<int> ReadGroupIconIds(string path)
    {
        using var module = ResourceModule.Open(path);
        if (module is null)
        {
            return [];
        }

        // Collects the identifiers through the callback; groups named with a string are skipped because IconResource
        // can only point to them by position, which changes between builds of Windows
        var ids = new List<int>();
        var handle = GCHandle.Alloc(ids);
        try
        {
            unsafe
            {
                Kernel32.EnumResourceNames(module.Handle, Kernel32.RtGroupIcon, &Collect, GCHandle.ToIntPtr(handle));
            }
        }
        finally
        {
            handle.Free();
        }

        // Returns them in order, so the grid shows them as the file holds them
        ids.Sort();
        return ids;
    }

    // Callback of EnumResourceNames that stores each integer identifier
    [UnmanagedCallersOnly]
    private static int Collect(nint module, nint type, nint name, nint state)
    {
        // Checks whether the name is an integer resource (IS_INTRESOURCE)
        if ((name >> 16) == 0)
        {
            var ids = (List<int>)GCHandle.FromIntPtr(state).Target!;
            ids.Add((int)name);
        }

        // Keeps enumerating
        return 1;
    }
}
