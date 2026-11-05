// Tests of the values written for an icon of a library
using Foldicon.Core.Icons;

namespace Foldicon.Core.Tests.Icons;

[TestClass]
public sealed class IconReferenceTests
{
    [TestMethod]
    public void writes_icon_resource_under_system_root()
    {
        var icon = new IconReference("imageres.dll", 3);
        Assert.AreEqual(@"%SystemRoot%\System32\imageres.dll,-3", icon.ToIconResource());
    }

    [TestMethod]
    public void writes_the_library_path_unexpanded()
    {
        Assert.AreEqual(@"%SystemRoot%\System32\shell32.dll", new IconReference("shell32.dll", 4).LibraryPath());
    }

    [TestMethod]
    public void writes_the_bundled_library_with_its_full_path()
    {
        var icon = new IconReference(BundledIconLibrary.Name, 7);
        var expected = Path.Combine(AppContext.BaseDirectory, "fluenticons.dll");

        Assert.AreEqual(expected, icon.LibraryPath());
        Assert.AreEqual($"{expected},-7", icon.ToIconResource());
    }

    [TestMethod]
    public void shows_library_and_negative_identifier()
    {
        Assert.AreEqual("shell32.dll,-16802", new IconReference("shell32.dll", 16802).ToString());
    }
}
