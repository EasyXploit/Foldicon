# Registers for the current user the package with external location that adds Foldicon to the folder context menu
#
# The installer runs it as the user who started the setup, not elevated, with the certificate already trusted.

param(
    [Parameter(Mandatory = $true)]
    [string]$InstallFolder
)

$ErrorActionPreference = 'Stop'

# Removes a previous registration, since Windows refuses a package with the same version and other content (0x80073CFB)
Get-AppxPackage -Name 'EasyXploit.Foldicon.Shell' | Remove-AppxPackage

# Registers it on the system volume, next to the installation folder: when the PC's default app volume is
# another drive, Windows can refuse the package state with 0x800701C0 (untrusted mount point)
$volume = Get-AppxVolume | Where-Object IsSystemVolume
Add-AppxPackage -Path (Join-Path $PSScriptRoot 'Foldicon.Shell.msix') -ExternalLocation $InstallFolder -Volume $volume -ForceUpdateFromAnyVersion
