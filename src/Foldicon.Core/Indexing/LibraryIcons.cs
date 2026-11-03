// Type with the icons found in one library and the stamp of the file they were read from
namespace Foldicon.Core.Indexing;

public sealed record LibraryIcons(string Library, LibraryStamp Stamp, IReadOnlyList<int> ResourceIds);
