// Function to read the folder to customize from the command line, as in Foldicon.exe --pick "C:\Photos"
namespace Foldicon.Core.Launch;

public static class LaunchArguments
{
    // Option that names the folder
    public const string PickOption = "--pick";

    // Returns the full path of the folder after --pick, or null when it is missing or empty
    public static string? ReadFolder(IReadOnlyList<string> arguments)
    {
        ArgumentNullException.ThrowIfNull(arguments);
        for (var i = 0; i < arguments.Count - 1; i++)
        {
            if (!string.Equals(arguments[i], PickOption, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            // Drops the quotes and the trailing separators left by the shell, except the one of a drive root
            var folder = arguments[i + 1].Trim().Trim('"');
            if (folder.Length == 0)
            {
                return null;
            }

            var full = Path.GetFullPath(folder);
            var root = Path.GetPathRoot(full);
            return string.Equals(full, root, StringComparison.OrdinalIgnoreCase) ? full : Path.TrimEndingDirectorySeparator(full);
        }

        return null;
    }
}
