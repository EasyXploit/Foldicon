// Service that answers the search box and the tabs of the window from the index
using Foldicon.Core.Icons;
using Foldicon.Core.Storage;

namespace Foldicon.Core.Search;

public sealed class IconBrowser(
    IconIndexStore store,
    SynonymTable synonyms,
    IReadOnlyDictionary<IconReference, IconCatalogEntry> catalog)
{
    // Returns the icons of a library, or of all of them when it is null, that match the typed text
    public IReadOnlyList<IconReference> Find(string? text, string? library)
    {
        // Runs the search with the synonyms of each typed word
        var query = SearchQuery.Parse(text, synonyms);
        var results = store.Search(query, library);

        // Browses the "All" tab in its own order while nothing is being searched
        return library is null && query.Count == 0 ? IconBrowseOrder.Sort(results, catalog) : results;
    }
}
