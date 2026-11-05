// Tree of folders kept in memory, which can fail on one of them and notes the ones it walks
using Foldicon.Core.Folders;

namespace Foldicon.Core.Tests.Fakes;

internal sealed class FakeFolderTree(Dictionary<string, string[]> folders) : IFolderTree
{
    // Folders walked, in order
    public List<string> Walked { get; } = [];

    // Folder that cannot be read
    public string? Denied { get; set; }

    public IReadOnlyList<string> Subfolders(string folder)
    {
        Walked.Add(folder);
        return folder == Denied ? throw new UnauthorizedAccessException() : folders.GetValueOrDefault(folder, []);
    }
}
