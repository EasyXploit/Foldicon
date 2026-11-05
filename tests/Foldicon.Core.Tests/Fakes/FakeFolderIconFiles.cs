// Copies of an icon kept in memory, which can refuse to be written
using Foldicon.Core.Folders;

namespace Foldicon.Core.Tests.Fakes;

internal sealed class FakeFolderIconFiles : IFolderIconFiles
{
    // The folder's copies, by name
    public Dictionary<string, byte[]> Files { get; } = [];

    // Whether writing fails, as in a read-only folder
    public bool ReadOnly { get; set; }

    public IReadOnlyList<string> List(string folder) => Files.Keys.ToList();

    public byte[]? Read(string folder, string name) => Files.GetValueOrDefault(name);

    public void Write(string folder, string name, byte[] icon)
    {
        if (ReadOnly)
        {
            throw new UnauthorizedAccessException();
        }

        Files[name] = icon;
    }

    public void Delete(string folder, string name) => Files.Remove(name);
}
