// Tests of the SQLite cache of icons and of its search
using Foldicon.Core.Icons;
using Foldicon.Core.Indexing;
using Foldicon.Core.Tests.Fakes;

namespace Foldicon.Core.Tests.Storage;

[TestClass]
public sealed class IconIndexStoreTests
{
    private static readonly LibraryStamp Stamp = new(100, 200);

    [TestMethod]
    public void returns_no_stamp_for_a_library_never_indexed()
    {
        using var store = InMemoryStore.Open();
        Assert.IsNull(store.GetStamp("imageres.dll"));
    }

    [TestMethod]
    public void keeps_the_stamp_of_the_last_read()
    {
        using var store = InMemoryStore.Open();
        store.ReplaceLibrary(new LibraryIcons("imageres.dll", Stamp, [3]));
        Assert.AreEqual(Stamp, store.GetStamp("imageres.dll"));
    }

    [TestMethod]
    public void replaces_the_previous_icons_of_a_library()
    {
        using var store = InMemoryStore.Open();
        store.ReplaceLibrary(new LibraryIcons("imageres.dll", Stamp, [3, 4]));
        store.ReplaceLibrary(new LibraryIcons("imageres.dll", Stamp, [5]));

        CollectionAssert.AreEqual(new[] { new IconReference("imageres.dll", 5) }, store.Search([], null).ToArray());
    }

    [TestMethod]
    public void narrows_the_search_to_one_library()
    {
        using var store = Seeded();
        var results = store.Search([], "shell32.dll");
        Assert.IsTrue(results.All(icon => icon.Library == "shell32.dll"));
        Assert.HasCount(2, results);
    }

    [TestMethod]
    public void finds_icons_by_the_start_of_a_word()
    {
        using var store = Seeded();
        var results = store.Search([["carp"]], null);
        CollectionAssert.AreEqual(new[] { new IconReference("imageres.dll", 3) }, results.ToArray());
    }

    [TestMethod]
    public void finds_icons_by_library_and_identifier()
    {
        using var store = Seeded();
        Assert.HasCount(2, store.Search([["shell32"]], null));
        CollectionAssert.AreEqual(new[] { new IconReference("shell32.dll", 4) }, store.Search([["shell"], ["4"]], null).ToArray());
    }

    [TestMethod]
    public void requires_every_group_of_words()
    {
        using var store = Seeded();
        Assert.IsEmpty(store.Search([["carpeta"], ["music"]], null));
    }

    [TestMethod]
    public void treats_wildcards_as_plain_text()
    {
        using var store = Seeded();
        Assert.IsEmpty(store.Search([["%"]], null));
        Assert.IsEmpty(store.Search([["_"]], null));
    }

    [TestMethod]
    public void replaces_every_search_word()
    {
        using var store = Seeded();
        store.ReplaceNames([(new IconReference("shell32.dll", 4), "music")]);
        Assert.IsEmpty(store.Search([["carpeta"]], null));
        Assert.HasCount(1, store.Search([["music"]], null));
    }

    // Opens a store with two libraries and the names of one icon
    private static Foldicon.Core.Storage.IconIndexStore Seeded()
    {
        var store = InMemoryStore.Open();
        store.ReplaceLibrary(new LibraryIcons("imageres.dll", Stamp, [3, 5]));
        store.ReplaceLibrary(new LibraryIcons("shell32.dll", Stamp, [4, 16802]));
        store.ReplaceNames([(new IconReference("imageres.dll", 3), "folder"), (new IconReference("imageres.dll", 3), "carpeta")]);
        return store;
    }
}
