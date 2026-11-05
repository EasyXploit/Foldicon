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
        var service = NewService(shell, new FakeDesktopIniFile("[.ShellClassInfo]\nIconResource=old.dll,-1\n"));

        var previous = service.Apply(Folder, new IconReference("imageres.dll", 3));

        Assert.AreEqual("old.dll,-1", previous);
        Assert.AreEqual((Folder, @"%SystemRoot%\System32\imageres.dll", -3), shell.Icons.Single());
        CollectionAssert.AreEqual(new[] { Folder }, shell.Refreshed);
    }

    [TestMethod]
    public void keeps_a_copy_of_a_bundled_icon_in_the_folder_and_points_to_it_by_its_name()
    {
        var shell = new FakeFolderShell();
        var copies = new FakeFolderIconFiles();

        NewService(shell, new FakeDesktopIniFile(null), copies).Apply(Folder, Bundled(12));

        Assert.AreEqual((Folder, ".foldicon-12.ico", 0), shell.Icons.Single());
        CollectionAssert.AreEqual(new byte[] { 12 }, copies.Files[".foldicon-12.ico"]);
    }

    [TestMethod]
    public void replaces_the_copy_when_another_bundled_icon_is_applied()
    {
        var copies = new FakeFolderIconFiles();
        var service = NewService(new FakeFolderShell(), new FakeDesktopIniFile(null), copies);

        service.Apply(Folder, Bundled(1));
        service.Apply(Folder, Bundled(2));

        CollectionAssert.AreEqual(new[] { ".foldicon-2.ico" }, copies.Files.Keys.ToArray());
    }

    [TestMethod]
    public void keeps_the_copy_when_the_same_bundled_icon_is_applied_again()
    {
        var copies = new FakeFolderIconFiles();
        var service = NewService(new FakeFolderShell(), new FakeDesktopIniFile(null), copies);

        service.Apply(Folder, Bundled(1));
        service.Apply(Folder, Bundled(1));

        CollectionAssert.AreEqual(new[] { ".foldicon-1.ico" }, copies.Files.Keys.ToArray());
    }

    [TestMethod]
    public void deletes_the_copy_when_a_system_icon_is_applied()
    {
        var copies = new FakeFolderIconFiles();
        var service = NewService(new FakeFolderShell(), new FakeDesktopIniFile(null), copies);

        service.Apply(Folder, Bundled(1));
        service.Apply(Folder, new IconReference("imageres.dll", 3));

        Assert.IsEmpty(copies.Files);
    }

    [TestMethod]
    public void changes_nothing_in_a_folder_that_cannot_be_written()
    {
        var shell = new FakeFolderShell();
        var copies = new FakeFolderIconFiles { ReadOnly = true };
        var service = NewService(shell, new FakeDesktopIniFile(null), copies);

        Assert.ThrowsExactly<UnauthorizedAccessException>(() => service.Apply(Folder, Bundled(1)));

        Assert.IsEmpty(shell.Icons);
        Assert.IsEmpty(shell.Refreshed);
    }

    [TestMethod]
    public void changes_nothing_when_the_bundled_icon_cannot_be_read()
    {
        var shell = new FakeFolderShell();
        var service = NewService(shell, new FakeDesktopIniFile(null));

        Assert.ThrowsExactly<IOException>(() => service.Apply(Folder, Bundled(FakeBundledIconSource.Missing)));

        Assert.IsEmpty(shell.Icons);
    }

    [TestMethod]
    public void removes_a_new_copy_when_the_shell_refuses_the_icon()
    {
        var copies = new FakeFolderIconFiles();
        var shell = new FakeFolderShell { Refuses = true };
        var service = NewService(shell, new FakeDesktopIniFile(null), copies);

        Assert.ThrowsExactly<UnauthorizedAccessException>(() => service.Apply(Folder, Bundled(1)));

        Assert.IsEmpty(copies.Files);
    }

    [TestMethod]
    public void keeps_a_copy_in_use_when_the_shell_refuses_the_same_icon()
    {
        var copies = new FakeFolderIconFiles();
        copies.Files[".foldicon-1.ico"] = [9];
        var shell = new FakeFolderShell { Refuses = true };
        var service = NewService(shell, new FakeDesktopIniFile(null), copies);

        Assert.ThrowsExactly<UnauthorizedAccessException>(() => service.Apply(Folder, Bundled(1)));

        Assert.IsTrue(copies.Files.ContainsKey(".foldicon-1.ico"));
    }

    [TestMethod]
    public void restores_by_deleting_the_copy()
    {
        var copies = new FakeFolderIconFiles();
        copies.Files[".foldicon-1.ico"] = [1];
        var file = new FakeDesktopIniFile("[.ShellClassInfo]\nIconResource=.foldicon-1.ico,0\n");

        NewService(new FakeFolderShell(), file, copies).Restore(Folder);

        Assert.IsEmpty(copies.Files);
        Assert.IsTrue(file.Deleted);
    }

    [TestMethod]
    public void restores_by_deleting_a_copy_the_folder_no_longer_uses()
    {
        var copies = new FakeFolderIconFiles();
        copies.Files[".foldicon-1.ico"] = [1];

        NewService(new FakeFolderShell(), new FakeDesktopIniFile(null), copies).Restore(Folder);

        Assert.IsEmpty(copies.Files);
    }

    [TestMethod]
    public void undoes_a_restore_by_bringing_the_copy_back()
    {
        const string Name = ".foldicon-1.ico";
        var shell = new FakeFolderShell();
        var copies = new FakeFolderIconFiles();
        copies.Files[Name] = [5];
        var service = NewService(shell, new FakeDesktopIniFile($"[.ShellClassInfo]\nIconResource={Name},0\n"), copies);

        service.Restore(Folder);
        service.Undo(Folder, $"{Name},0");

        CollectionAssert.AreEqual(new byte[] { 5 }, copies.Files[Name]);
        Assert.AreEqual((Folder, Name, 0), shell.Icons.Single());
    }

    [TestMethod]
    public void undoes_a_restore_of_a_system_icon_without_writing_a_copy()
    {
        var copies = new FakeFolderIconFiles();
        var service = NewService(new FakeFolderShell(), new FakeDesktopIniFile("[.ShellClassInfo]\nIconResource=a.dll,-1\n"), copies);

        service.Restore(Folder);
        service.Undo(Folder, "a.dll,-1");

        Assert.IsEmpty(copies.Files);
    }

    [TestMethod]
    public void returns_no_previous_icon_for_a_plain_folder()
    {
        var service = NewService(new FakeFolderShell(), new FakeDesktopIniFile(null));
        Assert.IsNull(service.Apply(Folder, new IconReference("shell32.dll", 4)));
    }

    [TestMethod]
    public void undoes_by_writing_the_previous_icon_again()
    {
        var shell = new FakeFolderShell();
        var service = NewService(shell, new FakeDesktopIniFile(null));

        service.Undo(Folder, @"C:\Icons\set.icl,7");

        Assert.AreEqual((Folder, @"C:\Icons\set.icl", 7), shell.Icons.Single());
    }

    [TestMethod]
    public void undoes_to_the_default_icon_when_there_was_none()
    {
        var file = new FakeDesktopIniFile("[.ShellClassInfo]\nIconResource=imageres.dll,-3\n");
        var shell = new FakeFolderShell();
        NewService(shell, file).Undo(Folder, null);

        Assert.IsTrue(file.Deleted);
        Assert.IsEmpty(shell.Icons);
    }

    [TestMethod]
    public void restores_by_removing_only_the_icon()
    {
        var file = new FakeDesktopIniFile("[.ShellClassInfo]\nIconResource=imageres.dll,-3\nInfoTip=Trips\n");
        NewService(new FakeFolderShell(), file).Restore(Folder);

        Assert.AreEqual("[.ShellClassInfo]\nInfoTip=Trips\n", file.Text);
        Assert.IsFalse(file.Deleted);
    }

    [TestMethod]
    public void restores_by_deleting_a_file_left_empty()
    {
        var file = new FakeDesktopIniFile("[.ShellClassInfo]\r\nIconResource=imageres.dll,-3\r\n");
        var shell = new FakeFolderShell();
        NewService(shell, file).Restore(Folder);

        Assert.IsTrue(file.Deleted);
        CollectionAssert.AreEqual(new[] { Folder }, shell.Refreshed);
    }

    [TestMethod]
    public void restores_a_folder_without_desktop_ini_by_refreshing_it()
    {
        var file = new FakeDesktopIniFile(null);
        var shell = new FakeFolderShell();
        NewService(shell, file).Restore(Folder);

        Assert.IsFalse(file.Deleted);
        CollectionAssert.AreEqual(new[] { Folder }, shell.Refreshed);
    }

    // Returns an icon of the bundled library
    private static IconReference Bundled(int id) => new(BundledIconLibrary.Name, id);

    // Creates the service over fakes
    private static FolderIconService NewService(FakeFolderShell shell, FakeDesktopIniFile file, FakeFolderIconFiles? copies = null) =>
        new(shell, file, copies ?? new FakeFolderIconFiles(), new FakeBundledIconSource());
}
