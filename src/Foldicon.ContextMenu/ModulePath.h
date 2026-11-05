// Functions to locate the DLL of the command, whose folder also holds the executable
#pragma once

#include <windows.h>
#include <string>

namespace Foldicon
{
    // Returns the handle of this DLL
    HMODULE ThisModule() noexcept;

    // Returns the folder of this DLL
    std::wstring ModuleDirectory();
}
