// Tests of the order of the libraries in the tabs
using Foldicon.Core.Icons;
using Foldicon.Core.Indexing;

namespace Foldicon.Core.Tests.Indexing;

[TestClass]
public sealed class IconLibraryTabsTests
{
    [TestMethod]
    public void puts_the_bundled_library_before_those_of_the_system()
    {
        Assert.AreEqual(BundledIconLibrary.Name, IconLibraryTabs.All[0]);
        CollectionAssert.AreEqual(SystemIconLibraries.All.ToArray(), IconLibraryTabs.All.Skip(1).ToArray());
    }

    [TestMethod]
    public void ranks_a_library_by_its_tab()
    {
        Assert.AreEqual(0, IconLibraryTabs.Rank(BundledIconLibrary.Name));
        Assert.AreEqual(1, IconLibraryTabs.Rank("imageres.dll"));
    }

    [TestMethod]
    public void places_any_other_library_after_the_tabs()
    {
        Assert.AreEqual(int.MaxValue, IconLibraryTabs.Rank("other.dll"));
    }
}
