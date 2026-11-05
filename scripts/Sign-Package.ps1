# Signs a local MSIX of Foldicon with the development certificate created by New-DevCertificate.ps1

[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [string]$Package
)

$ErrorActionPreference = 'Stop'
. (Join-Path $PSScriptRoot 'Common.ps1')

# Finds the development certificate in the store of the current user
$certificate = Get-DevCertificate
if ($null -eq $certificate) {
    throw 'There is no development certificate; run scripts\New-DevCertificate.ps1 first.'
}

# Signs the package with SHA-256 and the certificate of the store, without exporting its key
& (Find-SdkTool 'signtool.exe') sign /fd SHA256 /sha1 $certificate.Thumbprint /s My $Package
if ($LASTEXITCODE -ne 0) {
    throw "signtool failed with code $LASTEXITCODE."
}
