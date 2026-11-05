// Service that reads the icon groups of the libraries and keeps them in the cache
using Foldicon.Core.Storage;

namespace Foldicon.Core.Indexing;

public sealed class IconIndexer(
    IGroupIconReader reader,
    IconIndexStore store,
    string windowsDirectory,
    Func<string, bool> fileExists,
    Func<string, LibraryStamp> stampOf)
{
    // Creates the indexer for the real system, with the files of the Windows folder
    public static IconIndexer ForSystem(IconIndexStore store) => new(
        new NativeGroupIconReader(),
        store,
        Environment.GetFolderPath(Environment.SpecialFolder.Windows),
        File.Exists,
        LibraryStamp.Of);

    // Brings the cache up to date, reading again only the libraries whose file changed; returns how many it read
    public int Refresh(IEnumerable<string> libraries)
    {
        var read = 0;
        foreach (var library in libraries)
        {
            // Skips the libraries missing on this edition of Windows
            var path = ResourceFileResolver.Resolve(library, windowsDirectory, fileExists);
            if (!fileExists(path))
            {
                continue;
            }

            // Skips the libraries whose cached index matches the file
            var stamp = stampOf(path);
            if (store.GetStamp(library) == stamp)
            {
                continue;
            }

            // Reads the groups and replaces the cached ones
            var ids = reader.ReadGroupIconIds(path);
            store.ReplaceLibrary(new LibraryIcons(library, stamp, ids));
            read++;
        }

        return read;
    }
}
