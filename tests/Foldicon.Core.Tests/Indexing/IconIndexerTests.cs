// Tests of the refresh of the cached index from the library files
using Foldicon.Core.Icons;
using Foldicon.Core.Indexing;
using Foldicon.Core.Tests.Fakes;

namespace Foldicon.Core.Tests.Indexing;

[TestClass]
public sealed class IconIndexerTests
{
    private const string Windows = @"C:\Windows";
    private const string Imageres = @"C:\Windows\SystemResources\imageres.dll.mun";
    private const string Pifmgr = @"C:\Windows\System32\pifmgr.dll";

    [TestMethod]
    public void indexes_the_libraries_that_exist()
    {
        using var store = InMemoryStore.Open();
        var reader = new FakeGroupIconReader(new() { [Imageres] = [3, 4], [Pifmgr] = [1] });
        var indexer = new IconIndexer(reader, store, Windows, path => path is Imageres or Pifmgr, _ => new LibraryStamp(1, 1));

        var read = indexer.Refresh(["imageres.dll", "pifmgr.dll", "ddores.dll"]);

        Assert.AreEqual(2, read);
        Assert.HasCount(3, store.Search([], null));
    }

    [TestMethod]
    public void indexes_the_bundled_library_from_the_folder_of_the_app()
    {
        using var store = InMemoryStore.Open();
        var reader = new FakeGroupIconReader(new() { [BundledIconLibrary.FilePath] = [1, 2] });
        var indexer = new IconIndexer(reader, store, Windows, path => path == BundledIconLibrary.FilePath, _ => new LibraryStamp(1, 1));

        var read = indexer.Refresh(IconLibraryTabs.All);

        Assert.AreEqual(1, read);
        CollectionAssert.AreEqual(
            new[] { new IconReference(BundledIconLibrary.Name, 1), new IconReference(BundledIconLibrary.Name, 2) },
            store.Search([], BundledIconLibrary.Name).ToArray());
    }

    [TestMethod]
    public void skips_the_bundled_library_when_the_app_lacks_it()
    {
        using var store = InMemoryStore.Open();
        var indexer = new IconIndexer(new FakeGroupIconReader([]), store, Windows, _ => false, _ => new LibraryStamp(1, 1));

        Assert.AreEqual(0, indexer.Refresh([BundledIconLibrary.Name]));
    }

    [TestMethod]
    public void skips_libraries_whose_file_did_not_change()
    {
        using var store = InMemoryStore.Open();
        var reader = new FakeGroupIconReader(new() { [Pifmgr] = [1] });
        var indexer = new IconIndexer(reader, store, Windows, path => path is Pifmgr, _ => new LibraryStamp(10, 20));

        indexer.Refresh(["pifmgr.dll"]);
        var read = indexer.Refresh(["pifmgr.dll"]);

        Assert.AreEqual(0, read);
        Assert.HasCount(1, reader.Reads);
    }

    [TestMethod]
    public void reads_again_a_library_whose_file_changed()
    {
        using var store = InMemoryStore.Open();
        var stamp = new LibraryStamp(10, 20);
        var reader = new FakeGroupIconReader(new() { [Pifmgr] = [1] });
        var indexer = new IconIndexer(reader, store, Windows, path => path is Pifmgr, _ => stamp);

        indexer.Refresh(["pifmgr.dll"]);
        stamp = new LibraryStamp(10, 30);
        var read = indexer.Refresh(["pifmgr.dll"]);

        Assert.AreEqual(1, read);
    }
}
