// Tests of the reading of IconResource values
using Foldicon.Core.Folders;

namespace Foldicon.Core.Tests.Folders;

[TestClass]
public sealed class IconResourceValueTests
{
    [TestMethod]
    public void reads_file_and_negative_identifier()
    {
        Assert.AreEqual(
            new IconResourceValue(@"%SystemRoot%\System32\imageres.dll", -3),
            IconResourceValue.Parse(@"%SystemRoot%\System32\imageres.dll,-3"));
    }

    [TestMethod]
    public void splits_at_the_last_comma_and_drops_quotes()
    {
        Assert.AreEqual(
            new IconResourceValue(@"C:\Icons, old\set.icl", 7),
            IconResourceValue.Parse(@"""C:\Icons, old\set.icl"" , 7"));
    }

    [TestMethod]
    public void rejects_values_without_a_valid_index()
    {
        Assert.IsNull(IconResourceValue.Parse(@"C:\icon.ico"));
        Assert.IsNull(IconResourceValue.Parse(@"C:\icon.ico,abc"));
        Assert.IsNull(IconResourceValue.Parse(",3"));
        Assert.IsNull(IconResourceValue.Parse(" "));
    }
}
