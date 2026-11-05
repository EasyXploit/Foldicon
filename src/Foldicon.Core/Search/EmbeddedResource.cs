// Function to open a file embedded in the library
namespace Foldicon.Core.Search;

internal static class EmbeddedResource
{
    // Returns the stream of the file, which every build embeds
    public static Stream Open(string name) =>
        typeof(EmbeddedResource).Assembly.GetManifestResourceStream(name)
            ?? throw new InvalidOperationException($"The resource {name} is not embedded.");
}
