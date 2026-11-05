// Functions to locate the command DLL, whose folder also holds the executable
#include "ModulePath.h"

#include <shlwapi.h>

extern "C" IMAGE_DOS_HEADER __ImageBase;

namespace Foldicon
{
    HMODULE ThisModule() noexcept
    {
        return reinterpret_cast<HMODULE>(&__ImageBase);
    }

    std::wstring ModuleDirectory()
    {
        wchar_t module[MAX_PATH]{};
        GetModuleFileNameW(ThisModule(), module, MAX_PATH);
        PathRemoveFileSpecW(module);
        return module;
    }
}
