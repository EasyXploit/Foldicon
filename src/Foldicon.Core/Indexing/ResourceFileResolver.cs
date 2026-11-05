// Function to find the file that really holds the resources of an icon library
using Foldicon.Core.Icons;

namespace Foldicon.Core.Indexing;

public static class ResourceFileResolver
{
    // Returns the bundled library file, which is next to the app; for a system library, the .mun of
    // SystemResources when it exists, since newer builds of Windows keep the icons there and leave a stub in System32,
    // and the library of System32 otherwise
    public static string Resolve(string library, string windowsDirectory, Func<string, bool> fileExists)
    {
        // Answers for the library that is not part of Windows
        if (library == BundledIconLibrary.Name)
        {
            return BundledIconLibrary.FilePath;
        }

        // Checks the resource-only copy first
        var mun = Path.Combine(windowsDirectory, "SystemResources", library + ".mun");
        if (fileExists(mun))
        {
            return mun;
        }

        // Falls back to the library itself
        return Path.Combine(windowsDirectory, "System32", library);
    }
}
