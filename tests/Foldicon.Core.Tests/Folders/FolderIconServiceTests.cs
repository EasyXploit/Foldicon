// Tests of applying, undoing and restoring the icon of a folder
using Foldicon.Core.Folders;
using Foldicon.Core.Icons;
using Foldicon.Core.Tests.Fakes;

namespace Foldicon.Core.Tests.Folders;

[TestClass]
public sealed class FolderIconServiceTests
{
    private const string Folder = @"C:\Photos";

    [TestMethod]
    public void applies_the_icon_and_returns_the_previous_one()
    {
        var shell = new FakeFolderShell();
        var service = new FolderIconService(shell, new FakeDesktopIniFile("[.ShellClassInfo]\nIconResource=old.dll,-1\n"));

        var previous = service.Apply(Folder, new IconReference("imageres.dll", 3));

        Assert.AreEqual("old.dll,-1", previous);
        Assert.AreEqual((Folder, @"%SystemRoot%\System32\imageres.dll", -3), shell.Icons.Single());
        CollectionAssert.AreEqual(new[] { Folder }, shell.Refreshed);
    }

    [TestMethod]
    public void returns_no_previous_icon_for_a_plain_folder()
    {
        var service = new FolderIconService(new FakeFolderShell(), new FakeDesktopIniFile(null));
        Assert.IsNull(service.Apply(Folder, new IconReference("shell32.dll", 4)));
    }

    [TestMethod]
    public void undoes_by_writing_the_previous_icon_again()
    {
        var shell = new FakeFolderShell();
        var service = new FolderIconService(shell, new FakeDesktopIniFile(null));

        service.Undo(Folder, @"C:\Icons\set.icl,7");

        Assert.AreEqual((Folder, @"C:\Icons\set.icl", 7), shell.Icons.Single());
    }

    [TestMethod]
    public void undoes_to_the_default_icon_when_there_was_none()
    {
        var file = new FakeDesktopIniFile("[.ShellClassInfo]\nIconResource=imageres.dll,-3\n");
        var shell = new FakeFolderShell();
        new FolderIconService(shell, file).Undo(Folder, null);

        Assert.IsTrue(file.Deleted);
        Assert.IsEmpty(shell.Icons);
    }

    [TestMethod]
    public void restores_by_removing_only_the_icon()
    {
        var file = new FakeDesktopIniFile("[.ShellClassInfo]\nIconResource=imageres.dll,-3\nInfoTip=Trips\n");
        new FolderIconService(new FakeFolderShell(), file).Restore(Folder);

        Assert.AreEqual("[.ShellClassInfo]\nInfoTip=Trips\n", file.Text);
        Assert.IsFalse(file.Deleted);
    }

    [TestMethod]
    public void restores_by_deleting_a_file_left_empty()
    {
        var file = new FakeDesktopIniFile("[.ShellClassInfo]\r\nIconResource=imageres.dll,-3\r\n");
        var shell = new FakeFolderShell();
        new FolderIconService(shell, file).Restore(Folder);

        Assert.IsTrue(file.Deleted);
        CollectionAssert.AreEqual(new[] { Folder }, shell.Refreshed);
    }

    [TestMethod]
    public void restores_a_folder_without_desktop_ini_by_refreshing_it()
    {
        var file = new FakeDesktopIniFile(null);
        var shell = new FakeFolderShell();
        new FolderIconService(shell, file).Restore(Folder);

        Assert.IsFalse(file.Deleted);
        CollectionAssert.AreEqual(new[] { Folder }, shell.Refreshed);
    }
}
