// Access to the real desktop.ini of a folder, which is usually hidden, a system file and UTF-16
using System.Text;

namespace Foldicon.Core.Folders;

public sealed class DesktopIniFile : IDesktopIniFile
{
    // Name of the file inside the folder
    public const string FileName = "desktop.ini";

    // Reads the file with the encoding of its byte order mark
    public string? Read(string folder)
    {
        var path = Path.Combine(folder, FileName);
        return File.Exists(path) ? File.ReadAllText(path, DetectEncoding(path)) : null;
    }

    // Truncates and rewrites the file, since creating it again fails on a hidden file
    public void Write(string folder, string text)
    {
        var path = Path.Combine(folder, FileName);

        // Keeps UTF-16 when the file was UTF-16, as the shell writes it
        var encoding = DetectEncoding(path);
        using var stream = new FileStream(path, FileMode.Truncate, FileAccess.Write, FileShare.None);
        using var writer = new StreamWriter(stream, encoding);
        writer.Write(text);
    }

    // Deletes the file and drops the read-only and system marks that told Explorer to read it
    public void Delete(string folder)
    {
        MarkedFile.Delete(Path.Combine(folder, FileName));

        // Keeps the folder's other attributes
        var attributes = File.GetAttributes(folder);
        File.SetAttributes(folder, attributes & ~(FileAttributes.ReadOnly | FileAttributes.System));
    }

    // Returns the encoding of the byte order mark; without one, Latin-1 reads and writes back every byte unchanged,
    // whatever code page the file was written with
    private static Encoding DetectEncoding(string path)
    {
        using var stream = File.OpenRead(path);
        var first = stream.ReadByte();
        var second = stream.ReadByte();
        if (first == 0xFF && second == 0xFE)
        {
            return Encoding.Unicode;
        }

        if (first == 0xEF && second == 0xBB)
        {
            return new UTF8Encoding(true);
        }

        return Encoding.Latin1;
    }
}
