// Factory that COM asks for new instances of the Explorer command
#include "ClassFactory.h"

#include "ExplorerCommand.h"

namespace Foldicon
{
    IFACEMETHODIMP ClassFactory::CreateInstance(IUnknown* outer, REFIID iid, void** result) noexcept
    {
        *result = nullptr;

        // Refuses aggregation, which the command does not support
        if (outer != nullptr)
        {
            return CLASS_E_NOAGGREGATION;
        }

        try
        {
            return winrt::make<ExplorerCommand>().as<IUnknown>()->QueryInterface(iid, result);
        }
        catch (...)
        {
            return winrt::to_hresult();
        }
    }

    IFACEMETHODIMP ClassFactory::LockServer(BOOL lock) noexcept
    {
        // Uses the counter of C++/WinRT, which also counts every live object of the module
        lock ? ++winrt::get_module_lock() : --winrt::get_module_lock();
        return S_OK;
    }
}
