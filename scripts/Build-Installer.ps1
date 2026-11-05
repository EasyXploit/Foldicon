# Builds the Windows installers of Foldicon in dist\Foldicon-Setup-<version>-<platform>.exe
#
# It publishes the app without a package, self-contained, packs and signs the external location package for the
# context menu, and compiles packaging\windows\foldicon.iss with Inno Setup. It creates the development certificate in
# the current user's certificate store when there is none, but it never trusts it on this PC nor installs anything. The
# private key never leaves the certificate store.

param(
    # Platforms to build; each one gets its own installer
    [ValidateSet('x64', 'ARM64')]
    [string[]]$Platforms = @('x64', 'ARM64')
)

$ErrorActionPreference = 'Stop'
. (Join-Path $PSScriptRoot 'Common.ps1')
$root = Split-Path $PSScriptRoot -Parent

# Reads the package version from its manifest, and the three parts shown to the user
[xml]$manifest = Get-Content (Join-Path $root 'src\Foldicon.App\Package.appxmanifest')
$packageVersion = $manifest.Package.Identity.Version
$version = ($packageVersion -split '\.')[0..2] -join '.'

# Finds MSBuild, which also builds the C++ extension
$vswhere = "${env:ProgramFiles(x86)}\Microsoft Visual Studio\Installer\vswhere.exe"
$msbuild = & $vswhere -latest -requires Microsoft.Component.MSBuild -find 'MSBuild\**\Bin\MSBuild.exe' | Select-Object -First 1
if ($null -eq $msbuild) {
    throw 'MSBuild was not found; install Visual Studio with the .NET desktop and C++ desktop workloads.'
}

# Finds the Inno Setup compiler on the path, in the usual folders or where its uninstaller says it is
function Find-Iscc {
    $command = Get-Command ISCC.exe -ErrorAction SilentlyContinue
    if ($command) { return $command.Source }
    $folders = @("$env:LOCALAPPDATA\Programs\Inno Setup 6", "${env:ProgramFiles(x86)}\Inno Setup 6", "$env:ProgramFiles\Inno Setup 6")
    $folders += Get-ItemProperty 'HKCU:\Software\Microsoft\Windows\CurrentVersion\Uninstall\*', 'HKLM:\Software\Microsoft\Windows\CurrentVersion\Uninstall\*', 'HKLM:\Software\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall\*' -ErrorAction SilentlyContinue |
        Where-Object { $_.DisplayName -like 'Inno Setup*' } | ForEach-Object { $_.InstallLocation }
    foreach ($folder in $folders) {
        if ($folder -and (Test-Path (Join-Path $folder 'ISCC.exe'))) { return Join-Path $folder 'ISCC.exe' }
    }
    return $null
}
$iscc = Find-Iscc
if (-not $iscc) {
    throw 'Inno Setup was not found; install it from https://jrsoftware.org/isinfo.php.'
}

foreach ($platform in $Platforms) {
    # Prepares a clean folder for the payload
    $payload = Join-Path $root "obj\installer\$platform"
    if (Test-Path $payload) {
        Remove-Item $payload -Recurse -Force
    }

    $appFolder = Join-Path $payload 'app'
    $shellFolder = Join-Path $payload 'shell'
    $sparseFolder = Join-Path $payload 'sparse'
    New-Item -ItemType Directory -Force $shellFolder, (Join-Path $sparseFolder 'Assets') | Out-Null

    # Builds the C++ extension, which publishing the app copies next to the executable
    & $msbuild (Join-Path $root 'src\Foldicon.ContextMenu\Foldicon.ContextMenu.vcxproj') -p:Configuration=Release -p:Platform=$platform -nologo -v:m
    if ($LASTEXITCODE -ne 0) {
        throw "The build of the context menu failed with code $LASTEXITCODE."
    }

    # Builds the Fluent icon library, which publishing the app also copies next to the executable
    & $msbuild (Join-Path $root 'src\Foldicon.FluentIcons\Foldicon.FluentIcons.vcxproj') -p:Configuration=Release -p:Platform=$platform -nologo -v:m
    if ($LASTEXITCODE -ne 0) {
        throw "The build of the Fluent icons failed with code $LASTEXITCODE."
    }

    # Publishes the app without a package, carrying .NET and the Windows App SDK
    & $msbuild (Join-Path $root 'src\Foldicon.App\Foldicon.App.csproj') -restore -t:Rebuild -t:Publish -p:Configuration=Release -p:Platform=$platform `
        -p:WindowsPackageType=None -p:WindowsAppSDKSelfContained=true -p:PublishDir="$appFolder\" -nologo -v:m
    if ($LASTEXITCODE -ne 0) {
        throw "The publish of the app failed with code $LASTEXITCODE."
    }

    # Packs the package with external location with its version; its logos come from the app
    $sparseManifest = (Get-Content (Join-Path $root 'packaging\windows\sparse\AppxManifest.xml') -Raw).Replace('$version$', $packageVersion)
    [System.IO.File]::WriteAllText((Join-Path $sparseFolder 'AppxManifest.xml'), $sparseManifest)
    Copy-Item (Join-Path $root 'src\Foldicon.App\Assets\*.png') (Join-Path $sparseFolder 'Assets')
    $sparsePackage = Join-Path $shellFolder 'Foldicon.Shell.msix'
    & (Find-SdkTool 'makeappx.exe') pack /d $sparseFolder /p $sparsePackage /nv /o
    if ($LASTEXITCODE -ne 0) {
        throw "makeappx failed with code $LASTEXITCODE."
    }

    # Signs it with the development certificate, creating it when it is missing
    & (Join-Path $PSScriptRoot 'New-DevCertificate.ps1') | Out-Null
    & (Join-Path $PSScriptRoot 'Sign-Package.ps1') -Package $sparsePackage

    # Adds the certificate's public part and the scripts that register and unregister the package
    Export-Certificate -Cert (Get-DevCertificate) -FilePath (Join-Path $shellFolder 'Foldicon.cer') | Out-Null
    Copy-Item (Join-Path $root 'packaging\windows\*-Shell.ps1') $shellFolder

    # Compiles the installer
    & $iscc "/DAppVersion=$version" "/DPayload=$payload" "/DPlatform=$($platform.ToLowerInvariant())" (Join-Path $root 'packaging\windows\foldicon.iss')
    if ($LASTEXITCODE -ne 0) {
        throw "ISCC failed with code $LASTEXITCODE."
    }

    Write-Output (Join-Path $root "dist\Foldicon-Setup-$version-$($platform.ToLowerInvariant()).exe")
}
