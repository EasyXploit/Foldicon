// Tests of the split of a text into normalized words
using Foldicon.Core.Search;

namespace Foldicon.Core.Tests.Search;

[TestClass]
public sealed class TextNormalizerTests
{
    [TestMethod]
    public void lowercases_and_drops_accents()
    {
        CollectionAssert.AreEqual(new[] { "musica", "cancion" }, TextNormalizer.Words("Música  Canción").ToArray());
    }

    [TestMethod]
    public void splits_on_punctuation_and_drops_repeated_words()
    {
        CollectionAssert.AreEqual(new[] { "shell32", "dll", "4" }, TextNormalizer.Words("shell32.dll,-4 shell32").ToArray());
    }

    [TestMethod]
    public void returns_nothing_for_blank_text()
    {
        Assert.IsEmpty(TextNormalizer.Words("   "));
        Assert.IsEmpty(TextNormalizer.Words(null));
    }
}
