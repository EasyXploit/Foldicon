// Interface to the copies of an icon kept inside a folder, so the service can be tested without a disk
namespace Foldicon.Core.Folders;

public interface IFolderIconFiles
{
    // Returns the names of the copies the folder holds
    IReadOnlyList<string> List(string folder);

    // Returns the bytes of a copy, or null when the folder does not hold it
    byte[]? Read(string folder, string name);

    // Writes a copy, replacing the one with the same name, and hides it
    void Write(string folder, string name, byte[] icon);

    // Deletes a copy
    void Delete(string folder, string name);
}
