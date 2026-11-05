// App entry point: reads the folder from the command line and opens the picker on it
using System.Globalization;
using Foldicon.Core.Languages;
using Foldicon.Core.Launch;
using Microsoft.UI.Xaml;
using Microsoft.Windows.Globalization;

namespace Foldicon.App;

public partial class App : Application
{
    private Window? window;

    public App()
    {
        // Uses Spanish on any Spanish system and English on any other
        ApplicationLanguages.PrimaryLanguageOverride = AppLanguage.For(CultureInfo.CurrentUICulture.Name);

        InitializeComponent();
    }

    // Opens the picker on the folder given with --pick, or without a folder when it is missing
    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        var folder = LaunchArguments.ReadFolder(Environment.GetCommandLineArgs().Skip(1).ToList());
        window = new MainWindow(folder);
        window.Activate();
    }
}
