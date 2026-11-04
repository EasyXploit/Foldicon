// Function to choose the language of the app from the one of the system
namespace Foldicon.Core.Languages;

public static class AppLanguage
{
    // Languages of the strings of the app; English is the default one
    public const string English = "en-US";
    public const string Spanish = "es";

    // Returns Spanish for "es" and any of its variants, and English for any other language
    public static string For(string? cultureName)
    {
        var language = cultureName?.Split('-', '_')[0];
        return string.Equals(language, "es", StringComparison.OrdinalIgnoreCase) ? Spanish : English;
    }
}
