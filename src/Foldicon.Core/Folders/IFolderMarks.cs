// Interface to the marks that tell Explorer to use a desktop.ini, so the repair can be tested without a disk
namespace Foldicon.Core.Folders;

public interface IFolderMarks
{
    // Checks whether the folder is marked as customized
    bool IsCustomized(string folder);

    // Marks the folder as customized
    void MarkCustomized(string folder);

    // Checks whether a file in the folder is hidden and a system file
    bool IsHidden(string folder, string name);

    // Hides a file in the folder and returns whether it worked
    bool Hide(string folder, string name);
}
