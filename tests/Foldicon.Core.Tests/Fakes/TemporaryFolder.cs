// Real folder for the tests of the code that talks to the disk and the shell, deleted with everything in it
using Microsoft.Data.Sqlite;

namespace Foldicon.Core.Tests.Fakes;

internal sealed class TemporaryFolder : IDisposable
{
    // Marks that stop a deletion
    private const FileAttributes Marks = FileAttributes.ReadOnly | FileAttributes.Hidden | FileAttributes.System;

    public string Path { get; } = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "foldicon-tests", Guid.NewGuid().ToString("N"));

    public TemporaryFolder() => Directory.CreateDirectory(Path);

    // Returns the path of a file or folder inside
    public string Inside(string name) => System.IO.Path.Combine(Path, name);

    // Drops every mark that would stop the deletion, and the connections that keep a cache open
    public void Dispose()
    {
        SqliteConnection.ClearAllPools();
        var root = new DirectoryInfo(Path);
        foreach (var entry in root.EnumerateFileSystemInfos("*", new EnumerationOptions { AttributesToSkip = 0, RecurseSubdirectories = true }))
        {
            entry.Attributes &= ~Marks;
        }

        root.Attributes &= ~Marks;
        root.Delete(true);
    }
}
