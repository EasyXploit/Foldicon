// Type that identifies a version of a library file, to know when its cached index is stale
namespace Foldicon.Core.Indexing;

public readonly record struct LibraryStamp(long Length, long LastWriteTicks)
{
    // Reads the stamp of a file on disk
    public static LibraryStamp Of(string path)
    {
        var info = new FileInfo(path);
        return new LibraryStamp(info.Length, info.LastWriteTimeUtc.Ticks);
    }
}
