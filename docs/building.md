# Building and installing

## Requirements

- Windows 11.
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

## Logo

`assets\logo\foldicon.svg` is the source of every image. After changing it, run `scripts\Render-Logo.ps1`, which
rasterizes it with Edge or Chrome in headless mode and writes the logos of `src\Foldicon.App\Assets` and `Foldicon.ico`,
the icon of the executable, the window, the installer and the context menu.

## Fluent icons

The `.ico` files of `src\Foldicon.FluentIcons\icons` and its resource script are generated, and versioned so a build
needs neither a browser nor the network. After changing `pack.json`, run `scripts\Render-FluentIcons.ps1`, which fetches
the SVG files of the pinned commit into `obj\fluent`, rasterizes them with Edge or Chrome in headless mode and writes
them again. The same `.csproj` condition as for the context menu applies: `dotnet build` of the app alone warns that the
package will have no Fluent icons when the library was not built before. A new icon needs its names in
`Search/icon-names.json`, which a unit test checks.

## Release

```powershell
.\scripts\Set-Version.ps1 -Version 0.2.0
```

Set the new version, describe it in `CHANGELOG.md`, commit and push `main`. Then, with the
[GitHub CLI](https://cli.github.com/) signed in, run:

```powershell
.\scripts\Publish-Release.ps1
```

It builds both installers, lists their hashes in `SHA256SUMS.txt` and publishes them as a release of the tag `v<version>`. Releases are built on the PC of the maintainer, since the private key of the certificate
never leaves its store.

## Installer

```powershell
.\scripts\Build-Installer.ps1
```

It needs [Inno Setup](https://jrsoftware.org/isinfo.php) 6 and writes `dist\Foldicon-Setup-<version>-x64.exe` and
`-arm64.exe`; `-Platforms x64` builds only one. The ARM64 build needs the *MSVC ARM64 build tools*. For each platform, the
script:

1. Publishes the app without a package (`WindowsPackageType=None`), carrying .NET and the Windows App SDK.
2. Packs `packaging\windows\sparse\AppxManifest.xml` into `Foldicon.Shell.msix`, a package with external location that
   holds no code, and signs it, with a timestamp, with the development certificate, created in the store of the current user
   when it is missing.
3. Compiles `packaging\windows\foldicon.iss` with the app, that package, the public `Foldicon.cer` and the scripts that
   register and unregister the package.

The private key never leaves the store, and the script neither trusts the certificate nor installs anything on the PC
that builds it.

The setup installs in Program Files, with a normal entry in the installed apps. It trusts the certificate in Trusted
People of the machine and registers the package for the user who started it, which gives the installation folder the
identity the modern context menu needs. If that fails, it adds a classic item for every user instead, which shows under
*Show more options*. The package always goes to the system volume, because on a PC whose default volume for apps is
another drive Windows can refuse it with `0x800701C0` (untrusted mount point). The item of the modern menu is per user, so any other user of the PC has to run the setup too. The
uninstaller removes the package for every user, the trust in the certificate and the classic item.

The setup is not signed with a commercial certificate, so SmartScreen warns when it runs on a PC that downloaded it.

Explorer loads new context menu commands when it starts again: sign out and in after the first install.

The Store builds are uploaded unsigned (`.msixupload`), and the Store signs them with the identity reserved for the
app; the development certificate is never part of them. See [microsoft-store.md](microsoft-store.md).
