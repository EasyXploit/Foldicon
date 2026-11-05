// Function to build the command line that opens the app on a folder
#pragma once

#include <string>

namespace Foldicon
{
    // Builds the arguments "--pick "<folder>"", doubling the backslashes before the closing quote so a drive root
    // such as C:\ is not read as an escaped quote
    std::wstring BuildPickArguments(std::wstring const& folder);
}
