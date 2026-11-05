// Type that points to one icon group of a system library or of the library bundled with the app
namespace Foldicon.Core.Icons;

public readonly record struct IconReference(string Library, int ResourceId)
{
    // Checks whether the icon comes from the library bundled with the app
    public bool IsBundled => Library == BundledIconLibrary.Name;

    // Writes the full path of the bundled library, or the path of a system library under %SystemRoot%\System32 without
    // expanding it
    public string LibraryPath() => IsBundled ? BundledIconLibrary.FilePath : $@"%SystemRoot%\System32\{Library}";

    // Writes the short form shown to the user, as in "imageres.dll,-3"
    public override string ToString() => $"{Library},-{ResourceId}";
}
