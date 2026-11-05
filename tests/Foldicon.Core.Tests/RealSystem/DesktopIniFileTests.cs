// Tests of the access to a real desktop.ini: its encodings, its marks and the marks of the folder
using System.Text;
using Foldicon.Core.Folders;
using Foldicon.Core.Tests.Fakes;

namespace Foldicon.Core.Tests.RealSystem;

[TestClass]
public sealed class DesktopIniFileTests
{
    private static readonly DesktopIniFile Ini = new();

    [TestMethod]
    public void reads_nothing_from_a_folder_without_the_file()
    {
        using var folder = new TemporaryFolder();

        Assert.IsNull(Ini.Read(folder.Path));
    }

    [TestMethod]
    public void reads_and_rewrites_utf16_keeping_its_mark()
    {
        using var folder = new TemporaryFolder();
        var path = folder.Inside(DesktopIniFile.FileName);
        File.WriteAllText(path, "[.ShellClassInfo]\nIconResource=é,1\n", Encoding.Unicode);
        File.SetAttributes(path, FileAttributes.Hidden | FileAttributes.System);

        Assert.Contains("é", Ini.Read(folder.Path)!);
        Ini.Write(folder.Path, "[Other]\nKey=é\n");

        var bytes = File.ReadAllBytes(path);
        Assert.AreEqual(0xFF, bytes[0]);
        Assert.AreEqual(0xFE, bytes[1]);
        Assert.AreEqual("[Other]\nKey=é\n", Ini.Read(folder.Path));
        Assert.IsTrue(File.GetAttributes(path).HasFlag(FileAttributes.Hidden));
    }

    [TestMethod]
    public void keeps_the_mark_of_utf8()
    {
        using var folder = new TemporaryFolder();
        var path = folder.Inside(DesktopIniFile.FileName);
        File.WriteAllText(path, "a=é\n", new UTF8Encoding(true));

        Assert.AreEqual("a=é\n", Ini.Read(folder.Path));
        Ini.Write(folder.Path, "b=é\n");

        Assert.AreEqual(0xEF, File.ReadAllBytes(path)[0]);
        Assert.AreEqual("b=é\n", Ini.Read(folder.Path));
    }

    [TestMethod]
    public void writes_back_every_byte_of_a_file_without_a_mark()
    {
        using var folder = new TemporaryFolder();
        var path = folder.Inside(DesktopIniFile.FileName);
        byte[] bytes = [0x61, 0x3D, 0xE9, 0x98, 0x0A];
        File.WriteAllBytes(path, bytes);

        Ini.Write(folder.Path, Ini.Read(folder.Path)!);

        CollectionAssert.AreEqual(bytes, File.ReadAllBytes(path));
    }

    [TestMethod]
    public void deletes_the_file_and_only_the_marks_that_customize_the_folder()
    {
        using var folder = new TemporaryFolder();
        var path = folder.Inside(DesktopIniFile.FileName);
        File.WriteAllText(path, "a=b\n");
        File.SetAttributes(path, FileAttributes.Hidden | FileAttributes.System | FileAttributes.ReadOnly);
        File.SetAttributes(folder.Path, File.GetAttributes(folder.Path) | FileAttributes.ReadOnly | FileAttributes.System | FileAttributes.Hidden);

        Ini.Delete(folder.Path);

        var attributes = File.GetAttributes(folder.Path);
        Assert.IsFalse(File.Exists(path));
        Assert.IsFalse(attributes.HasFlag(FileAttributes.ReadOnly));
        Assert.IsFalse(attributes.HasFlag(FileAttributes.System));
        Assert.IsTrue(attributes.HasFlag(FileAttributes.Hidden));
    }

    [TestMethod]
    public void clears_the_marks_of_a_folder_that_has_no_file()
    {
        using var folder = new TemporaryFolder();
        File.SetAttributes(folder.Path, File.GetAttributes(folder.Path) | FileAttributes.ReadOnly);

        Ini.Delete(folder.Path);

        Assert.IsFalse(File.GetAttributes(folder.Path).HasFlag(FileAttributes.ReadOnly));
    }
}
