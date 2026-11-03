# Builds the local installer of Foldicon in dist\Foldicon-<version>\: the signed MSIX, the Windows App Runtime it
# depends on, the public development certificate and the install and uninstall scripts
#
# It creates the development certificate in the store of the current user when there is none, but it never trusts it
# on this PC nor installs anything: that is what Install.cmd does on the PC that receives the folder. The private key
# never leaves the certificate store.

[CmdletBinding()]
param(
    [ValidateSet('x64', 'ARM64')]
    [string]$Platform = 'x64'
)

$ErrorActionPreference = 'Stop'
$root = Split-Path $PSScriptRoot -Parent

# Reads the version of the package from its manifest
[xml]$manifest = Get-Content (Join-Path $root 'src\Foldicon.App\Package.appxmanifest')
$version = $manifest.Package.Identity.Version
$dist = Join-Path $root "dist\Foldicon-$version"

# Finds MSBuild, which also builds the C++ extension
$vswhere = "${env:ProgramFiles(x86)}\Microsoft Visual Studio\Installer\vswhere.exe"
$msbuild = & $vswhere -latest -requires Microsoft.Component.MSBuild -find 'MSBuild\**\Bin\MSBuild.exe' | Select-Object -First 1
if ($null -eq $msbuild) {
    throw 'MSBuild was not found; install Visual Studio with the .NET desktop and C++ desktop workloads.'
}

# Builds the solution, then the package of the app into a clean folder
$packages = Join-Path $root "obj\installer\$Platform"
if (Test-Path $packages) {
    Remove-Item $packages -Recurse -Force
}

& $msbuild (Join-Path $root 'Foldicon.slnx') -restore -p:Configuration=Release -p:Platform=$Platform -nologo -v:m
if ($LASTEXITCODE -ne 0) {
    throw "The build failed with code $LASTEXITCODE."
}

& $msbuild (Join-Path $root 'src\Foldicon.App\Foldicon.App.csproj') -p:Configuration=Release -p:Platform=$Platform `
    -p:GenerateAppxPackageOnBuild=true -p:AppxPackageDir="$packages\" -nologo -v:m
if ($LASTEXITCODE -ne 0) {
    throw "The packaging failed with code $LASTEXITCODE."
}

# Signs the package with the development certificate, creating it when it is missing
& (Join-Path $PSScriptRoot 'New-DevCertificate.ps1') | Out-Null
$package = Get-ChildItem $packages -Recurse -Filter '*.msix' | Where-Object { $_.DirectoryName -notmatch 'Dependencies' } | Select-Object -First 1
& (Join-Path $PSScriptRoot 'Sign-Package.ps1') -Package $package.FullName

# Lays out the installer folder from scratch
if (Test-Path $dist) {
    Remove-Item $dist -Recurse -Force
}

New-Item -ItemType Directory -Force (Join-Path $dist 'Dependencies') | Out-Null
Copy-Item $package.FullName (Join-Path $dist "Foldicon_$($version)_$Platform.msix")
Get-ChildItem (Join-Path $package.DirectoryName "Dependencies\$Platform") -Filter '*.msix' | Copy-Item -Destination (Join-Path $dist 'Dependencies')
Get-ChildItem (Join-Path $PSScriptRoot 'installer') | Copy-Item -Destination $dist

# Exports only the public part of the certificate
$certificate = Get-ChildItem Cert:\CurrentUser\My |
    Where-Object { $_.Subject -eq 'CN=Sencimo Labs' -and $_.FriendlyName -eq 'Foldicon development' -and $_.NotAfter -gt (Get-Date) } |
    Select-Object -First 1
Export-Certificate -Cert $certificate -FilePath (Join-Path $dist 'Foldicon.cer') | Out-Null

Write-Output "Installer ready in $dist"
