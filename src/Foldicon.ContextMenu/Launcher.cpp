// Function to start the app of the package on a folder
#include "Launcher.h"

#include <windows.h>
#include <appmodel.h>
#include <shobjidl_core.h>
#include <winrt/base.h>

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
            // Reads the family name of the package the surrogate process runs in
            UINT32 length = 0;
            GetCurrentPackageFamilyName(&length, nullptr);
            std::wstring family(length, L'\0');
            winrt::check_win32(GetCurrentPackageFamilyName(&length, family.data()));
            family.resize(length - 1);

            // Activates the application "App" of the manifest; a full-trust app receives the arguments in its
            // command line
            auto manager = winrt::create_instance<IApplicationActivationManager>(CLSID_ApplicationActivationManager);
            auto const userModelId = family + L"!App";
            auto const arguments = BuildPickArguments(folder);
            DWORD processId = 0;
            winrt::check_hresult(manager->ActivateApplication(userModelId.c_str(), arguments.c_str(), AO_NONE, &processId));
            return S_OK;
        }
        catch (...)
        {
            return winrt::to_hresult();
        }
    }
}
