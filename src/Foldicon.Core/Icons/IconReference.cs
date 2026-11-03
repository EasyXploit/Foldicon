// Type that points to one icon group of a system library
namespace Foldicon.Core.Icons;

public readonly record struct IconReference(string Library, int ResourceId)
{
    // Writes the value of IconResource for desktop.ini, with the library under %SystemRoot%\System32
    public string ToIconResource() => $@"%SystemRoot%\System32\{Library},-{ResourceId}";

    // Writes the short form shown to the user, as in "imageres.dll,-3"
    public override string ToString() => $"{Library},-{ResourceId}";
}
