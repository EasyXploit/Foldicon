// Tests of the marks of real folders and files
using Foldicon.Core.Folders;
using Foldicon.Core.Tests.Fakes;

namespace Foldicon.Core.Tests.RealSystem;

[TestClass]
public sealed class FolderMarksTests
{
    private static readonly FolderMarks Marks = new();

    [TestMethod]
    public void marks_a_folder_as_customized_keeping_its_attributes()
    {
        using var folder = new TemporaryFolder();
        File.SetAttributes(folder.Path, File.GetAttributes(folder.Path) | FileAttributes.Hidden);
        Assert.IsFalse(Marks.IsCustomized(folder.Path));

        Marks.MarkCustomized(folder.Path);

        Assert.IsTrue(Marks.IsCustomized(folder.Path));
        Assert.IsTrue(File.GetAttributes(folder.Path).HasFlag(FileAttributes.Hidden));
    }

    [TestMethod]
    public void takes_the_system_mark_as_customized()
    {
        using var folder = new TemporaryFolder();
        File.SetAttributes(folder.Path, File.GetAttributes(folder.Path) | FileAttributes.System);

        Assert.IsTrue(Marks.IsCustomized(folder.Path));
    }

    [TestMethod]
    public void hides_a_file_with_both_marks()
    {
        using var folder = new TemporaryFolder();
        File.WriteAllText(folder.Inside("a.ini"), "");
        Assert.IsFalse(Marks.IsHidden(folder.Path, "a.ini"));

        Assert.IsTrue(Marks.Hide(folder.Path, "a.ini"));

        Assert.IsTrue(Marks.IsHidden(folder.Path, "a.ini"));
    }

    [TestMethod]
    public void does_not_take_a_single_mark_as_hidden()
    {
        using var folder = new TemporaryFolder();
        File.WriteAllText(folder.Inside("a.ini"), "");
        File.SetAttributes(folder.Inside("a.ini"), FileAttributes.Hidden);

        Assert.IsFalse(Marks.IsHidden(folder.Path, "a.ini"));
    }
}
