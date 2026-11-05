// Function to build the command line that opens the app on a folder
#include "PickArguments.h"

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
}
