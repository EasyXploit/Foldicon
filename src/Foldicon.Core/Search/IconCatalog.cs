// Catalog of the names given to the icons, which the window shows and the index stores as search words
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Foldicon.Core.Icons;

namespace Foldicon.Core.Search;

public static class IconCatalog
{
    // Loads the catalog embedded in the library
    public static IReadOnlyList<IconCatalogEntry> LoadEmbedded()
    {
        using var stream = EmbeddedResource.Open("Foldicon.Core.icon-names.json");
        using var reader = new StreamReader(stream);
        return Parse(reader.ReadToEnd());
    }

    // Reads a catalog shaped as { "imageres.dll": { "3": ["Folder", "Carpeta", "directory", "directorio"] } }: the
    // English name, the Spanish name and then the keywords
    public static IReadOnlyList<IconCatalogEntry> Parse(string json)
    {
        var catalog = JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, List<string>>>>(json) ?? [];
        var entries = new List<IconCatalogEntry>();
        foreach (var (library, icons) in catalog)
        {
            foreach (var (id, values) in icons)
            {
                // Skips identifiers that are not numbers, which IconResource cannot point to, and entries without
                // both names
                if (!int.TryParse(id, NumberStyles.None, CultureInfo.InvariantCulture, out var resourceId) || values.Count < 2)
                {
                    continue;
                }

                var icon = new IconReference(library.ToLowerInvariant(), resourceId);
                entries.Add(new IconCatalogEntry(icon, values[0], values[1], values.Skip(2).ToList()));
            }
        }

        return entries;
    }

    // Returns a value that changes with the search words of the entries, to store them again only when they change
    public static string Stamp(IEnumerable<IconCatalogEntry> entries)
    {
        var terms = Terms(entries).Select(name => $"{name.Icon}={name.Term}");
        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(string.Join('\n', terms))));
    }

    // Returns the search words of every entry, as the index stores them
    public static IEnumerable<(IconReference Icon, string Term)> Terms(IEnumerable<IconCatalogEntry> entries)
    {
        ArgumentNullException.ThrowIfNull(entries);
        return entries.SelectMany(entry => entry.Terms().Select(term => (entry.Icon, term)));
    }
}
