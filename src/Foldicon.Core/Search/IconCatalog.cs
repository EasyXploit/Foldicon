// Catalog of the names given to known icons, which the index stores as search words
using System.Text.Json;
using Foldicon.Core.Icons;

namespace Foldicon.Core.Search;

public static class IconCatalog
{
    // Loads the catalog embedded in the library
    public static IReadOnlyList<(IconReference Icon, string Term)> LoadEmbedded()
    {
        using var stream = typeof(IconCatalog).Assembly.GetManifestResourceStream("Foldicon.Core.icon-names.json")
            ?? throw new InvalidOperationException("The icon catalog is not embedded.");
        using var reader = new StreamReader(stream);
        return Parse(reader.ReadToEnd());
    }

    // Reads a catalog shaped as { "imageres.dll": { "3": ["folder", "carpeta"] } } into normalized words
    public static IReadOnlyList<(IconReference Icon, string Term)> Parse(string json)
    {
        var catalog = JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, List<string>>>>(json) ?? [];
        var names = new List<(IconReference, string)>();
        foreach (var (library, icons) in catalog)
        {
            foreach (var (id, words) in icons)
            {
                // Skips identifiers that are not numbers, which IconResource cannot point to
                if (!int.TryParse(id, System.Globalization.NumberStyles.None, System.Globalization.CultureInfo.InvariantCulture, out var resourceId))
                {
                    continue;
                }

                // Stores each word once per icon
                var icon = new IconReference(library.ToLowerInvariant(), resourceId);
                foreach (var term in words.SelectMany(TextNormalizer.Words).Distinct(StringComparer.Ordinal))
                {
                    names.Add((icon, term));
                }
            }
        }

        return names;
    }
}
