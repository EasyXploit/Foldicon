// Names the icon library that Foldicon carries next to its executable, and says where it is
namespace Foldicon.Core.Icons;

public static class BundledIconLibrary
{
    // Name of the file, in lowercase as the catalog keeps it; the search finds its icons by the word "fluent"
    public const string Name = "fluenticons.dll";

    // Returns the full path of the file, in the running app's folder
    public static string FilePath => Path.Combine(AppContext.BaseDirectory, Name);
}
