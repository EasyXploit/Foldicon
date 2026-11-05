// Interface to list a library file's icon groups, so the indexer can be tested without native code
namespace Foldicon.Core.Indexing;

public interface IGroupIconReader
{
    // Returns the numeric identifiers of every RT_GROUP_ICON of the file, or an empty list if it cannot be read
    IReadOnlyList<int> ReadGroupIconIds(string path);
}
