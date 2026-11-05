// Tests of the answers to the search box and the tabs
using Foldicon.Core.Icons;
using Foldicon.Core.Indexing;
using Foldicon.Core.Search;
using Foldicon.Core.Tests.Fakes;

namespace Foldicon.Core.Tests.Search;

[TestClass]
public sealed class IconBrowserTests
{
    private static readonly IconReference Star = new("imageres.dll", 1);
    private static readonly IconReference Folder = new("imageres.dll", 2);

    private static IconBrowser Open(Foldicon.Core.Storage.IconIndexStore store)
    {
        var entries = new[]
        {
            new IconCatalogEntry(Star, "Star", "Estrella", []),
            new IconCatalogEntry(Folder, "Folder", "Carpeta", []),
        };
        store.ReplaceLibrary(new LibraryIcons("imageres.dll", new LibraryStamp(1, 1), [1, 2]));
        store.ReplaceNames(IconCatalog.Terms(entries));
        return new IconBrowser(store, new SynonymTable([["folder", "directorio"]]), entries.ToDictionary(entry => entry.Icon));
    }

    [TestMethod]
    public void browses_the_all_tab_with_the_folders_first()
    {
        using var store = InMemoryStore.Open();

        CollectionAssert.AreEqual(new[] { Folder, Star }, Open(store).Find("  ", null).ToArray());
    }

    [TestMethod]
    public void keeps_the_identifiers_in_the_tab_of_a_library()
    {
        using var store = InMemoryStore.Open();

        CollectionAssert.AreEqual(new[] { Star, Folder }, Open(store).Find(null, "imageres.dll").ToArray());
    }

    [TestMethod]
    public void finds_an_icon_through_a_synonym()
    {
        using var store = InMemoryStore.Open();

        CollectionAssert.AreEqual(new[] { Folder }, Open(store).Find("direct", null).ToArray());
    }
}
