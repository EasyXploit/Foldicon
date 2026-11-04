// Type with the names of one icon in English and Spanish and the keywords it can be searched by
using System.Globalization;
using Foldicon.Core.Icons;
using Foldicon.Core.Languages;

namespace Foldicon.Core.Search;

public sealed record IconCatalogEntry(IconReference Icon, string English, string Spanish, IReadOnlyList<string> Keywords)
{
    // Checks whether the icon shows a folder, from the words of its English name
    public bool IsFolder => TextNormalizer.Words(English).Contains("folder");

    // Checks whether the icon only repeats another one or shows nothing
    public bool IsFiller =>
        Keywords.Contains("duplicate") || English == "Blank" || English.StartsWith("Blank (", StringComparison.Ordinal);

    // Returns the name in the language the app chooses for the culture
    public string NameFor(CultureInfo culture)
    {
        ArgumentNullException.ThrowIfNull(culture);
        return AppLanguage.For(culture.Name) == AppLanguage.Spanish ? Spanish : English;
    }

    // Returns the normalized words of both names and of the keywords, once each
    public IEnumerable<string> Terms() =>
        new[] { English, Spanish }.Concat(Keywords).SelectMany(TextNormalizer.Words).Distinct(StringComparer.Ordinal);
}
