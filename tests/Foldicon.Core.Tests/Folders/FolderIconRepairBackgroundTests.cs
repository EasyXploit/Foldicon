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
