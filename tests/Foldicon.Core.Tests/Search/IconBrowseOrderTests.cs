// Tests of the order of the icons when nothing is being searched
using Foldicon.Core.Icons;
using Foldicon.Core.Search;

namespace Foldicon.Core.Tests.Search;

[TestClass]
public sealed class IconBrowseOrderTests
{
    private static readonly IconReference Picture = new("imageres.dll", 2);
    private static readonly IconReference Folder = new("imageres.dll", 3);
    private static readonly IconReference RepeatedFolder = new("imageres.dll", 4);
    private static readonly IconReference Blank = new("imageres.dll", 5);
    private static readonly IconReference MusicFolder = new("imageres.dll", 108);
    private static readonly IconReference ShellFolder = new("shell32.dll", 4);
    private static readonly IconReference ShellFile = new("shell32.dll", 1);
    private static readonly IconReference Unnamed = new("ddores.dll", 2001);

    private static readonly Dictionary<IconReference, IconCatalogEntry> Catalog = new[]
    {
        new IconCatalogEntry(Picture, "Picture", "Imagen", []),
        new IconCatalogEntry(Folder, "Folder", "Carpeta", []),
        new IconCatalogEntry(RepeatedFolder, "Folder (variant)", "Carpeta (variante)", ["duplicate", "duplicado"]),
        new IconCatalogEntry(Blank, "Blank", "En blanco", []),
        new IconCatalogEntry(MusicFolder, "Music folder", "Carpeta de música", []),
        new IconCatalogEntry(ShellFolder, "Folder", "Carpeta", []),
        new IconCatalogEntry(ShellFile, "Blank document", "Documento en blanco", []),
    }.ToDictionary(entry => entry.Icon);

    [TestMethod]
    public void starts_with_the_folders_of_the_first_library_and_ends_with_the_filler()
    {
        var sorted = IconBrowseOrder.Sort(
            [Unnamed, ShellFolder, ShellFile, Blank, RepeatedFolder, MusicFolder, Picture, Folder],
            Catalog);

        CollectionAssert.AreEqual(
            new[] { Folder, MusicFolder, Picture, ShellFile, ShellFolder, Unnamed, RepeatedFolder, Blank },
            sorted.ToArray());
    }

    [TestMethod]
    public void orders_the_embedded_catalog_from_the_first_folder()
    {
        var catalog = IconCatalog.LoadEmbedded().ToDictionary(entry => entry.Icon);
        var sorted = IconBrowseOrder.Sort(catalog.Keys, catalog);

        Assert.AreEqual(Folder, sorted[0]);
        Assert.IsTrue(catalog[sorted[1]].IsFolder);
        Assert.IsTrue(catalog[sorted[^1]].IsFiller);
    }
}
