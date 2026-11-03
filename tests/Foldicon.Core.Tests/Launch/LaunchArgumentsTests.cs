// Tests of the reading of the folder from the command line
using Foldicon.Core.Launch;

namespace Foldicon.Core.Tests.Launch;

[TestClass]
public sealed class LaunchArgumentsTests
{
    [TestMethod]
    public void reads_the_folder_after_pick()
    {
        Assert.AreEqual(@"C:\Photos\Trips", LaunchArguments.ReadFolder(["--pick", @"C:\Photos\Trips"]));
    }

    [TestMethod]
    public void ignores_case_and_other_arguments()
    {
        Assert.AreEqual(@"C:\Photos", LaunchArguments.ReadFolder(["-x", "--PICK", @"""C:\Photos\"""]));
    }

    [TestMethod]
    public void drops_the_trailing_separator_except_on_a_root()
    {
        Assert.AreEqual(@"C:\Photos", LaunchArguments.ReadFolder(["--pick", @"C:\Photos\"]));
        Assert.AreEqual(@"C:\", LaunchArguments.ReadFolder(["--pick", @"C:\"]));
    }

    [TestMethod]
    public void returns_null_without_a_folder()
    {
        Assert.IsNull(LaunchArguments.ReadFolder([]));
        Assert.IsNull(LaunchArguments.ReadFolder(["--pick"]));
        Assert.IsNull(LaunchArguments.ReadFolder(["--pick", " "]));
    }
}
