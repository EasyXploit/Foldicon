// Function to turn the text of the search box into the groups of words the index looks for
namespace Foldicon.Core.Search;

public static class SearchQuery
{
    // Returns one group per typed word, holding the word and its synonyms; an icon must match every group
    public static IReadOnlyList<IReadOnlyList<string>> Parse(string? text, SynonymTable synonyms)
    {
        ArgumentNullException.ThrowIfNull(synonyms);
        return TextNormalizer.Words(text).Select(synonyms.Expand).ToList();
    }
}
