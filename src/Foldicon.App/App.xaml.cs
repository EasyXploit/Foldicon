// Entry of the app: reads the folder of the command line and opens the picker on it
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
        // Uses Spanish for any variant of that language of the system and English for any other
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
