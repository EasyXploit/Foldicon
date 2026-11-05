// Function to order every icon for browsing, when nothing is being searched
using Foldicon.Core.Icons;
using Foldicon.Core.Indexing;

namespace Foldicon.Core.Search;

public static class IconBrowseOrder
{
    // Returns the folders of the bundled library and of the first system library, then each library in the order of
    // the tabs by identifier, and leaves the repeated and blank icons at the end
    public static IReadOnlyList<IconReference> Sort(
        IEnumerable<IconReference> icons,
        IReadOnlyDictionary<IconReference, IconCatalogEntry> catalog)
    {
        ArgumentNullException.ThrowIfNull(icons);
        ArgumentNullException.ThrowIfNull(catalog);
        return icons
            .OrderBy(icon => catalog.TryGetValue(icon, out var entry) && entry.IsFiller)
            .ThenBy(icon => !OpensTheList(icon, catalog))
            .ThenBy(icon => IconLibraryTabs.Rank(icon.Library))
            .ThenBy(icon => icon.ResourceId)
            .ToList();
    }

    // Checks whether the icon is a folder of one of the libraries that open the list
    private static bool OpensTheList(IconReference icon, IReadOnlyDictionary<IconReference, IconCatalogEntry> catalog) =>
        (icon.IsBundled || icon.Library == SystemIconLibraries.All[0])
        && catalog.TryGetValue(icon, out var entry) && entry.IsFolder;
}
