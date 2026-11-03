# Uninstalls Foldicon for the current user; with -RemoveCertificate it also stops trusting its development
# certificate, which asks for administrator rights

[CmdletBinding()]
param(
    [switch]$RemoveCertificate
)

$ErrorActionPreference = 'Stop'

# Removes the package; the icons already applied to folders stay, since they point to libraries of Windows
$installed = Get-AppxPackage -Name 'SencimoLabs.Foldicon'
if ($null -eq $installed) {
    Write-Output 'Foldicon is not installed.'
}
else {
    $installed | Remove-AppxPackage
    Write-Output 'Foldicon is uninstalled.'
}

# Removes the certificate from Trusted People of the PC when asked
if ($RemoveCertificate) {
    $certificate = New-Object System.Security.Cryptography.X509Certificates.X509Certificate2 (Join-Path $PSScriptRoot 'Foldicon.cer')
    $command = "Get-ChildItem Cert:\LocalMachine\TrustedPeople | Where-Object { `$_.Thumbprint -eq '$($certificate.Thumbprint)' } | Remove-Item"
    Start-Process powershell.exe -Verb RunAs -Wait -ArgumentList @('-NoProfile', '-ExecutionPolicy', 'Bypass', '-Command', $command)
    Write-Output 'The certificate of Foldicon is no longer trusted.'
}
