// Names of the copies of an icon that a folder keeps next to its desktop.ini
using System.Globalization;

namespace Foldicon.Core.Folders;

public static class FolderIconCopyName
{
    // Start and end of the name; they hold the identifier because Explorer caches an icon by its path and index, so a
    // single name would keep showing the previous icon
    private const string Prefix = ".foldicon-";
    private const string Extension = ".ico";

    // Pattern that finds the copies of a folder
    public const string SearchPattern = Prefix + "*" + Extension;

    // Returns the name of the copy of an icon
    public static string For(int resourceId) => Prefix + resourceId.ToString(CultureInfo.InvariantCulture) + Extension;

    // Checks whether a file named by an IconResource value is one of the copies
    public static bool IsCopy(string file)
    {
        ArgumentNullException.ThrowIfNull(file);
        return file.StartsWith(Prefix, StringComparison.OrdinalIgnoreCase)
            && file.EndsWith(Extension, StringComparison.OrdinalIgnoreCase)
            && file.AsSpan(Prefix.Length, file.Length - Prefix.Length - Extension.Length) is { IsEmpty: false } id
            && int.TryParse(id, NumberStyles.None, CultureInfo.InvariantCulture, out _);
    }
}
