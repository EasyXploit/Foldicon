// Walk of the real folders, which never leaves through a link and keeps the placeholders of sync clients
namespace Foldicon.Core.Folders;

public sealed class FolderTree : IFolderTree
{
    // Skips what cannot be read and the hidden folders, but not the system ones: a customized folder can be one
    private static readonly EnumerationOptions Options = new() { IgnoreInaccessible = true, AttributesToSkip = FileAttributes.Hidden };

    // Lists the folders; a sync client marks its own as reparse points too, so only real links are left out
    public IReadOnlyList<string> Subfolders(string folder) =>
        new DirectoryInfo(folder)
            .EnumerateDirectories("*", Options)
            .Where(directory => !FolderScanRules.Skips(directory.Name))
            .Where(directory => !directory.Attributes.HasFlag(FileAttributes.ReparsePoint) || directory.LinkTarget is null)
            .Select(directory => directory.FullName)
            .ToList();
}
