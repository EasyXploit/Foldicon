// Tests of the command line that the context menu builds; the exit code is the number of failed checks
#include <cstdio>
#include <string>

#include "PickArguments.h"

namespace
{
    int failures = 0;

    // Compares the arguments built for a folder with the expected ones
    void Check(std::wstring const& folder, std::wstring const& expected)
    {
        auto const actual = Foldicon::BuildPickArguments(folder);
        if (actual != expected)
        {
            std::wprintf(L"FAILED: %ls gave %ls\n", folder.c_str(), actual.c_str());
            ++failures;
        }
    }
}

int wmain()
{
    // Quotes a plain path, also with spaces
    Check(LR"(C:\Photos)", LR"(--pick "C:\Photos")");
    Check(LR"(C:\My Photos)", LR"(--pick "C:\My Photos")");

    // Doubles the backslash of a drive root, so the closing quote is not escaped
    Check(LR"(C:\)", LR"(--pick "C:\\")");
    Check(LR"(C:\Photos\\)", LR"(--pick "C:\Photos\\\\")");

    // Leaves the backslashes inside the path alone, and an empty folder empty
    Check(LR"(\\server\share)", LR"(--pick "\\server\share")");
    Check(L"", LR"(--pick "")");

    return failures;
}
