# Registers for the current user the package with external location that adds Foldicon to the context menu of folders
#
# The installer runs it as the user who started the setup, not elevated, with the certificate already trusted.

param(
    [Parameter(Mandatory = $true)]
    [string]$InstallFolder
)

$ErrorActionPreference = 'Stop'

# Registers it on the system volume, next to the installation folder: when the default volume of the apps of the PC is
# another drive, Windows can refuse the state of the package with 0x800701C0 (untrusted mount point)
$volume = Get-AppxVolume | Where-Object IsSystemVolume
Add-AppxPackage -Path (Join-Path $PSScriptRoot 'Foldicon.Shell.msix') -ExternalLocation $InstallFolder -Volume $volume -ForceUpdateFromAnyVersion
