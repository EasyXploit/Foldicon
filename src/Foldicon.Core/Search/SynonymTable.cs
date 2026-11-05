// Table of groups of equivalent words in Spanish and English, to find "carpeta" when an icon is named "folder"
using System.Text.Json;

namespace Foldicon.Core.Search;

public sealed class SynonymTable
{
    private readonly IReadOnlyList<IReadOnlyList<string>> groups;

    // Keeps the groups with their words already normalized
    public SynonymTable(IEnumerable<IEnumerable<string>> groups)
    {
        ArgumentNullException.ThrowIfNull(groups);
        this.groups = groups
            .Select(group => (IReadOnlyList<string>)group.SelectMany(TextNormalizer.Words).Distinct(StringComparer.Ordinal).ToList())
            .Where(group => group.Count > 0)
            .ToList();
    }

    // Loads the table embedded in the library
    public static SynonymTable LoadEmbedded()
    {
        using var stream = EmbeddedResource.Open("Foldicon.Core.synonyms.json");
        var groups = JsonSerializer.Deserialize<List<List<string>>>(stream) ?? [];
        return new SynonymTable(groups);
    }

    // Returns the word with every word of the groups that has one starting with it, so a word still being typed
    // ("carp") already brings its translation ("folder")
    public IReadOnlyList<string> Expand(string word)
    {
        var expanded = new List<string> { word };
        foreach (var group in groups)
        {
            if (group.Any(candidate => candidate.StartsWith(word, StringComparison.Ordinal)))
            {
                expanded.AddRange(group);
            }
        }

        return expanded.Distinct(StringComparer.Ordinal).ToList();
    }
}
