// Type that holds the file and the index of an IconResource value of desktop.ini
using System.Globalization;

namespace Foldicon.Core.Folders;

public readonly record struct IconResourceValue(string File, int Index)
{
    // Reads a value such as "%SystemRoot%\System32\imageres.dll,-3"; returns null when it has no valid index
    public static IconResourceValue? Parse(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        // Splits at the last comma, since the path itself may hold commas
        var comma = value.LastIndexOf(',');
        if (comma <= 0)
        {
            return null;
        }

        // Reads the index, which is negative for a resource identifier and positive for a position
        var index = value[(comma + 1)..].Trim();
        if (!int.TryParse(index, NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out var number))
        {
            return null;
        }

        return new IconResourceValue(value[..comma].Trim().Trim('"'), number);
    }
}
