// Command "Change folder icon..." of the context menu of folders
#include "ExplorerCommand.h"

#include <shlwapi.h>
#include <string>

#include "Launcher.h"
#include "resource.h"

extern "C" IMAGE_DOS_HEADER __ImageBase;

namespace
{
    // Returns the path of the single folder of the selection, or an empty string when there is not exactly one
    std::wstring SingleFolder(IShellItemArray* items)
    {
        // Checks that exactly one item is selected
        DWORD count = 0;
        if (items == nullptr || FAILED(items->GetCount(&count)) || count != 1)
        {
            return {};
        }

        // Checks that it is a folder of the file system, not a library or a virtual item
        winrt::com_ptr<IShellItem> item;
        SFGAOF attributes = 0;
        if (FAILED(items->GetItemAt(0, item.put()))
            || FAILED(item->GetAttributes(SFGAO_FILESYSTEM | SFGAO_FOLDER, &attributes))
            || (attributes & (SFGAO_FILESYSTEM | SFGAO_FOLDER)) != (SFGAO_FILESYSTEM | SFGAO_FOLDER))
        {
            return {};
        }

        // Reads its path
        PWSTR path = nullptr;
        if (FAILED(item->GetDisplayName(SIGDN_FILESYSPATH, &path)))
        {
            return {};
        }

        std::wstring folder(path);
        CoTaskMemFree(path);
        return folder;
    }

    // Returns the path of Foldicon.exe, which lives next to this DLL in the package
    std::wstring AppExecutable()
    {
        wchar_t module[MAX_PATH]{};
        GetModuleFileNameW(reinterpret_cast<HMODULE>(&__ImageBase), module, MAX_PATH);
        PathRemoveFileSpecW(module);
        return std::wstring(module) + L"\\Foldicon.exe";
    }
}

namespace Foldicon
{
    IFACEMETHODIMP ExplorerCommand::GetTitle(IShellItemArray*, LPWSTR* name)
    {
        // Reads the string of the language of the user from the resources of the DLL
        wchar_t const* text = nullptr;
        auto const length = LoadStringW(reinterpret_cast<HINSTANCE>(&__ImageBase), IDS_CHANGE_FOLDER_ICON,
            reinterpret_cast<LPWSTR>(&text), 0);
        if (length <= 0)
        {
            *name = nullptr;
            return E_FAIL;
        }

        return SHStrDupW(std::wstring(text, static_cast<size_t>(length)).c_str(), name);
    }

    IFACEMETHODIMP ExplorerCommand::GetIcon(IShellItemArray*, LPWSTR* icon)
    {
        return SHStrDupW((AppExecutable() + L",0").c_str(), icon);
    }

    IFACEMETHODIMP ExplorerCommand::GetToolTip(IShellItemArray*, LPWSTR* infoTip)
    {
        *infoTip = nullptr;
        return E_NOTIMPL;
    }

    IFACEMETHODIMP ExplorerCommand::GetCanonicalName(GUID* commandName)
    {
        *commandName = __uuidof(ExplorerCommand);
        return S_OK;
    }

    IFACEMETHODIMP ExplorerCommand::GetState(IShellItemArray* items, BOOL, EXPCMDSTATE* state)
    {
        *state = SingleFolder(items).empty() ? ECS_HIDDEN : ECS_ENABLED;
        return S_OK;
    }

    IFACEMETHODIMP ExplorerCommand::Invoke(IShellItemArray* items, IBindCtx*)
    {
        auto const folder = SingleFolder(items);
        return folder.empty() ? E_INVALIDARG : LaunchPicker(folder);
    }

    IFACEMETHODIMP ExplorerCommand::GetFlags(EXPCMDFLAGS* flags)
    {
        *flags = ECF_DEFAULT;
        return S_OK;
    }

    IFACEMETHODIMP ExplorerCommand::EnumSubCommands(IEnumExplorerCommand** commands)
    {
        *commands = nullptr;
        return E_NOTIMPL;
    }
}
