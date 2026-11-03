// Functions to read and remove keys of the text of a desktop.ini while keeping the rest of it untouched
namespace Foldicon.Core.Folders;

public static class DesktopIniEditor
{
    // Section of desktop.ini that holds the icon of the folder
    public const string ShellClassInfo = ".ShellClassInfo";

    // Key written by the shell for the icon of the folder
    public const string IconResource = "IconResource";

    // Returns the value of a key of a section, or null when it is missing
    public static string? ReadValue(string text, string section, string key)
    {
        ArgumentNullException.ThrowIfNull(text);
        string? current = null;
        foreach (var line in SplitLines(text))
        {
            // Tracks the section each line belongs to
            if (TrySection(line, out var name))
            {
                current = name;
                continue;
            }

            // Returns the value of the first matching key of the section
            if (string.Equals(current, section, StringComparison.OrdinalIgnoreCase) && TryKey(line, out var lineKey, out var value)
                && string.Equals(lineKey, key, StringComparison.OrdinalIgnoreCase))
            {
                return value;
            }
        }

        return null;
    }

    // Returns the text without the key in the section, and without the section if no key is left in it
    public static string RemoveKey(string text, string section, string key)
    {
        ArgumentNullException.ThrowIfNull(text);
        var newline = text.Contains("\r\n", StringComparison.Ordinal) ? "\r\n" : "\n";
        var output = new List<string>();
        var sectionStart = -1;
        var sectionHasKeys = false;
        string? current = null;

        foreach (var line in SplitLines(text))
        {
            // Closes the previous section when a new one starts
            if (TrySection(line, out var name))
            {
                DropIfEmpty(output, sectionStart, sectionHasKeys);
                current = name;
                sectionStart = string.Equals(name, section, StringComparison.OrdinalIgnoreCase) ? output.Count : -1;
                sectionHasKeys = false;
                output.Add(line);
                continue;
            }

            // Skips the key being removed and notes whether the section keeps any other key
            if (TryKey(line, out var lineKey, out _))
            {
                if (string.Equals(current, section, StringComparison.OrdinalIgnoreCase)
                    && string.Equals(lineKey, key, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                sectionHasKeys = true;
            }

            output.Add(line);
        }

        // Closes the last section and drops the blank lines left at the end
        DropIfEmpty(output, sectionStart, sectionHasKeys);
        while (output.Count > 0 && string.IsNullOrWhiteSpace(output[^1]))
        {
            output.RemoveAt(output.Count - 1);
        }

        return output.Count == 0 ? "" : string.Join(newline, output) + newline;
    }

    // Checks whether the text holds nothing but blank lines
    public static bool IsEmpty(string text) => SplitLines(text).All(string.IsNullOrWhiteSpace);

    // Removes the target section from the output when it was left without keys
    private static void DropIfEmpty(List<string> output, int sectionStart, bool sectionHasKeys)
    {
        if (sectionStart >= 0 && !sectionHasKeys)
        {
            output.RemoveRange(sectionStart, output.Count - sectionStart);
        }
    }

    // Splits the text into lines, whatever its line endings
    private static string[] SplitLines(string text) =>
        text.Replace("\r\n", "\n", StringComparison.Ordinal).TrimEnd('\n').Split('\n');

    // Reads the name of a section header such as "[.ShellClassInfo]"
    private static bool TrySection(string line, out string name)
    {
        var trimmed = line.Trim();
        if (trimmed.Length >= 2 && trimmed[0] == '[' && trimmed[^1] == ']')
        {
            name = trimmed[1..^1].Trim();
            return true;
        }

        name = "";
        return false;
    }

    // Reads the key and the value of a line such as "IconResource=imageres.dll,-3", skipping comments
    private static bool TryKey(string line, out string key, out string value)
    {
        var trimmed = line.TrimStart();
        var equals = trimmed.IndexOf('=', StringComparison.Ordinal);
        if (equals <= 0 || trimmed[0] == ';' || trimmed[0] == '#')
        {
            key = value = "";
            return false;
        }

        key = trimmed[..equals].Trim();
        value = trimmed[(equals + 1)..].Trim();
        return true;
    }
}
