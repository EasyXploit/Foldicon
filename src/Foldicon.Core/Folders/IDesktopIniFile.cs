// Interface to the desktop.ini file of a folder, so the service can be tested without a disk
namespace Foldicon.Core.Folders;

public interface IDesktopIniFile
{
    // Returns the text of the desktop.ini of the folder, or null when the folder has none
    string? Read(string folder);

    // Replaces the text of the desktop.ini, keeping its encoding and attributes
    void Write(string folder, string text);

    // Deletes the desktop.ini and the marks that made the folder customized
    void Delete(string folder);
}
