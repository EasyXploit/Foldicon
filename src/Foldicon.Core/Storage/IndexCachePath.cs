// Function to locate the regenerable cache of the icon index
namespace Foldicon.Core.Storage;

public static class IndexCachePath
{
    // Returns %LOCALAPPDATA%\Foldicon\index.db, creating its folder; the app keeps no other data
    public static string Get()
    {
        var folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Foldicon");
        Directory.CreateDirectory(folder);
        return Path.Combine(folder, "index.db");
    }
}
