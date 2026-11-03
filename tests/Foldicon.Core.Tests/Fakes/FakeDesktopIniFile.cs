// desktop.ini kept in memory, which notes whether it was deleted
using Foldicon.Core.Folders;

namespace Foldicon.Core.Tests.Fakes;

internal sealed class FakeDesktopIniFile(string? text) : IDesktopIniFile
{
    // Current text of the file, or null when there is none
    public string? Text { get; private set; } = text;

    // Whether the file was deleted
    public bool Deleted { get; private set; }

    public string? Read(string folder) => Text;

    public void Write(string folder, string text) => Text = text;

    public void Delete(string folder)
    {
        Text = null;
        Deleted = true;
    }
}
