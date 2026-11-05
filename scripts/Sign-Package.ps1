# Signs a local Foldicon MSIX with the development certificate created by New-DevCertificate.ps1

[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [string]$Package
)

$ErrorActionPreference = 'Stop'
. (Join-Path $PSScriptRoot 'Common.ps1')

# Finds the development certificate in the current user's certificate store
$certificate = Get-DevCertificate
if ($null -eq $certificate) {
    throw 'There is no development certificate; run scripts\New-DevCertificate.ps1 first.'
}

# Signs the package with SHA-256 and the certificate in the store, without exporting its key; the timestamp keeps the
# signature valid after the certificate expires
& (Find-SdkTool 'signtool.exe') sign /fd SHA256 /sha1 $certificate.Thumbprint /s My /tr $TimestampServer /td SHA256 $Package
if ($LASTEXITCODE -ne 0) {
    throw "signtool failed with code $LASTEXITCODE."
}
