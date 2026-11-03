// Exported functions through which COM gets the factory of the command and unloads the DLL
#include <windows.h>
#include <unknwn.h>
#include <winrt/base.h>

#include "ClassFactory.h"
#include "ExplorerCommand.h"

// Returns the factory when COM asks for the class of the command
STDAPI DllGetClassObject(REFCLSID clsid, REFIID iid, void** result)
{
    *result = nullptr;
    if (clsid != __uuidof(Foldicon::ExplorerCommand))
    {
        return CLASS_E_CLASSNOTAVAILABLE;
    }

    try
    {
        return winrt::make<Foldicon::ClassFactory>().as<IUnknown>()->QueryInterface(iid, result);
    }
    catch (...)
    {
        return winrt::to_hresult();
    }
}

// Lets COM unload the DLL when no command, factory or lock is alive
STDAPI DllCanUnloadNow()
{
    return winrt::get_module_lock() == 0 ? S_OK : S_FALSE;
}
