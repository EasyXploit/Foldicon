// "Change icon" command of the folder context menu
#pragma once

#include <windows.h>
#include <shobjidl_core.h>
#include <winrt/base.h>

namespace Foldicon
{
    // Class identifier, the same one registered in Package.appxmanifest
    struct __declspec(uuid("8fce7177-7f17-4d80-b255-7036dd73ebbd")) ExplorerCommand
        : winrt::implements<ExplorerCommand, IExplorerCommand>
    {
        // Returns the item text, in the user's language
        IFACEMETHODIMP GetTitle(IShellItemArray* items, LPWSTR* name) override;

        // Returns the item icon: the app's own
        IFACEMETHODIMP GetIcon(IShellItemArray* items, LPWSTR* icon) override;

        IFACEMETHODIMP GetToolTip(IShellItemArray* items, LPWSTR* infoTip) override;
        IFACEMETHODIMP GetCanonicalName(GUID* commandName) override;

        // Shows the item only for a single file system folder
        IFACEMETHODIMP GetState(IShellItemArray* items, BOOL okToBeSlow, EXPCMDSTATE* state) override;

        // Starts the app on the selected folder
        IFACEMETHODIMP Invoke(IShellItemArray* items, IBindCtx* context) override;

        IFACEMETHODIMP GetFlags(EXPCMDFLAGS* flags) override;
        IFACEMETHODIMP EnumSubCommands(IEnumExplorerCommand** commands) override;
    };
}
