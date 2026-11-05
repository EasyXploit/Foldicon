// Tests of the file of the settings
using Foldicon.Core.Storage;
using Foldicon.Core.Tests.Fakes;

namespace Foldicon.Core.Tests.RealSystem;

[TestClass]
public sealed class AppSettingsFileTests
{
    [TestMethod]
    public void repairs_at_startup_by_default_without_a_file()
    {
        using var folder = new TemporaryFolder();

        Assert.AreEqual(new AppSettings(true, false), AppSettingsFile.Load(folder.Inside("settings.json")));
    }

    [TestMethod]
    public void keeps_the_settings()
    {
        using var folder = new TemporaryFolder();
        var path = folder.Inside("settings.json");
        var settings = new AppSettings(RepairAtStartup: false, RepairNoticeShown: true);

        AppSettingsFile.Save(path, settings);

        Assert.AreEqual(settings, AppSettingsFile.Load(path));
    }

    [TestMethod]
    public void falls_back_to_the_defaults_with_a_damaged_file()
    {
        using var folder = new TemporaryFolder();
        var path = folder.Inside("settings.json");
        File.WriteAllText(path, "{");

        Assert.AreEqual(new AppSettings(), AppSettingsFile.Load(path));
    }

    [TestMethod]
    public void only_traces_a_failed_write()
    {
        using var folder = new TemporaryFolder();

        AppSettingsFile.Save(folder.Inside(@"missing\settings.json"), new AppSettings());

        Assert.IsFalse(File.Exists(folder.Inside(@"missing\settings.json")));
    }
}
