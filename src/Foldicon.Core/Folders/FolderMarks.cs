// Marks of the real folders and files
namespace Foldicon.Core.Folders;

public sealed class FolderMarks : IFolderMarks
{
    // Read-only or system: Explorer reads the desktop.ini of a folder with either of them
    public bool IsCustomized(string folder) =>
        (File.GetAttributes(folder) & (FileAttributes.ReadOnly | FileAttributes.System)) != 0;

    // Adds the same mark as the shell, and keeps every other attribute of the folder
    public void MarkCustomized(string folder) =>
        File.SetAttributes(folder, File.GetAttributes(folder) | FileAttributes.ReadOnly);

    public bool IsHidden(string folder, string name) =>
        File.GetAttributes(Path.Combine(folder, name)).HasFlag(FolderIconFiles.Hidden);

    public bool Hide(string folder, string name) => FolderIconFiles.Hide(Path.Combine(folder, name));
}
