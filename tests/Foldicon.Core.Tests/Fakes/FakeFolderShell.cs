// Shell that records the icons written and the folders refreshed
using Foldicon.Core.Folders;

namespace Foldicon.Core.Tests.Fakes;

internal sealed class FakeFolderShell : IFolderShell
{
    // Icons written, in order
    public List<(string Folder, string File, int Index)> Icons { get; } = [];

    // Folders refreshed, in order
    public List<string> Refreshed { get; } = [];

    // Whether the shell refuses to write the icon
    public bool Refuses { get; set; }

    public void SetIcon(string folder, string iconFile, int iconIndex)
    {
        if (Refuses)
        {
            throw new UnauthorizedAccessException();
        }

        Icons.Add((folder, iconFile, iconIndex));
    }

    public void Refresh(string folder) => Refreshed.Add(folder);
}
