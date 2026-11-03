// Tests of the turn of the text of the search box into groups of words
using Foldicon.Core.Search;

namespace Foldicon.Core.Tests.Search;

[TestClass]
public sealed class SearchQueryTests
{
    [TestMethod]
    public void builds_one_group_per_word()
    {
        var groups = SearchQuery.Parse("Carpeta azul", new SynonymTable([["folder", "carpeta"]]));

        Assert.HasCount(2, groups);
        CollectionAssert.Contains(groups[0].ToList(), "folder");
        CollectionAssert.AreEqual(new[] { "azul" }, groups[1].ToArray());
    }

    [TestMethod]
    public void returns_no_groups_for_an_empty_box()
    {
        Assert.IsEmpty(SearchQuery.Parse("", new SynonymTable([])));
    }
}
