// Layout of SHFOLDERCUSTOMSETTINGS, the settings of desktop.ini read and written by the shell
using System.Runtime.InteropServices;

namespace Foldicon.Core.Interop;

[StructLayout(LayoutKind.Sequential)]
internal struct ShFolderCustomSettings
{
    internal uint Size;
    internal uint Mask;
    internal nint ViewId;
    internal nint WebViewTemplate;
    internal uint WebViewTemplateLength;
    internal nint WebViewTemplateVersion;
    internal nint InfoTip;
    internal uint InfoTipLength;
    internal nint ClassId;
    internal uint Flags;
    internal nint IconFile;
    internal uint IconFileLength;
    internal int IconIndex;
    internal nint Logo;
    internal uint LogoLength;
}
