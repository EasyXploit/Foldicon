// Tests of the choice of the language of the app
using Foldicon.Core.Languages;

namespace Foldicon.Core.Tests.Languages;

[TestClass]
public sealed class AppLanguageTests
{
    [TestMethod]
    [DataRow("es")]
    [DataRow("es-ES")]
    [DataRow("es-MX")]
    [DataRow("es-419")]
    [DataRow("ES-ar")]
    public void chooses_spanish_for_any_variant_of_spanish(string culture)
    {
        Assert.AreEqual(AppLanguage.Spanish, AppLanguage.For(culture));
    }

    [TestMethod]
    [DataRow("EN-gb")]
    public void reads_the_language_code_in_any_case(string culture)
    {
        Assert.AreEqual(AppLanguage.English, AppLanguage.For(culture));
    }

    [TestMethod]
    [DataRow("en-US")]
    [DataRow("en-GB")]
    [DataRow("de-DE")]
    [DataRow("fr-FR")]
    [DataRow("est-EE")]
    [DataRow("")]
    [DataRow(null)]
    public void chooses_english_for_any_other_language(string? culture)
    {
        Assert.AreEqual(AppLanguage.English, AppLanguage.For(culture));
    }
}
