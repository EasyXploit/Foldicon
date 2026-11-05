// Tests of the choice of the file that holds the icons of a library
using Foldicon.Core.Icons;
using Foldicon.Core.Indexing;

namespace Foldicon.Core.Tests.Indexing;

[TestClass]
public sealed class ResourceFileResolverTests
{
    [TestMethod]
    public void prefers_the_mun_file_when_it_exists()
    {
        var path = ResourceFileResolver.Resolve("imageres.dll", @"C:\Windows", file => file.EndsWith(".mun", StringComparison.Ordinal));
        Assert.AreEqual(@"C:\Windows\SystemResources\imageres.dll.mun", path);
    }

    [TestMethod]
    public void falls_back_to_system32_without_mun_file()
    {
        var path = ResourceFileResolver.Resolve("pifmgr.dll", @"C:\Windows", _ => false);
        Assert.AreEqual(@"C:\Windows\System32\pifmgr.dll", path);
    }

    [TestMethod]
    public void looks_for_the_bundled_library_next_to_the_app()
    {
        var path = ResourceFileResolver.Resolve(BundledIconLibrary.Name, @"C:\Windows", _ => true);
        Assert.AreEqual(BundledIconLibrary.FilePath, path);
    }
}
