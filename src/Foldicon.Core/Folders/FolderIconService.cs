// Service that applies, undoes and restores the icon of a folder
using Foldicon.Core.Icons;

namespace Foldicon.Core.Folders;

public sealed class FolderIconService(
    IFolderShell shell,
    IDesktopIniFile desktopIni,
    IFolderIconFiles iconFiles,
    IBundledIconSource bundledIcons)
{
    // Copy removed by the last restore, kept in memory for Undo
    private (string Name, byte[] Icon)? removedCopy;

    // Creates the service for the real system
    public static FolderIconService ForSystem() =>
        new(new NativeFolderShell(), new DesktopIniFile(), new FolderIconFiles(), new NativeBundledIconSource(BundledIconLibrary.FilePath));

    // Returns the IconResource the folder has now, or null when it shows the default icon
    public string? GetCurrent(string folder) => DesktopIniEditor.ReadIconResource(desktopIni.Read(folder));

    // Applies the icon and returns the previous IconResource, so the change can be undone
    public string? Apply(string folder, IconReference icon)
    {
        // Notes the current icon before changing it
        var previous = GetCurrent(folder);

        // Writes the new icon, leaves only its own copy, if any, and redraws the folder
        var kept = icon.IsBundled ? ApplyCopy(folder, icon.ResourceId) : ApplySystemIcon(folder, icon);
        DeleteCopies(folder, kept);
        shell.Refresh(folder);
        return previous;
    }

    // Points the folder to a system icon, which every copy of Windows has
    private string? ApplySystemIcon(string folder, IconReference icon)
    {
        shell.SetIcon(folder, icon.LibraryPath(), -icon.ResourceId);
        return null;
    }

    // Keeps a copy of a bundled icon inside the folder and points to it by its bare name, so the folder keeps its icon
    // when it moves to another PC and when the app is updated or removed; returns the name of the copy
    private string ApplyCopy(string folder, int resourceId)
    {
        var icon = bundledIcons.ReadIconFile(resourceId)
            ?? throw new IOException($"The bundled icon {resourceId} cannot be read.");
        var name = FolderIconCopyName.For(resourceId);
        var existed = iconFiles.List(folder).Contains(name, FolderIconCopyName.Comparer);
        iconFiles.Write(folder, name, icon);
        try
        {
            shell.SetIcon(folder, name, 0);
        }
        catch
        {
            // Leaves the folder as it was when the shell refuses the change
            if (!existed)
            {
                iconFiles.Delete(folder, name);
            }

            throw;
        }

        return name;
    }

    // Deletes the folder's copies, except for the one in use
    private void DeleteCopies(string folder, string? kept)
    {
        foreach (var name in iconFiles.List(folder).Where(name => !FolderIconCopyName.Comparer.Equals(name, kept)))
        {
            iconFiles.Delete(folder, name);
        }
    }

    // Puts back the IconResource the folder had before the last change, or the default icon when it had none
    public void Undo(string folder, string? previous)
    {
        // Falls back to the default icon when there was none, or when the previous value cannot be read
        var value = IconResourceValue.Parse(previous);
        if (value is null)
        {
            Restore(folder);
            return;
        }

        // Brings back the copy the previous icon needs
        if (removedCopy is { } copy && FolderIconCopyName.Comparer.Equals(copy.Name, value.Value.File))
        {
            iconFiles.Write(folder, copy.Name, copy.Icon);
        }

        // Writes the previous icon again
        shell.SetIcon(folder, value.Value.File, value.Value.Index);
        shell.Refresh(folder);
    }

    // Removes IconResource from the desktop.ini, the file itself when nothing else is left in it, and the copies of the
    // icon, keeping the one in use in memory for Undo
    public void Restore(string folder)
    {
        var inUse = IconResourceValue.Parse(GetCurrent(folder))?.File;
        var kept = inUse is not null && FolderIconCopyName.IsCopy(inUse) ? iconFiles.Read(folder, inUse) : null;
        removedCopy = kept is null ? null : (inUse!, kept);

        var text = desktopIni.Read(folder);
        if (text is not null)
        {
            // Drops the key and keeps the folder's other settings
            var remaining = DesktopIniEditor.RemoveKey(text, DesktopIniEditor.ShellClassInfo, DesktopIniEditor.IconResource);
            if (DesktopIniEditor.IsEmpty(remaining))
            {
                desktopIni.Delete(folder);
            }
            else if (remaining != text)
            {
                desktopIni.Write(folder, remaining);
            }
        }

        DeleteCopies(folder, null);

        // Redraws the folder in every case
        shell.Refresh(folder);
    }
}
