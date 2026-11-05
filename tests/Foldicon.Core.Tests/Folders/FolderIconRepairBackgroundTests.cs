// Tests of the walk at low priority and of the places it starts from
using Foldicon.Core.Folders;
using Foldicon.Core.Tests.Fakes;

namespace Foldicon.Core.Tests.Folders;

[TestClass]
public sealed class FolderIconRepairBackgroundTests
{
    // Tree that notes the priority of the thread that walks it
    private sealed class ProbeTree : IFolderTree
    {
        public ThreadPriority Priority { get; private set; }

        public IReadOnlyList<string> Subfolders(string folder)
        {
            Priority = Thread.CurrentThread.Priority;
            return [];
        }
    }

    [TestMethod]
    public async Task walks_at_low_priority_on_a_background_thread()
    {
        var tree = new ProbeTree();
        var repair = new FolderIconRepair(tree, new FakeFolderIconFiles(), new FakeDesktopIniFile(null), new FakeFolderMarks(), new FakeFolderShell());

        var repaired = await repair.RepairAllInBackground([@"C:\Root"], FolderScanLimits.Startup);

        Assert.AreEqual(0, repaired);
        Assert.AreEqual(ThreadPriority.BelowNormal, tree.Priority);
    }

    // Tree of three levels whose middle folder holds a copy that cannot be read
    private sealed class DeepTree : IFolderTree
    {
        public IReadOnlyList<string> Subfolders(string folder) => folder switch
        {
            "root" => ["locked"],
            "locked" => ["inside"],
            _ => [],
        };
    }

    // Copies that fail in one folder and are missing in the rest
    private sealed class LockedFiles : IFolderIconFiles
    {
        public List<string> Listed { get; } = [];

        public IReadOnlyList<string> List(string folder)
        {
            Listed.Add(folder);
            return folder == "locked" ? throw new UnauthorizedAccessException() : [];
        }

        public byte[]? Read(string folder, string name) => null;

        public void Write(string folder, string name, byte[] icon)
        {
        }

        public void Delete(string folder, string name)
        {
        }
    }

    // Tree that cannot list its only folder
    private sealed class BrokenTree : IFolderTree
    {
        public IReadOnlyList<string> Subfolders(string folder) => throw new IOException();
    }

    [TestMethod]
    public void goes_on_through_the_folders_inside_one_that_fails()
    {
        var files = new LockedFiles();
        var repair = new FolderIconRepair(new DeepTree(), files, new FakeDesktopIniFile(null), new FakeFolderMarks(), new FakeFolderShell());

        repair.RepairAll(["root"], FolderScanLimits.Startup);

        CollectionAssert.Contains(files.Listed, "inside");
    }

    [TestMethod]
    public void survives_a_folder_that_cannot_be_listed()
    {
        var repair = new FolderIconRepair(new BrokenTree(), new FakeFolderIconFiles(), new FakeDesktopIniFile(null), new FakeFolderMarks(), new FakeFolderShell());

        Assert.AreEqual(0, repair.RepairAll(["root"], FolderScanLimits.Startup));
    }

    [TestMethod]
    public void starts_from_the_folder_of_the_user_and_the_fixed_drives()
    {
        var profile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

        CollectionAssert.AreEqual(new[] { profile }, FolderIconRepair.StartupRoots().ToArray());
        Assert.AreEqual(profile, FolderIconRepair.ManualRoots()[0]);
        Assert.IsGreaterThan(1, FolderIconRepair.ManualRoots().Count);
    }

    [TestMethod]
    public void creates_the_services_of_the_real_system()
    {
        Assert.IsNotNull(FolderIconRepair.ForSystem());
        Assert.IsNotNull(FolderIconService.ForSystem());
    }
}
