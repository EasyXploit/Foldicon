// Tests of the folders that the search leaves out
using Foldicon.Core.Folders;

namespace Foldicon.Core.Tests.Folders;

[TestClass]
public sealed class FolderScanRulesTests
{
    [TestMethod]
    public void skips_the_folders_of_the_system_and_of_tools()
    {
        Assert.IsTrue(FolderScanRules.Skips("AppData"));
        Assert.IsTrue(FolderScanRules.Skips("NODE_MODULES"));
        Assert.IsTrue(FolderScanRules.Skips(".git"));
    }

    [TestMethod]
    public void walks_the_folders_of_the_user()
    {
        Assert.IsFalse(FolderScanRules.Skips("Documents"));
        Assert.IsFalse(FolderScanRules.Skips("SynologyDrive"));
    }
}
