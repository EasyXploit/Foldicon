# Registers for the current user the package with external location that adds Foldicon to the context menu of folders
#
# The installer runs it as the user who started the setup, not elevated, with the certificate already trusted.

param(
    [Parameter(Mandatory = $true)]
    [string]$InstallFolder
)

$ErrorActionPreference = 'Stop'

Add-AppxPackage -Path (Join-Path $PSScriptRoot 'Foldicon.Shell.msix') -ExternalLocation $InstallFolder -ForceUpdateFromAnyVersion
