// Function to start the app of the package on a folder
#include "Launcher.h"

#include <windows.h>
#include <appmodel.h>
#include <shlwapi.h>
#include <shobjidl_core.h>
#include <winrt/base.h>

extern "C" IMAGE_DOS_HEADER __ImageBase;

namespace Foldicon
{
    std::wstring BuildPickArguments(std::wstring const& folder)
    {
        // Counts the backslashes at the end of the path
        std::wstring::size_type trailing = 0;
        while (trailing < folder.size() && folder[folder.size() - 1 - trailing] == L'\\')
        {
            ++trailing;
        }

        // Doubles them so the quote that follows closes the argument
        return L"--pick \"" + folder + std::wstring(trailing, L'\\') + L"\"";
    }

    long LaunchPicker(std::wstring const& folder) noexcept
    {
        try
        {
            // Reads the folder of this DLL, which also holds the executable
            wchar_t module[MAX_PATH]{};
            GetModuleFileNameW(reinterpret_cast<HMODULE>(&__ImageBase), module, MAX_PATH);
            PathRemoveFileSpecW(module);
            std::wstring const directory(module);
            auto const arguments = BuildPickArguments(folder);

            // Activates the application of a full package, the only one with a manifest next to the DLL; a full-trust
            // app receives the arguments in its command line
            if (PathFileExistsW((directory + L"\\AppxManifest.xml").c_str()))
            {
                UINT32 length = 0;
                GetCurrentPackageFamilyName(&length, nullptr);
                std::wstring family(length, L'\0');
                winrt::check_win32(GetCurrentPackageFamilyName(&length, family.data()));
                family.resize(length - 1);

                auto manager = winrt::create_instance<IApplicationActivationManager>(CLSID_ApplicationActivationManager);
                auto const userModelId = family + L"!App";
                DWORD processId = 0;
                winrt::check_hresult(manager->ActivateApplication(userModelId.c_str(), arguments.c_str(), AO_NONE, &processId));
                return S_OK;
            }

            // Starts the executable of the installation folder otherwise, when the DLL comes from a package with an
            // external location
            auto const executable = directory + L"\\Foldicon.exe";
            auto commandLine = L"\"" + executable + L"\" " + arguments;
            STARTUPINFOW startup{};
            startup.cb = sizeof(startup);
            PROCESS_INFORMATION process{};
            winrt::check_bool(CreateProcessW(executable.c_str(), commandLine.data(), nullptr, nullptr, FALSE, 0, nullptr,
                directory.c_str(), &startup, &process));
            CloseHandle(process.hThread);
            CloseHandle(process.hProcess);
            return S_OK;
        }
        catch (...)
        {
            return winrt::to_hresult();
        }
    }
}
