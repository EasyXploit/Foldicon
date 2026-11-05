// Tests of the names of the copies of an icon
using Foldicon.Core.Folders;

namespace Foldicon.Core.Tests.Folders;

[TestClass]
public sealed class FolderIconCopyNameTests
{
    [TestMethod]
    public void names_a_copy_after_its_identifier()
    {
        Assert.AreEqual(".foldicon-129.ico", FolderIconCopyName.For(129));
    }

    [TestMethod]
    public void recognizes_its_own_names()
    {
        Assert.IsTrue(FolderIconCopyName.IsCopy(FolderIconCopyName.For(7)));
    }

    [TestMethod]
    public void rejects_any_other_file()
    {
        Assert.IsFalse(FolderIconCopyName.IsCopy("folder.ico"));
        Assert.IsFalse(FolderIconCopyName.IsCopy(".foldicon-.ico"));
        Assert.IsFalse(FolderIconCopyName.IsCopy(".foldicon-a.ico"));
        Assert.IsFalse(FolderIconCopyName.IsCopy(@"C:\Icons\.foldicon-7.ico"));
    }
}
