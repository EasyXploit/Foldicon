// Reader of icon groups that returns fixed identifiers per file and counts how many files it read
using Foldicon.Core.Indexing;

namespace Foldicon.Core.Tests.Fakes;

internal sealed class FakeGroupIconReader(Dictionary<string, int[]> idsByPath) : IGroupIconReader
{
    // Paths read so far, in order
    public List<string> Reads { get; } = [];

    // Returns the identifiers given for the path, or none
    public IReadOnlyList<int> ReadGroupIconIds(string path)
    {
        Reads.Add(path);
        return idsByPath.TryGetValue(path, out var ids) ? ids : [];
    }
}
