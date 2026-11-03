# Building and trying a local package

## Requirements

- Windows 10 2004 or later, with *Developer mode* on to install local packages.
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

## Package, sign and install

```powershell
# Creates the development certificate once; -Trust needs an elevated PowerShell
.\scripts\New-DevCertificate.ps1 -Trust

# Builds the MSIX
& $msbuild src\Foldicon.App\Foldicon.App.csproj -p:Configuration=Release -p:Platform=x64 -p:GenerateAppxPackageOnBuild=true -p:AppxPackageDir=..\..\AppPackages\

# Signs it and installs it
.\scripts\Sign-Package.ps1 -Package AppPackages\Foldicon.App_0.1.0.0_x64_Test\Foldicon.App_0.1.0.0_x64.msix
Add-AppxPackage AppPackages\Foldicon.App_0.1.0.0_x64_Test\Foldicon.App_0.1.0.0_x64.msix
```

Explorer loads new context menu commands when it starts again: sign out or restart Explorer after the first install.

The Store builds are uploaded unsigned (`.msixupload`), and the Store signs them with the identity reserved for the
app; the development certificate is never part of them.
