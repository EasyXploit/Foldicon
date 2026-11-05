// Interface to the bundled library icons, so the service can be tested without native code
namespace Foldicon.Core.Icons;

public interface IBundledIconSource
{
    // Returns the .ico file of an icon from the bundled library, or null when it cannot be read
    byte[]? ReadIconFile(int resourceId);
}
