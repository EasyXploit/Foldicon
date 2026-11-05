// Interface to the icons of the bundled library, so the service can be tested without native code
namespace Foldicon.Core.Icons;

public interface IBundledIconSource
{
    // Returns the .ico file of an icon of the bundled library, or null when it cannot be read
    byte[]? ReadIconFile(int resourceId);
}
