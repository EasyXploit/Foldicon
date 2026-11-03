// Service that applies, undoes and restores the icon of a folder
using Foldicon.Core.Icons;

namespace Foldicon.Core.Folders;

public sealed class FolderIconService(IFolderShell shell, IDesktopIniFile desktopIni)
{
    // Creates the service for the real system
    public static FolderIconService ForSystem() => new(new NativeFolderShell(), new DesktopIniFile());

    // Returns the IconResource the folder has now, or null when it shows the default icon
    public string? GetCurrent(string folder)
    {
        var text = desktopIni.Read(folder);
        return text is null ? null : DesktopIniEditor.ReadValue(text, DesktopIniEditor.ShellClassInfo, DesktopIniEditor.IconResource);
    }

    // Applies the icon and returns the previous IconResource, so the change can be undone
    public string? Apply(string folder, IconReference icon)
    {
        // Notes the current icon before changing it
        var previous = GetCurrent(folder);

        // Writes the new icon and redraws the folder
        var value = IconResourceValue.Parse(icon.ToIconResource())!.Value;
        shell.SetIcon(folder, value.File, value.Index);
        shell.Refresh(folder);
        return previous;
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

        // Writes the previous icon again
        shell.SetIcon(folder, value.Value.File, value.Value.Index);
        shell.Refresh(folder);
    }

    // Removes only IconResource from the desktop.ini, and the file itself when nothing else is left in it
    public void Restore(string folder)
    {
        var text = desktopIni.Read(folder);
        if (text is not null)
        {
            // Drops the key and keeps every other setting of the folder
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

        // Redraws the folder in every case
        shell.Refresh(folder);
    }
}
