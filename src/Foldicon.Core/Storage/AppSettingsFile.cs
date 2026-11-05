// Functions to read and write the settings, whose loss only brings back the default ones
using System.Diagnostics;
using System.Text.Json;

namespace Foldicon.Core.Storage;

public static class AppSettingsFile
{
    // Returns the settings of the file, or the default ones when it is missing or damaged
    public static AppSettings Load(string path)
    {
        try
        {
            return File.Exists(path) ? JsonSerializer.Deserialize<AppSettings>(File.ReadAllText(path)) ?? new() : new();
        }
        catch (Exception exception) when (exception is JsonException or IOException or UnauthorizedAccessException)
        {
            Trace.TraceWarning($"The settings could not be read: {exception.Message}");
            return new();
        }
    }

    // Writes the settings, and only traces a failure, since the app works the same without them
    public static void Save(string path, AppSettings settings)
    {
        try
        {
            File.WriteAllText(path, JsonSerializer.Serialize(settings));
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
        {
            Trace.TraceWarning($"The settings could not be written: {exception.Message}");
        }
    }
}
