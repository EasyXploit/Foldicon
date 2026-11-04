// List of the system libraries whose icons are offered to the user
namespace Foldicon.Core.Indexing;

public static class SystemIconLibraries
{
    // Keeps the order of the tabs of the interface, after the "All" tab
    public static readonly IReadOnlyList<string> All =
    [
        "imageres.dll",
        "shell32.dll",
        "ddores.dll",
        "pifmgr.dll",
        "moricons.dll",
        "wmploc.dll",
        "netshell.dll",
        "mmcndmgr.dll",
        "setupapi.dll",
    ];

    // Returns the position of a library among the tabs, and places any other one after them
    public static int Rank(string library)
    {
        var rank = All.ToList().IndexOf(library);
        return rank < 0 ? int.MaxValue : rank;
    }
}
