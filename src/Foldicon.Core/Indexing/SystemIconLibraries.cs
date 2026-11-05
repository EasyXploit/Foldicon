// List of the system libraries whose icons are offered to the user
namespace Foldicon.Core.Indexing;

public static class SystemIconLibraries
{
    // Keeps the order of their tabs in the interface
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
}
