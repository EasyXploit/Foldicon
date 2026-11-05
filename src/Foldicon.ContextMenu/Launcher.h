// Function to start the packaged app on a folder
#pragma once

#include <string>

namespace Foldicon
{
    // Starts the app on the folder, from a full package or from the installation folder; returns the resulting HRESULT
    long LaunchPicker(std::wstring const& folder) noexcept;
}
