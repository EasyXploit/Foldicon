// Tests of the expansion of a word with its Spanish and English synonyms
using Foldicon.Core.Search;

namespace Foldicon.Core.Tests.Search;

[TestClass]
public sealed class SynonymTableTests
{
    private static readonly SynonymTable Table = new([["folder", "carpeta"], ["music", "música"]]);

    [TestMethod]
    public void expands_a_word_with_its_group()
    {
        CollectionAssert.AreEquivalent(new[] { "carpeta", "folder" }, Table.Expand("carpeta").ToArray());
    }

    [TestMethod]
    public void expands_a_word_still_being_typed()
    {
        CollectionAssert.Contains(Table.Expand("carp").ToList(), "folder");
    }

    [TestMethod]
    public void matches_the_normalized_form_of_the_group()
    {
        CollectionAssert.Contains(Table.Expand("musica").ToList(), "music");
    }

    [TestMethod]
    public void keeps_an_unknown_word_alone()
    {
        CollectionAssert.AreEqual(new[] { "zebra" }, Table.Expand("zebra").ToArray());
    }

    [TestMethod]
    public void loads_the_embedded_table()
    {
        CollectionAssert.Contains(SynonymTable.LoadEmbedded().Expand("carpeta").ToList(), "folder");
    }
}
