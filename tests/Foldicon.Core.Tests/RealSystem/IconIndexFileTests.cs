// Tests of the files of the index: the stamp of a library and a cache on disk
using Foldicon.Core.Icons;
using Foldicon.Core.Indexing;
using Foldicon.Core.Storage;
using Foldicon.Core.Tests.Fakes;

namespace Foldicon.Core.Tests.RealSystem;

[TestClass]
public sealed class IconIndexFileTests
{
    [TestMethod]
    public void stamps_a_file_by_its_size_and_write_time()
    {
        using var folder = new TemporaryFolder();
        var path = folder.Inside("library.dll");
        File.WriteAllBytes(path, [1, 2, 3]);
        var time = new DateTime(2026, 1, 2, 3, 4, 5, DateTimeKind.Utc);
        File.SetLastWriteTimeUtc(path, time);

        Assert.AreEqual(new LibraryStamp(3, time.Ticks), LibraryStamp.Of(path));
    }

    [TestMethod]
    public void places_the_cache_in_the_local_data_of_the_user()
    {
        var path = LocalData.IndexPath();

        Assert.StartsWith(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), path);
        Assert.EndsWith(@"Foldicon\index.db", path);
        Assert.AreEqual("settings.json", Path.GetFileName(LocalData.SettingsPath()));
        Assert.IsTrue(Directory.Exists(Path.GetDirectoryName(path)));
    }

    [TestMethod]
    public void rebuilds_a_damaged_cache()
    {
        using var folder = new TemporaryFolder();
        var path = folder.Inside("index.db");
        File.WriteAllText(path, new string('x', 8192));
        var icons = new LibraryIcons("shell32.dll", new LibraryStamp(1, 2), [4]);

        using var store = IconIndexStore.OpenPrepared(path, opened => opened.ReplaceLibrary(icons));

        Assert.HasCount(1, store.Search([], null));
    }

    [TestMethod]
    public void leaves_alone_a_failure_that_is_not_damage()
    {
        using var folder = new TemporaryFolder();

        Assert.ThrowsExactly<InvalidOperationException>(
            () => IconIndexStore.OpenPrepared(folder.Inside("index.db"), _ => throw new InvalidOperationException()));
    }

    [TestMethod]
    public void opens_a_cache_whose_path_holds_a_semicolon_and_keeps_it()
    {
        using var folder = new TemporaryFolder();
        var path = folder.Inside("a;b.db");
        var icons = new LibraryIcons("shell32.dll", new LibraryStamp(1, 2), [4]);

        using (var store = IconIndexStore.Open(path))
        {
            store.ReplaceLibrary(icons);
        }

        using var reopened = IconIndexStore.Open(path);
        CollectionAssert.AreEqual(new[] { new IconReference("shell32.dll", 4) }, reopened.Search([], null).ToArray());
    }
}
