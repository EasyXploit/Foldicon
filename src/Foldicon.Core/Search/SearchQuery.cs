// Function to turn the search box text into the groups of words the index looks for
namespace Foldicon.Core.Search;

public static class SearchQuery
{
    // Most words in a search, far above any real one, since a pasted text would exceed what SQLite can run
    public const int MaximumWords = 32;

    // Returns one group per typed word, holding the word and its synonyms; an icon must match every group
    public static IReadOnlyList<IReadOnlyList<string>> Parse(string? text, SynonymTable synonyms)
    {
        ArgumentNullException.ThrowIfNull(synonyms);
        return TextNormalizer.Words(text).Take(MaximumWords).Select(synonyms.Expand).ToList();
    }
}
