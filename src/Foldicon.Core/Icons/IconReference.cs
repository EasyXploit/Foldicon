// Type that points to one icon group of a system library or of the library bundled with the app
namespace Foldicon.Core.Icons;

public readonly record struct IconReference(string Library, int ResourceId)
{
    // Writes the full path of the bundled library, or the path of a system library under %SystemRoot%\System32 without
    // expanding it
    public string LibraryPath() =>
        Library == BundledIconLibrary.Name ? BundledIconLibrary.FilePath : $@"%SystemRoot%\System32\{Library}";

    // Writes the value of IconResource for desktop.ini
    public string ToIconResource() => $"{LibraryPath()},-{ResourceId}";

    // Writes the short form shown to the user, as in "imageres.dll,-3"
    public override string ToString() => $"{Library},-{ResourceId}";
}
