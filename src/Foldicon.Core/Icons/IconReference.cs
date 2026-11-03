// Type that points to one icon group of a system library
namespace Foldicon.Core.Icons;

public readonly record struct IconReference(string Library, int ResourceId)
{
    // Writes the path of the library under %SystemRoot%\System32, without expanding it
    public string LibraryPath() => $@"%SystemRoot%\System32\{Library}";

    // Writes the value of IconResource for desktop.ini
    public string ToIconResource() => $"{LibraryPath()},-{ResourceId}";

    // Writes the short form shown to the user, as in "imageres.dll,-3"
    public override string ToString() => $"{Library},-{ResourceId}";
}
