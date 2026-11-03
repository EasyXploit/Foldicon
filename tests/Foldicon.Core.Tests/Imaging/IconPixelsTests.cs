// Tests of the fixes of alpha of the pixels of an icon
using Foldicon.Core.Imaging;

namespace Foldicon.Core.Tests.Imaging;

[TestClass]
public sealed class IconPixelsTests
{
    [TestMethod]
    public void finds_alpha_in_any_pixel()
    {
        Assert.IsTrue(IconPixels.HasAlpha([0, 0, 0, 0, 9, 9, 9, 1]));
        Assert.IsFalse(IconPixels.HasAlpha([9, 9, 9, 0, 9, 9, 9, 0]));
    }

    [TestMethod]
    public void builds_alpha_from_the_mask()
    {
        byte[] pixels = [1, 2, 3, 0, 4, 5, 6, 0];
        IconPixels.ApplyMask(pixels, [0, 0, 0, 0, 255, 255, 255, 0]);
        CollectionAssert.AreEqual(new byte[] { 1, 2, 3, 255, 4, 5, 6, 0 }, pixels);
    }

    [TestMethod]
    public void makes_every_pixel_opaque_without_a_mask()
    {
        byte[] pixels = [1, 2, 3, 0];
        IconPixels.ApplyMask(pixels, null);
        Assert.AreEqual(255, pixels[3]);
    }

    [TestMethod]
    public void premultiplies_colors_by_alpha()
    {
        byte[] pixels = [200, 100, 50, 128, 10, 20, 30, 255, 10, 20, 30, 0];
        IconPixels.Premultiply(pixels);
        CollectionAssert.AreEqual(new byte[] { 100, 50, 25, 128, 10, 20, 30, 255, 0, 0, 0, 0 }, pixels);
    }
}
