// Tests of the real copies of an icon inside a folder and of their marks
using Foldicon.Core.Folders;
using Foldicon.Core.Tests.Fakes;

namespace Foldicon.Core.Tests.RealSystem;

[TestClass]
public sealed class FolderIconFilesTests
{
    private static readonly FolderIconFiles Files = new();
    private static readonly byte[] Icon = [1, 2, 3];

    [TestMethod]
    public void writes_a_hidden_copy_and_lists_it()
    {
        using var folder = new TemporaryFolder();
        var name = FolderIconCopyName.For(7);

        Files.Write(folder.Path, name, Icon);

        CollectionAssert.AreEqual(new[] { name }, Files.List(folder.Path).ToArray());
        Assert.IsTrue(File.GetAttributes(folder.Inside(name)).HasFlag(FolderIconFiles.Hidden));
    }

    [TestMethod]
    public void leaves_out_files_that_only_look_like_a_copy()
    {
        using var folder = new TemporaryFolder();
        File.WriteAllBytes(folder.Inside(".foldicon-name.ico"), Icon);
        File.WriteAllBytes(folder.Inside("other.ico"), Icon);

        Assert.IsEmpty(Files.List(folder.Path));
    }

    [TestMethod]
    public void replaces_a_hidden_copy()
    {
        using var folder = new TemporaryFolder();
        var name = FolderIconCopyName.For(7);
        Files.Write(folder.Path, name, [9]);

        Files.Write(folder.Path, name, Icon);

        CollectionAssert.AreEqual(Icon, Files.Read(folder.Path, name));
    }

    [TestMethod]
    public void reads_nothing_when_the_copy_is_missing()
    {
        using var folder = new TemporaryFolder();

        Assert.IsNull(Files.Read(folder.Path, FolderIconCopyName.For(7)));
    }

    [TestMethod]
    public void deletes_a_hidden_copy_and_ignores_a_missing_one()
    {
        using var folder = new TemporaryFolder();
        var name = FolderIconCopyName.For(7);
        Files.Write(folder.Path, name, Icon);

        Files.Delete(folder.Path, name);
        Files.Delete(folder.Path, name);

        Assert.IsEmpty(Files.List(folder.Path));
    }

    [TestMethod]
    public void reports_a_file_that_cannot_be_hidden()
    {
        using var folder = new TemporaryFolder();

        Assert.IsFalse(FolderIconFiles.Hide(folder.Inside("missing.ico")));
    }
}
