// Tests of the real shell on a temporary folder: it writes the icon and marks the folder
using Foldicon.Core.Folders;
using Foldicon.Core.Tests.Fakes;

namespace Foldicon.Core.Tests.RealSystem;

[TestClass]
public sealed class NativeFolderShellTests
{
    private static readonly NativeFolderShell Shell = new();

    [TestMethod]
    public void writes_the_icon_and_marks_the_folder()
    {
        using var folder = new TemporaryFolder();

        Shell.SetIcon(folder.Path, @"%SystemRoot%\System32\imageres.dll", -3);
        Shell.Refresh(folder.Path);

        var value = IconResourceValue.Parse(DesktopIniEditor.ReadIconResource(new DesktopIniFile().Read(folder.Path)));
        Assert.AreEqual(new IconResourceValue(@"%SystemRoot%\System32\imageres.dll", -3), value);
        Assert.IsTrue(new FolderMarks().IsCustomized(folder.Path));
    }

    [TestMethod]
    public void keeps_the_other_settings_of_the_file()
    {
        using var folder = new TemporaryFolder();
        File.WriteAllText(folder.Inside(DesktopIniFile.FileName), "[Other]\nKey=Value\n");

        Shell.SetIcon(folder.Path, "icon.ico", 0);

        Assert.AreEqual("Value", DesktopIniEditor.ReadValue(new DesktopIniFile().Read(folder.Path)!, "Other", "Key"));
    }

    [TestMethod]
    public void raises_the_error_of_a_folder_that_does_not_exist()
    {
        using var folder = new TemporaryFolder();

        Assert.ThrowsExactly<IOException>(() => Shell.SetIcon(folder.Inside("missing"), "icon.ico", 0));
    }
}
