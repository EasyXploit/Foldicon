// Interface to walk the folders on a disk, so the repair can be tested without one
namespace Foldicon.Core.Folders;

public interface IFolderTree
{
    // Returns the full paths of the folders worth walking inside a folder
    IReadOnlyList<string> Subfolders(string folder);
}
