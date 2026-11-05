// Tests that turn the search box text into groups of words
using Foldicon.Core.Search;
using Foldicon.Core.Tests.Fakes;

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
    public void runs_a_pasted_text_keeping_only_the_first_words()
    {
        using var store = InMemoryStore.Open();
        var text = string.Join(' ', Enumerable.Range(0, 2000).Select(number => "a" + number));
        var groups = SearchQuery.Parse(text, SynonymTable.LoadEmbedded());

        Assert.HasCount(SearchQuery.MaximumWords, groups);
        Assert.IsEmpty(store.Search(groups, null));
    }

    [TestMethod]
    public void runs_the_longest_search_with_the_most_synonyms()
    {
        using var store = InMemoryStore.Open();
        var text = string.Join(' ', Enumerable.Range(0, SearchQuery.MaximumWords).Select(number => (char)('a' + number % 26) + (number >= 26 ? "a" : "")));

        Assert.IsEmpty(store.Search(SearchQuery.Parse(text, SynonymTable.LoadEmbedded()), null));
    }

    [TestMethod]
    public void returns_no_groups_for_an_empty_box()
    {
        Assert.IsEmpty(SearchQuery.Parse("", new SynonymTable([])));
    }
}
