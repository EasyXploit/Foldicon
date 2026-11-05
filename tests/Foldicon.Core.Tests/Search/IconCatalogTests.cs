// Tests of the catalog of icon names and of the search over the embedded one
using System.Globalization;
using System.Text.Json;
using Foldicon.Core.Icons;
using Foldicon.Core.Indexing;
using Foldicon.Core.Search;
using Foldicon.Core.Storage;
using Foldicon.Core.Tests.Fakes;

namespace Foldicon.Core.Tests.Search;

[TestClass]
public sealed class IconCatalogTests
{
    [TestMethod]
    public void reads_names_and_keywords_per_icon()
    {
        var entry = IconCatalog.Parse("""{ "ImageRes.dll": { "3": ["Folder", "Carpeta", "directory"] } }""").Single();

        Assert.AreEqual(new IconReference("imageres.dll", 3), entry.Icon);
        Assert.AreEqual("Folder", entry.English);
        Assert.AreEqual("Carpeta", entry.Spanish);
        CollectionAssert.AreEqual(new[] { "directory" }, entry.Keywords.ToArray());
    }

    [TestMethod]
    public void skips_identifiers_that_are_not_numbers_and_entries_without_both_names()
    {
        Assert.IsEmpty(IconCatalog.Parse("""{ "shell32.dll": { "FOLDER": ["Folder", "Carpeta"], "-3": ["Folder", "Carpeta"], "4": ["Folder"] } }"""));
    }

    [TestMethod]
    public void names_the_icon_in_the_language_of_the_culture()
    {
        var entry = new IconCatalogEntry(new IconReference("shell32.dll", 4), "Folder", "Carpeta", []);

        Assert.AreEqual("Carpeta", entry.NameFor(CultureInfo.GetCultureInfo("es-ES")));
        Assert.AreEqual("Folder", entry.NameFor(CultureInfo.GetCultureInfo("en-US")));
        Assert.AreEqual("Folder", entry.NameFor(CultureInfo.GetCultureInfo("de-DE")));
    }

    [TestMethod]
    public void builds_search_words_from_names_and_keywords()
    {
        var entry = new IconCatalogEntry(new IconReference("imageres.dll", 108), "Music folder", "Carpeta de música", ["audio"]);
        CollectionAssert.AreEquivalent(
            new[] { "music", "folder", "carpeta", "de", "musica", "audio" },
            entry.Terms().ToArray());
    }

    [TestMethod]
    public void names_every_icon_of_the_embedded_catalog_in_both_languages()
    {
        var entries = IconCatalog.LoadEmbedded();

        Assert.IsGreaterThan(1400, entries.Count);
        Assert.IsTrue(entries.All(entry => entry.English.Length > 0 && entry.Spanish.Length > 0));
        Assert.HasCount(entries.Count, entries.Select(entry => entry.Icon).Distinct().ToList());
    }

    [TestMethod]
    public void names_every_icon_of_the_fluent_pack_and_nothing_else()
    {
        // Reads the identifiers in the list the icon library is built from
        using var stream = typeof(IconCatalogTests).Assembly.GetManifestResourceStream("Foldicon.Core.Tests.pack.json")!;
        using var pack = JsonDocument.Parse(stream);
        var ids = pack.RootElement.GetProperty("icons").EnumerateArray().Select(icon => icon.GetProperty("id").GetInt32());

        var named = IconCatalog.LoadEmbedded().Where(entry => entry.Icon.Library == BundledIconLibrary.Name);

        CollectionAssert.AreEquivalent(ids.ToArray(), named.Select(entry => entry.Icon.ResourceId).ToArray());
    }

    [TestMethod]
    public void finds_the_fluent_pack_by_its_name()
    {
        using var store = EmbeddedStore();

        var found = Search(store, "fluent");

        Assert.IsNotEmpty(found);
        Assert.IsTrue(found.All(icon => icon.Library == BundledIconLibrary.Name));
    }

    [TestMethod]
    public void finds_a_fluent_folder_in_both_languages_before_those_of_the_system()
    {
        using var store = EmbeddedStore();

        Assert.AreEqual(new IconReference(BundledIconLibrary.Name, 1), Search(store, "folder")[0]);
        Assert.AreEqual(new IconReference(BundledIconLibrary.Name, 1), Search(store, "carpeta")[0]);
    }

    [TestMethod]
    public void finds_music_in_both_languages()
    {
        using var store = EmbeddedStore();
        foreach (var text in new[] { "música", "music", "MUSICA" })
        {
            var results = Search(store, text);
            CollectionAssert.Contains(results, new IconReference("imageres.dll", 108), text);
            CollectionAssert.Contains(results, new IconReference("shell32.dll", 237), text);
            CollectionAssert.DoesNotContain(results, new IconReference("imageres.dll", 3), text);
        }
    }

    [TestMethod]
    public void finds_usb_drives()
    {
        using var store = EmbeddedStore();
        var results = Search(store, "usb");

        CollectionAssert.Contains(results, new IconReference("ddores.dll", 2526));
        CollectionAssert.Contains(results, new IconReference("imageres.dll", 33));
        CollectionAssert.DoesNotContain(results, new IconReference("imageres.dll", 108));
    }

    [TestMethod]
    public void finds_the_recycle_bin_in_spanish()
    {
        using var store = EmbeddedStore();
        var results = Search(store, "papelera");

        CollectionAssert.Contains(results, new IconReference("imageres.dll", 54));
        CollectionAssert.Contains(results, new IconReference("imageres.dll", 55));
        CollectionAssert.DoesNotContain(results, new IconReference("imageres.dll", 3));
    }

    // Opens an in-memory index with every icon in the embedded catalog and its search words
    private static IconIndexStore EmbeddedStore()
    {
        var store = InMemoryStore.Open();
        var entries = IconCatalog.LoadEmbedded();
        foreach (var library in entries.GroupBy(entry => entry.Icon.Library))
        {
            store.ReplaceLibrary(new LibraryIcons(library.Key, new LibraryStamp(1, 1), library.Select(entry => entry.Icon.ResourceId).ToList()));
        }

        store.ReplaceNames(IconCatalog.Terms(entries));
        return store;
    }

    // Runs the window's search for a text
    private static List<IconReference> Search(IconIndexStore store, string text) =>
        store.Search(SearchQuery.Parse(text, SynonymTable.LoadEmbedded()), null).ToList();
}
