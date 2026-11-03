// Function to open an icon index that lives only in memory, for the tests
using Foldicon.Core.Storage;

namespace Foldicon.Core.Tests.Fakes;

internal static class InMemoryStore
{
    // Opens a private in-memory database, dropped when the store is disposed
    public static IconIndexStore Open() => new("Data Source=:memory:");
}
