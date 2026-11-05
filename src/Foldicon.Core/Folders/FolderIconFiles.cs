// Access to the real icon copies inside a folder, hidden and marked as system files like desktop.ini
using System.Diagnostics;

namespace Foldicon.Core.Folders;

public sealed class FolderIconFiles : IFolderIconFiles
{
    // Marks of a copy, which keep it out of the folder listing
    internal const FileAttributes Hidden = FileAttributes.Hidden | FileAttributes.System;

    // Lists the copies, whose marks do not keep them out of this search
    public IReadOnlyList<string> List(string folder) =>
        new DirectoryInfo(folder)
            .EnumerateFiles(FolderIconCopyName.SearchPattern, new EnumerationOptions { AttributesToSkip = 0 })
            .Select(file => file.Name)
            .Where(FolderIconCopyName.IsCopy)
            .ToList();

    // Reads a copy
    public byte[]? Read(string folder, string name)
    {
        var path = Path.Combine(folder, name);
        return File.Exists(path) ? File.ReadAllBytes(path) : null;
    }

    // Writes the file without its marks, since creating it again fails on a hidden file
    public void Write(string folder, string name, byte[] icon)
    {
        var path = Path.Combine(folder, name);
        MarkedFile.ClearMarks(path);
        File.WriteAllBytes(path, icon);
        Hide(path);
    }

    // Puts back the marks of a copy and returns whether it worked; it stays visible when a network drive refuses them,
    // since the icon still works
    public static bool Hide(string path)
    {
        try
        {
            File.SetAttributes(path, Hidden);
            return true;
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
        {
            Trace.TraceWarning($"The copy of the icon could not be hidden: {exception.Message}");
            return false;
        }
    }

    // Deletes the file after dropping its marks
    public void Delete(string folder, string name) => MarkedFile.Delete(Path.Combine(folder, name));
}
