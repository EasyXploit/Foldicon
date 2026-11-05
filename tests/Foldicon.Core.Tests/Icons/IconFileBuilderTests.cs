// Tests of the reconstruction of an .ico file from the resources of a group
using Foldicon.Core.Icons;

namespace Foldicon.Core.Tests.Icons;

[TestClass]
public sealed class IconFileBuilderTests
{
    // Group of two images, 16 px with identifier 7 and 256 px with identifier 8
    private static readonly byte[] Group =
    [
        0, 0, 1, 0, 2, 0,
        16, 16, 0, 0, 1, 0, 32, 0, 3, 0, 0, 0, 7, 0,
        0, 0, 0, 0, 1, 0, 32, 0, 2, 0, 0, 0, 8, 0,
    ];

    private static byte[]? Images(int id) => id switch
    {
        7 => [0xA1, 0xA2, 0xA3],
        8 => [0xB1, 0xB2],
        _ => null,
    };

    [TestMethod]
    public void writes_the_directory_with_the_offset_of_each_image()
    {
        var file = IconFileBuilder.Build(Group, Images)!;

        Assert.HasCount(6 + 2 * 16 + 5, file);
        CollectionAssert.AreEqual(new byte[] { 0, 0, 1, 0, 2, 0 }, file[..6]);
        Assert.AreEqual(38, BitConverter.ToInt32(file, 6 + 12));
        Assert.AreEqual(41, BitConverter.ToInt32(file, 6 + 16 + 12));
    }

    [TestMethod]
    public void keeps_the_size_of_each_entry()
    {
        var file = IconFileBuilder.Build(Group, Images)!;

        CollectionAssert.AreEqual(new byte[] { 16, 16, 0, 0, 1, 0, 32, 0, 3, 0, 0, 0 }, file[6..18]);
    }

    [TestMethod]
    public void places_the_images_after_the_directory()
    {
        var file = IconFileBuilder.Build(Group, Images)!;

        CollectionAssert.AreEqual(new byte[] { 0xA1, 0xA2, 0xA3, 0xB1, 0xB2 }, file[38..]);
    }

    [TestMethod]
    public void returns_nothing_when_an_image_is_missing()
    {
        Assert.IsNull(IconFileBuilder.Build(Group, id => id == 7 ? [1] : null));
    }

    [TestMethod]
    public void returns_nothing_for_a_group_shorter_than_it_says()
    {
        Assert.IsNull(IconFileBuilder.Build(Group[..20], Images));
    }

    [TestMethod]
    public void returns_nothing_for_a_group_without_a_header_or_without_images()
    {
        Assert.IsNull(IconFileBuilder.Build([0, 0], Images));
        Assert.IsNull(IconFileBuilder.Build([0, 0, 1, 0, 0, 0], Images));
    }
}
