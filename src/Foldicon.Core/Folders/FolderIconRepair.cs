// Service that repairs the folders whose Fluent icon lost its marks, or its desktop.ini, on the way through a sync client
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Foldicon.Core.Folders;

public sealed class FolderIconRepair(
    IFolderTree tree,
    IFolderIconFiles iconFiles,
    IDesktopIniFile desktopIni,
    IFolderMarks marks,
    IFolderShell shell)
{
    // Creates the service for the real system
    public static FolderIconRepair ForSystem() =>
        new(new FolderTree(), new FolderIconFiles(), new DesktopIniFile(), new FolderMarks(), new NativeFolderShell());

    // Returns the places a search starts from: the folder of the user, where the sync clients keep theirs by default
    public static IReadOnlyList<string> StartupRoots() =>
        [Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)];

    // Returns the same places and every fixed drive, for a sync folder that lives somewhere else
    public static IReadOnlyList<string> ManualRoots() =>
    [
        .. StartupRoots(),
        .. DriveInfo.GetDrives().Where(drive => drive.DriveType == DriveType.Fixed && drive.IsReady).Select(drive => drive.Name),
    ];

    // Runs the walk at low priority on a thread of its own, which ends with the process when the window closes
    public Task<int> RepairAllInBackground(IEnumerable<string> roots, FolderScanLimits limits) => Task.Factory.StartNew(
        () =>
        {
            Thread.CurrentThread.Priority = ThreadPriority.BelowNormal;
            return RepairAll(roots, limits);
        },
        CancellationToken.None,
        TaskCreationOptions.LongRunning,
        TaskScheduler.Default);

    // Walks the folders from each root, breadth first so the limit leaves out the deepest ones, and returns how many it
    // repaired; a folder that fails is skipped
    public int RepairAll(IEnumerable<string> roots, FolderScanLimits limits)
    {
        ArgumentNullException.ThrowIfNull(roots);
        ArgumentNullException.ThrowIfNull(limits);
        var repaired = 0;
        var visited = 0;
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var pending = new Queue<(string Folder, int Depth)>(roots.Select(root => (root, 0)));
        while (visited < limits.MaximumFolders && pending.TryDequeue(out var next))
        {
            // Skips a folder reached from two roots
            if (!seen.Add(next.Folder))
            {
                continue;
            }

            visited++;
            try
            {
                if (Repair(next.Folder))
                {
                    repaired++;
                }

                if (next.Depth < limits.MaximumDepth)
                {
                    foreach (var folder in tree.Subfolders(next.Folder))
                    {
                        pending.Enqueue((folder, next.Depth + 1));
                    }
                }
            }
            catch (Exception exception) when (exception is IOException or UnauthorizedAccessException or COMException)
            {
                Trace.TraceWarning($"The folder {next.Folder} could not be repaired: {exception.Message}");
            }
        }

        return repaired;
    }

    // Puts back what a folder with a copy of an icon needs, and returns whether anything was missing
    public bool Repair(string folder)
    {
        // Leaves alone a folder without copies, which is nearly all of them, without reading anything else
        var copies = iconFiles.List(folder);
        if (copies.Count == 0)
        {
            return false;
        }

        var current = DesktopIniEditor.ReadIconResource(desktopIni.Read(folder));
        var inUse = IconResourceValue.Parse(current)?.File;

        // Writes the icon again when the settings did not arrive, if the single copy says which one it was; a folder
        // that chose another icon keeps it
        if (current is null)
        {
            if (copies.Count != 1)
            {
                return false;
            }

            shell.SetIcon(folder, copies[0], 0);
            shell.Refresh(folder);
            return true;
        }

        if (inUse is null || !copies.Contains(inUse))
        {
            return false;
        }

        // Puts back each mark that was lost
        var changed = false;
        foreach (var name in new[] { DesktopIniFile.FileName, inUse })
        {
            if (!marks.IsHidden(folder, name))
            {
                changed |= marks.Hide(folder, name);
            }
        }

        if (!marks.IsCustomized(folder))
        {
            marks.MarkCustomized(folder);
            changed = true;
        }

        // Redraws the folder only when something changed
        if (changed)
        {
            shell.Refresh(folder);
        }

        return changed;
    }
}
