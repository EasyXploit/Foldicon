// Tests of the walk of real folders
using System.Diagnostics;
using Foldicon.Core.Folders;
using Foldicon.Core.Tests.Fakes;

namespace Foldicon.Core.Tests.RealSystem;

[TestClass]
public sealed class FolderTreeTests
{
    private static readonly FolderTree Tree = new();

    [TestMethod]
    public void lists_system_folders_and_leaves_out_hidden_and_skipped_ones()
    {
        using var folder = new TemporaryFolder();
        var kept = Directory.CreateDirectory(folder.Inside("Photos"));
        kept.Attributes |= FileAttributes.System;
        Directory.CreateDirectory(folder.Inside("Secret")).Attributes |= FileAttributes.Hidden;
        Directory.CreateDirectory(folder.Inside("node_modules"));
        Directory.CreateDirectory(folder.Inside(".git"));

        CollectionAssert.AreEqual(new[] { kept.FullName }, Tree.Subfolders(folder.Path).ToArray());
    }

    [TestMethod]
    public void never_leaves_through_a_link()
    {
        using var folder = new TemporaryFolder();
        var target = Directory.CreateDirectory(folder.Inside("Target"));
        var link = folder.Inside("Link");
        RunCommand($"""mklink /J "{link}" "{target.FullName}" """);
        var folders = Tree.Subfolders(folder.Path);

        // Removes the link itself, which a recursive deletion refuses
        RunCommand($"""rmdir "{link}" """);
        CollectionAssert.AreEqual(new[] { target.FullName }, folders.ToArray());
    }

    // Runs a command prompt command, without a window
    private static void RunCommand(string command)
    {
        using var process = Process.Start(new ProcessStartInfo("cmd.exe", "/c " + command)
        {
            CreateNoWindow = true,
            UseShellExecute = false,
            RedirectStandardOutput = true,
        })!;
        process.WaitForExit();
        Assert.AreEqual(0, process.ExitCode);
    }
}
