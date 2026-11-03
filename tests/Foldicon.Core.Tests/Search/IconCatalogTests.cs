// Tests of the reading of the catalog of icon names
using Foldicon.Core.Icons;
using Foldicon.Core.Search;

namespace Foldicon.Core.Tests.Search;

[TestClass]
public sealed class IconCatalogTests
{
    [TestMethod]
    public void reads_normalized_words_per_icon()
    {
        var names = IconCatalog.Parse("""{ "ImageRes.dll": { "3": ["Carpeta abierta", "folder"] } }""");
        var icon = new IconReference("imageres.dll", 3);

        CollectionAssert.AreEquivalent(
            new[] { (icon, "carpeta"), (icon, "abierta"), (icon, "folder") },
            names.ToArray());
    }

    [TestMethod]
    public void skips_identifiers_that_are_not_numbers()
    {
        Assert.IsEmpty(IconCatalog.Parse("""{ "shell32.dll": { "FOLDER": ["folder"], "-3": ["folder"] } }"""));
    }

    [TestMethod]
    public void loads_the_embedded_catalog()
    {
        Assert.IsNotNull(IconCatalog.LoadEmbedded());
    }
}
