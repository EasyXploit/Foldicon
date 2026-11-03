// Function to start the app of the package on a folder
#pragma once

#include <string>

namespace Foldicon
{
    // Builds the arguments "--pick "<folder>"", doubling the backslashes before the closing quote so a drive root
    // such as C:\ is not read as an escaped quote
    std::wstring BuildPickArguments(std::wstring const& folder);

    // Activates the app of the current package with the folder; returns the HRESULT of the activation
    long LaunchPicker(std::wstring const& folder) noexcept;
}
