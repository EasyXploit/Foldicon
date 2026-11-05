// Tests of the readers of icons on the libraries of the running Windows
using Foldicon.Core.Icons;
using Foldicon.Core.Imaging;
using Foldicon.Core.Indexing;
using Foldicon.Core.Tests.Fakes;

namespace Foldicon.Core.Tests.RealSystem;

[TestClass]
public sealed class NativeIconsTests
{
    // Folder icon, present in every version of the library
    private const int FolderIcon = 3;

    private static string Imageres =>
        ResourceFileResolver.Resolve("imageres.dll", Environment.GetFolderPath(Environment.SpecialFolder.Windows), File.Exists);

    [TestMethod]
    public void lists_the_icon_groups_of_a_library_in_order()
    {
        var ids = new NativeGroupIconReader().ReadGroupIconIds(Imageres);

        CollectionAssert.Contains(ids.ToList(), FolderIcon);
        CollectionAssert.AreEqual(ids.Order().ToList(), ids.ToList());
    }

    [TestMethod]
    public void lists_nothing_from_a_file_that_is_not_a_library()
    {
        using var folder = new TemporaryFolder();
        File.WriteAllText(folder.Inside("text.dll"), "text");

        Assert.IsEmpty(new NativeGroupIconReader().ReadGroupIconIds(folder.Inside("text.dll")));
        Assert.IsEmpty(new NativeGroupIconReader().ReadGroupIconIds(folder.Inside("missing.dll")));
    }

    [TestMethod]
    public void rebuilds_an_icon_file_from_a_library()
    {
        var file = new NativeBundledIconSource(Imageres).ReadIconFile(FolderIcon)!;

        Assert.AreEqual(1, BitConverter.ToUInt16(file, 2));
        Assert.IsGreaterThan(0, (int)BitConverter.ToUInt16(file, 4));
    }

    [TestMethod]
    public void rebuilds_nothing_from_a_missing_icon_or_library()
    {
        using var folder = new TemporaryFolder();

        Assert.IsNull(new NativeBundledIconSource(Imageres).ReadIconFile(int.MaxValue & 0xFFFF));
        Assert.IsNull(new NativeBundledIconSource(folder.Inside("missing.dll")).ReadIconFile(FolderIcon));
    }

    [TestMethod]
    public void reads_an_icon_as_pixels_of_the_requested_size()
    {
        var pixels = IconPixelReader.Read(new IconReference("imageres.dll", FolderIcon), 32)!;

        Assert.HasCount(32 * 32 * 4, pixels);
        Assert.IsTrue(IconPixels.HasAlpha(pixels));
    }

    [TestMethod]
    public void reads_no_pixels_from_a_library_that_does_not_exist()
    {
        Assert.IsNull(IconPixelReader.Read(new IconReference("missing.dll", FolderIcon), 32));
    }
}
