// Rules that keep the search of customized folders away from places where the user keeps no folders of their own
namespace Foldicon.Core.Folders;

public static class FolderScanRules
{
    // Folders of the system, of apps and of development tools, which are large and hold no customized folders
    private static readonly HashSet<string> Skipped = new(StringComparer.OrdinalIgnoreCase)
    {
        "AppData",
        "Windows",
        "Program Files",
        "Program Files (x86)",
        "ProgramData",
        "$Recycle.Bin",
        "System Volume Information",
        "node_modules",
        "bin",
        "obj",
    };

    // Checks whether a folder is left out, also when its name starts with a dot, as in ".git"
    public static bool Skips(string name)
    {
        ArgumentNullException.ThrowIfNull(name);
        return name.StartsWith('.') || Skipped.Contains(name);
    }
}
