// Function to locate the files of the app in the local data of the user
namespace Foldicon.Core.Storage;

public static class LocalData
{
    // Returns the regenerable cache of the icon index
    public static string IndexPath() => PathOf("index.db");

    // Returns the settings of the app
    public static string SettingsPath() => PathOf("settings.json");

    // Returns the path of a file in %LOCALAPPDATA%\Foldicon, creating the folder
    private static string PathOf(string name)
    {
        var folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Foldicon");
        Directory.CreateDirectory(folder);
        return Path.Combine(folder, name);
    }
}
