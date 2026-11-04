# Building and installing

## Requirements

- Windows 10 2004 or later.
- .NET 10 SDK.
- Visual Studio 2026 with the *.NET desktop development* and *Desktop development with C++* workloads. ARM64 builds of
  the context menu also need the *MSVC ARM64 build tools* component.

Every package comes from nuget.org (see `nuget.config`), and every one of them is free.

## Build

The C++ extension can only be built by MSBuild, not by `dotnet build`, so build the whole solution with it:

```powershell
$msbuild = "$env:ProgramFiles\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\MSBuild.exe"
& $msbuild Foldicon.slnx -restore -p:Configuration=Debug -p:Platform=x64
```

`dotnet build src\Foldicon.App -p:Platform=x64` still works for the app alone: it warns that the package will have no
context menu when the DLL of the extension was not built before.

## Installer

```powershell
.\scripts\Build-Installer.ps1
```

It needs [Inno Setup](https://jrsoftware.org/isinfo.php) 6 and writes `dist\Foldicon-Setup-<version>.exe`. The script:

1. Publishes the app without a package (`WindowsPackageType=None`), carrying .NET and the Windows App SDK.
2. Packs `packaging\windows\sparse\AppxManifest.xml` into `Foldicon.Shell.msix`, a package with external location that
   holds no code, and signs it with the development certificate, created in the store of the current user when it is
   missing.
3. Compiles `packaging\windows\foldicon.iss` with the app, that package, the public `Foldicon.cer` and the scripts that
   register and unregister the package.

The private key never leaves the store, and the script neither trusts the certificate nor installs anything on the PC
that builds it.

The setup installs in Program Files, with a normal entry in the installed apps. It trusts the certificate in Trusted
People of the machine and registers the package for the user who started it, which gives the installation folder the
identity the modern context menu needs. If that fails, it adds a classic item for every user instead, which shows under
*Show more options*. The item of the modern menu is per user, so any other user of the PC has to run the setup too. The
uninstaller removes the package for every user, the trust in the certificate and the classic item.

The setup is not signed with a commercial certificate, so SmartScreen warns when it runs on a PC that downloaded it.

Explorer loads new context menu commands when it starts again: sign out and in after the first install.

The Store builds are uploaded unsigned (`.msixupload`), and the Store signs them with the identity reserved for the
app; the development certificate is never part of them.
