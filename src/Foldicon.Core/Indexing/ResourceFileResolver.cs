// Function to find the file that really holds the resources of a system library
namespace Foldicon.Core.Indexing;

public static class ResourceFileResolver
{
    // Returns the .mun of SystemResources when it exists, since newer builds of Windows keep the icons there and
    // leave a stub in System32; otherwise returns the library of System32
    public static string Resolve(string library, string windowsDirectory, Func<string, bool> fileExists)
    {
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
