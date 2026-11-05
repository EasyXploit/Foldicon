// Functions for files with the hidden and system marks, which keep them from being replaced or deleted
namespace Foldicon.Core.Folders;

internal static class MarkedFile
{
    // Drops the file's marks, when it exists, so it can be replaced
    public static void ClearMarks(string path)
    {
        if (File.Exists(path))
        {
            File.SetAttributes(path, FileAttributes.Normal);
        }
    }

    // Deletes the file, when it exists, after dropping its marks
    public static void Delete(string path)
    {
        ClearMarks(path);
        if (File.Exists(path))
        {
            File.Delete(path);
        }
    }
}
