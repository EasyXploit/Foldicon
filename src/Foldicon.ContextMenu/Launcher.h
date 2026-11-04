// Function to start the app of the package on a folder
#pragma once

#include <string>

namespace Foldicon
{
    // Builds the arguments "--pick "<folder>"", doubling the backslashes before the closing quote so a drive root
    // such as C:\ is not read as an escaped quote
    std::wstring BuildPickArguments(std::wstring const& folder);

    // Starts the app on the folder, from a full package or from the installation folder; returns the resulting HRESULT
    long LaunchPicker(std::wstring const& folder) noexcept;
}
