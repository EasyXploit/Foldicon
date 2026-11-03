# Building and trying a local package

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

## Local installer

```powershell
.\scripts\Build-Installer.ps1            # or -Platform ARM64
```

It builds the solution in Release, packages the app, signs the MSIX with the development certificate (created in the
store of the current user by `scripts\New-DevCertificate.ps1` when it is missing) and lays out
`dist\Foldicon-<version>\`:

| File | What it is |
| --- | --- |
| `Foldicon_<version>_<platform>.msix` | The signed package. It carries .NET, so the PC needs no runtime installed. |
| `Dependencies\*.msix` | The Windows App Runtime the package depends on. |
| `Foldicon.cer` | The public part of the development certificate. The private key never leaves the store. |
| `Install.cmd`, `Install.ps1` | Trust the certificate in Trusted People (one administrator prompt) and install the package for the current user. |
| `Uninstall.cmd`, `Uninstall.ps1` | Remove the package; `-RemoveCertificate` also stops trusting the certificate. |
| `README.txt` | The same instructions, in English and Spanish. |

The script never trusts the certificate nor installs anything on the PC that builds it. The sources of the files of the
installer are in `scripts\installer\`, and `dist\` is not versioned.

Explorer loads new context menu commands when it starts again: sign out and in after the first install.

The Store builds are uploaded unsigned (`.msixupload`), and the Store signs them with the identity reserved for the
app; the development certificate is never part of them.
