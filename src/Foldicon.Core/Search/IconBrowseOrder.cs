// Function to order every icon for browsing, when nothing is being searched
using Foldicon.Core.Icons;
using Foldicon.Core.Indexing;

namespace Foldicon.Core.Search;

public static class IconBrowseOrder
{
    // Returns the folders of the first library, then each library in the order of the tabs by identifier, and leaves
    // the repeated and blank icons at the end
    public static IReadOnlyList<IconReference> Sort(
        IEnumerable<IconReference> icons,
        IReadOnlyDictionary<IconReference, IconCatalogEntry> catalog)
    {
        ArgumentNullException.ThrowIfNull(icons);
        ArgumentNullException.ThrowIfNull(catalog);
        return icons
            .OrderBy(icon => catalog.TryGetValue(icon, out var entry) && entry.IsFiller)
            .ThenBy(icon => SystemIconLibraries.Rank(icon.Library))
            .ThenBy(icon => icon.Library != SystemIconLibraries.All[0] || !catalog.TryGetValue(icon, out var entry) || !entry.IsFolder)
            .ThenBy(icon => icon.ResourceId)
            .ToList();
    }
}
