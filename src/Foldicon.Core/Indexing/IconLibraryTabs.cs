// Order of the icon libraries in the tabs of the interface, which the lists of icons also follow
using Foldicon.Core.Icons;

namespace Foldicon.Core.Indexing;

public static class IconLibraryTabs
{
    // Holds every library offered to the user, after the "All" tab: the bundled one and then those of the system
    public static readonly IReadOnlyList<string> All = [BundledIconLibrary.Name, .. SystemIconLibraries.All];

    // Position of each library among the tabs
    private static readonly Dictionary<string, int> Ranks = All.Select((library, rank) => (library, rank)).ToDictionary();

    // Returns the position of a library among the tabs, and places any other one after them
    public static int Rank(string library) => Ranks.GetValueOrDefault(library, int.MaxValue);
}
