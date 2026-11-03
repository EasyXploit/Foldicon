// Type that groups the icons whose pixels are exactly the same
using System.Security.Cryptography;
using Foldicon.Core.Icons;

namespace Foldicon.Tools.IconExport;

internal sealed class DuplicateFinder
{
    private readonly Dictionary<string, List<IconReference>> byHash = [];

    // Notes the icon under the hash of its pixels
    public void Add(IconReference icon, byte[] pixels)
    {
        var hash = Convert.ToHexString(SHA256.HashData(pixels));
        if (!byHash.TryGetValue(hash, out var icons))
        {
            icons = [];
            byHash[hash] = icons;
        }

        icons.Add(icon);
    }

    // Returns one line per group of two or more identical icons
    public IEnumerable<string> Groups() =>
        byHash.Values.Where(icons => icons.Count > 1).Select(icons => string.Join(" = ", icons));
}
