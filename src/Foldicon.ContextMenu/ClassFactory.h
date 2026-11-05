// Factory that COM asks for new instances of the Explorer command
#pragma once

#include <unknwn.h>
#include <winrt/base.h>

namespace Foldicon
{
    struct ClassFactory : winrt::implements<ClassFactory, IClassFactory>
    {
        // Creates the command and returns its requested interface
        IFACEMETHODIMP CreateInstance(IUnknown* outer, REFIID iid, void** result) noexcept override;

        // Keeps the module loaded while a client holds the lock
        IFACEMETHODIMP LockServer(BOOL lock) noexcept override;
    };
}
