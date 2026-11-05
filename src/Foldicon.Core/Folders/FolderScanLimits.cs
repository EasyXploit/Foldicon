// Limits of a search of customized folders
namespace Foldicon.Core.Folders;

public sealed record FolderScanLimits(int MaximumDepth, int MaximumFolders)
{
    // Search of every start of the app, which must stay cheap
    public static readonly FolderScanLimits Startup = new(6, 20_000);

    // Search that the user asks for, who expects it to take a while
    public static readonly FolderScanLimits Manual = new(32, 2_000_000);
}
