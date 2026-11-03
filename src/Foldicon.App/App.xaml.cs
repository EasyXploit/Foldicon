// Entry of the app: reads the folder of the command line and opens the picker on it
using Foldicon.Core.Launch;
using Microsoft.UI.Xaml;

namespace Foldicon.App;

public partial class App : Application
{
    private Window? window;

    public App()
    {
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
