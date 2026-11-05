// Shell of the real system, through SHGetSetFolderCustomSettings and SHChangeNotify
using System.Runtime.InteropServices;
using Foldicon.Core.Interop;

namespace Foldicon.Core.Folders;

public sealed class NativeFolderShell : IFolderShell
{
    // Writes IconResource with FCSM_ICONFILE, which keeps every other setting of the desktop.ini
    public void SetIcon(string folder, string iconFile, int iconIndex)
    {
        var file = Marshal.StringToHGlobalUni(iconFile);
        try
        {
            // Fills only the icon fields of the settings
            var settings = new ShFolderCustomSettings
            {
                Size = (uint)Marshal.SizeOf<ShFolderCustomSettings>(),
                Mask = Shell32.FcsmIconFile,
                IconFile = file,
                IconIndex = iconIndex,
            };

            // Raises every error of the shell, such as a denied access, as the one type the callers expect
            var result = Shell32.SHGetSetFolderCustomSettings(ref settings, folder, Shell32.FcsForceWrite);
            if (result < 0)
            {
                throw new IOException($"The shell refused the icon of {folder}.", result);
            }
        }
        finally
        {
            Marshal.FreeHGlobal(file);
        }
    }

    // Notifies the change of the folder without waiting for Explorer
    public void Refresh(string folder) =>
        Shell32.SHChangeNotify(Shell32.ShcneUpdateItem, Shell32.ShcnfPathW | Shell32.ShcnfFlushNoWait, folder, 0);
}
