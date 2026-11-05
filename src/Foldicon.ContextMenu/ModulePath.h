// Functions to locate the command DLL, whose folder also holds the executable
#pragma once

#include <windows.h>
#include <string>

namespace Foldicon
{
    // Returns this DLL's handle
    HMODULE ThisModule() noexcept;

    // Returns this DLL's folder
    std::wstring ModuleDirectory();
}
