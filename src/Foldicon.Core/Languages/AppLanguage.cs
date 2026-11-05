// Function to choose the app language from the system language
namespace Foldicon.Core.Languages;

public static class AppLanguage
{
    // App string languages; English is the default one
    public const string English = "en-US";
    public const string Spanish = "es";

    // Translations by their language code, each with a folder in Strings; a new one adds its line here
    private static readonly Dictionary<string, string> Translations = new(StringComparer.OrdinalIgnoreCase)
    {
        ["en"] = English,
        ["es"] = Spanish,
    };

    // Returns the translation for a language and any of its variants, and English when there is none
    public static string For(string? cultureName)
    {
        var language = cultureName?.Split('-', '_')[0];
        return language is not null && Translations.TryGetValue(language, out var translation) ? translation : English;
    }
}
