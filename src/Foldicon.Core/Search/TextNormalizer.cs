// Function to turn a text into the lowercase words without accents that the index compares
using System.Globalization;
using System.Text;

namespace Foldicon.Core.Search;

public static class TextNormalizer
{
    // Splits the text into words in lowercase, without diacritics, so "Música" and "musica" match
    public static IReadOnlyList<string> Words(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return [];
        }

        // Drops the combining marks left after decomposing the text
        var decomposed = text.Normalize(NormalizationForm.FormD);
        var builder = new StringBuilder(decomposed.Length);
        foreach (var character in decomposed)
        {
            var category = CharUnicodeInfo.GetUnicodeCategory(character);
            if (category == UnicodeCategory.NonSpacingMark)
            {
                continue;
            }

            // Keeps letters and digits, and turns the rest into separators
            builder.Append(char.IsLetterOrDigit(character) ? char.ToLowerInvariant(character) : ' ');
        }

        // Returns the words without repetitions, in the order they were typed
        return builder.ToString()
            .Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .Distinct(StringComparer.Ordinal)
            .ToList();
    }
}
