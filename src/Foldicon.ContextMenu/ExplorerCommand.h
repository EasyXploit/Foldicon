// Command "Change folder icon..." of the context menu of folders
#pragma once

#include <windows.h>
#include <shobjidl_core.h>
#include <winrt/base.h>

namespace Foldicon
{
    // Identifier of the class, the same one registered in Package.appxmanifest
    struct __declspec(uuid("8fce7177-7f17-4d80-b255-7036dd73ebbd")) ExplorerCommand
        : winrt::implements<ExplorerCommand, IExplorerCommand>
    {
        // Returns the text of the item, in the language of the user
        IFACEMETHODIMP GetTitle(IShellItemArray* items, LPWSTR* name) override;

        // Returns the icon of the item: the one of the app
        IFACEMETHODIMP GetIcon(IShellItemArray* items, LPWSTR* icon) override;

        IFACEMETHODIMP GetToolTip(IShellItemArray* items, LPWSTR* infoTip) override;
        IFACEMETHODIMP GetCanonicalName(GUID* commandName) override;

        // Shows the item only for one folder of the file system
        IFACEMETHODIMP GetState(IShellItemArray* items, BOOL okToBeSlow, EXPCMDSTATE* state) override;

        // Starts the app on the selected folder
        IFACEMETHODIMP Invoke(IShellItemArray* items, IBindCtx* context) override;

        IFACEMETHODIMP GetFlags(EXPCMDFLAGS* flags) override;
        IFACEMETHODIMP EnumSubCommands(IEnumExplorerCommand** commands) override;
    };
}
