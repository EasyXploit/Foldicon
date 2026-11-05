// Tests of the reading and removal of keys of a desktop.ini
using Foldicon.Core.Folders;

namespace Foldicon.Core.Tests.Folders;

[TestClass]
public sealed class DesktopIniEditorTests
{
    private const string Section = DesktopIniEditor.ShellClassInfo;
    private const string Key = DesktopIniEditor.IconResource;

    [TestMethod]
    public void reads_the_icon_of_a_text_and_nothing_without_one()
    {
        Assert.AreEqual("a.dll,-1", DesktopIniEditor.ReadIconResource("[.ShellClassInfo]\nIconResource=a.dll,-1\n"));
        Assert.IsNull(DesktopIniEditor.ReadIconResource("[.ShellClassInfo]\n"));
        Assert.IsNull(DesktopIniEditor.ReadIconResource(null));
    }

    [TestMethod]
    public void reads_a_key_ignoring_case_and_spaces()
    {
        var text = "[.ShellClassInfo]\r\n iconresource = imageres.dll,-3\r\n";
        Assert.AreEqual("imageres.dll,-3", DesktopIniEditor.ReadValue(text, Section, Key));
    }

    [TestMethod]
    public void ignores_the_key_in_another_section_or_a_comment()
    {
        var text = "[Other]\nIconResource=a,1\n[.ShellClassInfo]\n;IconResource=b,2\n";
        Assert.IsNull(DesktopIniEditor.ReadValue(text, Section, Key));
    }

    [TestMethod]
    public void removes_only_the_icon_and_keeps_other_settings()
    {
        var text = "[.ShellClassInfo]\r\nIconResource=imageres.dll,-3\r\nLocalizedResourceName=@shell32.dll,-21770\r\n[ViewState]\r\nMode=\r\n";
        Assert.AreEqual(
            "[.ShellClassInfo]\r\nLocalizedResourceName=@shell32.dll,-21770\r\n[ViewState]\r\nMode=\r\n",
            DesktopIniEditor.RemoveKey(text, Section, Key));
    }

    [TestMethod]
    public void drops_the_section_left_without_keys()
    {
        var text = "[.ShellClassInfo]\nIconResource=imageres.dll,-3\n\n[ViewState]\nFolderType=Pictures\n";
        Assert.AreEqual("[ViewState]\nFolderType=Pictures\n", DesktopIniEditor.RemoveKey(text, Section, Key));
    }

    [TestMethod]
    public void leaves_nothing_when_the_icon_was_the_only_setting()
    {
        var remaining = DesktopIniEditor.RemoveKey("[.ShellClassInfo]\r\nIconResource=imageres.dll,-3\r\n", Section, Key);
        Assert.IsTrue(DesktopIniEditor.IsEmpty(remaining));
    }

    [TestMethod]
    public void keeps_the_text_without_the_key()
    {
        var text = "[ViewState]\nMode=\n";
        Assert.AreEqual(text, DesktopIniEditor.RemoveKey(text, Section, Key));
    }
}
