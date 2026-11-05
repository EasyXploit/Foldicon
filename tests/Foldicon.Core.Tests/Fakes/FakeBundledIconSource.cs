// Source of bundled icons that returns the identifier as the bytes of the file, and nothing for the identifier 404
using Foldicon.Core.Icons;

namespace Foldicon.Core.Tests.Fakes;

internal sealed class FakeBundledIconSource : IBundledIconSource
{
    // Identifier of an icon that cannot be read
    public const int Missing = 404;

    public byte[]? ReadIconFile(int resourceId) => resourceId == Missing ? null : [(byte)resourceId];
}
