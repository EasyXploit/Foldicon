// Tests of the repair of the folders that lost the marks of their icon
using Foldicon.Core.Folders;
using Foldicon.Core.Tests.Fakes;

namespace Foldicon.Core.Tests.Folders;

[TestClass]
public sealed class FolderIconRepairTests
{
    private const string Folder = @"C:\Sync\Work";
    private const string Copy = ".foldicon-1.ico";
    private const string Ini = "[.ShellClassInfo]\nIconResource=.foldicon-1.ico,0\n";

    [TestMethod]
    public void puts_back_every_mark_a_sync_client_dropped()
    {
        var marks = new FakeFolderMarks();
        var shell = new FakeFolderShell();

        Assert.IsTrue(NewRepair(shell, Ini, marks).Repair(Folder));

        Assert.IsTrue(marks.Customized);
        CollectionAssert.AreEquivalent(new[] { "desktop.ini", Copy }, marks.Hidden.ToArray());
        CollectionAssert.AreEqual(new[] { Folder }, shell.Refreshed);
    }

    [TestMethod]
    public void leaves_a_healthy_folder_alone()
    {
        var marks = new FakeFolderMarks { Customized = true };
        marks.Hidden.UnionWith(["desktop.ini", Copy]);
        var shell = new FakeFolderShell();

        Assert.IsFalse(NewRepair(shell, Ini, marks).Repair(Folder));

        Assert.IsEmpty(shell.Refreshed);
    }

    [TestMethod]
    public void leaves_a_folder_without_a_copy_alone_without_reading_its_settings()
    {
        var shell = new FakeFolderShell();

        Assert.IsFalse(NewRepair(shell, "[.ShellClassInfo]\nIconResource=a.dll,-1\n", new FakeFolderMarks(), copies: []).Repair(Folder));

        Assert.IsEmpty(shell.Icons);
    }

    [TestMethod]
    public void writes_the_icon_again_when_the_desktop_ini_did_not_arrive()
    {
        var shell = new FakeFolderShell();

        Assert.IsTrue(NewRepair(shell, null, new FakeFolderMarks()).Repair(Folder));

        Assert.AreEqual((Folder, Copy, 0), shell.Icons.Single());
    }

    [TestMethod]
    public void does_not_guess_the_icon_among_several_copies()
    {
        var shell = new FakeFolderShell();

        Assert.IsFalse(NewRepair(shell, null, new FakeFolderMarks(), copies: [Copy, ".foldicon-2.ico"]).Repair(Folder));

        Assert.IsEmpty(shell.Icons);
    }

    [TestMethod]
    public void respects_a_folder_that_chose_another_icon()
    {
        var marks = new FakeFolderMarks();

        Assert.IsFalse(NewRepair(new FakeFolderShell(), "[.ShellClassInfo]\nIconResource=a.dll,-1\n", marks).Repair(Folder));

        Assert.IsEmpty(marks.Hidden);
    }

    [TestMethod]
    public void does_not_count_a_mark_the_drive_refuses()
    {
        var marks = new FakeFolderMarks { Customized = true, RefusesToHide = true };
        var shell = new FakeFolderShell();

        Assert.IsFalse(NewRepair(shell, Ini, marks).Repair(Folder));

        Assert.IsEmpty(shell.Refreshed);
    }

    [TestMethod]
    public void walks_the_tree_breadth_first()
    {
        var tree = new FakeFolderTree(new() { ["C:"] = ["C:\\a", "C:\\b"], ["C:\\a"] = ["C:\\a\\c"] });
        var marks = new FakeFolderMarks();
        var repair = new FolderIconRepair(tree, Copies([]), new FakeDesktopIniFile(Ini), marks, new FakeFolderShell());

        repair.RepairAll(["C:"], FolderScanLimits.Manual);

        CollectionAssert.AreEqual(new[] { "C:", "C:\\a", "C:\\b", "C:\\a\\c" }, tree.Walked);
    }

    [TestMethod]
    public void stops_at_the_maximum_depth()
    {
        var tree = new FakeFolderTree(new() { ["r"] = ["r/a"], ["r/a"] = ["r/a/b"] });

        NewWalk(tree).RepairAll(["r"], new FolderScanLimits(1, 100));

        CollectionAssert.AreEqual(new[] { "r" }, tree.Walked);
    }

    [TestMethod]
    public void stops_at_the_maximum_number_of_folders()
    {
        var tree = new FakeFolderTree(new() { ["r"] = ["r/a", "r/b", "r/c"] });

        NewWalk(tree).RepairAll(["r"], new FolderScanLimits(9, 2));

        CollectionAssert.AreEqual(new[] { "r", "r/a" }, tree.Walked);
    }

    [TestMethod]
    public void walks_only_once_a_folder_reached_from_two_roots()
    {
        var tree = new FakeFolderTree(new() { ["r"] = ["r/a"] });

        NewWalk(tree).RepairAll(["r", "r/a"], FolderScanLimits.Manual);

        CollectionAssert.AreEqual(new[] { "r", "r/a" }, tree.Walked);
    }

    [TestMethod]
    public void keeps_walking_after_a_folder_that_fails()
    {
        var tree = new FakeFolderTree(new() { ["r"] = ["r/a", "r/b"] }) { Denied = "r/a" };

        NewWalk(tree).RepairAll(["r"], FolderScanLimits.Manual);

        CollectionAssert.AreEqual(new[] { "r", "r/a", "r/b" }, tree.Walked);
    }

    [TestMethod]
    public void counts_each_folder_it_repairs()
    {
        var tree = new FakeFolderTree(new() { ["r"] = ["r/a"] });
        var repair = new FolderIconRepair(tree, Copies([Copy]), new FakeDesktopIniFile(Ini), new FakeFolderMarks(), new FakeFolderShell());

        Assert.AreEqual(1, repair.RepairAll(["r"], FolderScanLimits.Manual));
    }

    // Returns the copies of a folder
    private static FakeFolderIconFiles Copies(string[] names)
    {
        var copies = new FakeFolderIconFiles();
        foreach (var name in names)
        {
            copies.Files[name] = [1];
        }

        return copies;
    }

    // Creates a repair of a single folder
    private static FolderIconRepair NewRepair(FakeFolderShell shell, string? ini, FakeFolderMarks marks, string[]? copies = null) =>
        new(new FakeFolderTree([]), Copies(copies ?? [Copy]), new FakeDesktopIniFile(ini), marks, shell);

    // Creates a repair that only walks
    private static FolderIconRepair NewWalk(FakeFolderTree tree) =>
        new(tree, Copies([]), new FakeDesktopIniFile(null), new FakeFolderMarks(), new FakeFolderShell());
}
