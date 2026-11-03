// Interface to the shell functions that change the icon of a folder, so the service can be tested without them
namespace Foldicon.Core.Folders;

public interface IFolderShell
{
    // Writes the icon of the folder through the shell, which merges it into the existing desktop.ini and marks the
    // folder as customized; throws when the shell refuses it
    void SetIcon(string folder, string iconFile, int iconIndex);

    // Tells Explorer to redraw the folder
    void Refresh(string folder);
}
