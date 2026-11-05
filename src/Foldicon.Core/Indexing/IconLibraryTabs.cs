// Order of the icon libraries in the tabs of the interface, which the lists of icons also follow
using Foldicon.Core.Icons;

namespace Foldicon.Core.Indexing;

public static class IconLibraryTabs
{
    // Holds every library offered to the user, after the "All" tab: the bundled one and then those of the system
    public static readonly IReadOnlyList<string> All = [BundledIconLibrary.Name, .. SystemIconLibraries.All];

    // Returns the position of a library among the tabs, and places any other one after them
    public static int Rank(string library)
    {
        var rank = All.ToList().IndexOf(library);
        return rank < 0 ? int.MaxValue : rank;
    }
}
