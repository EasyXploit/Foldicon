// Marks kept in memory, which can refuse to hide a file
using Foldicon.Core.Folders;

namespace Foldicon.Core.Tests.Fakes;

internal sealed class FakeFolderMarks : IFolderMarks
{
    // Whether the folder is marked as customized
    public bool Customized { get; set; }

    // Names of the hidden files
    public HashSet<string> Hidden { get; } = [];

    // Whether hiding a file fails, as on some network drives
    public bool RefusesToHide { get; set; }

    public bool IsCustomized(string folder) => Customized;

    public void MarkCustomized(string folder) => Customized = true;

    public bool IsHidden(string folder, string name) => Hidden.Contains(name);

    public bool Hide(string folder, string name) => !RefusesToHide && Hidden.Add(name);
}
