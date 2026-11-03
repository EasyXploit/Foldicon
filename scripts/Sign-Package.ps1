# Signs a local MSIX of Foldicon with the development certificate created by New-DevCertificate.ps1

[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [string]$Package
)

$ErrorActionPreference = 'Stop'

# Finds the development certificate in the store of the current user
$certificate = Get-ChildItem Cert:\CurrentUser\My |
    Where-Object { $_.Subject -eq 'CN=Sencimo Labs' -and $_.FriendlyName -eq 'Foldicon development' -and $_.NotAfter -gt (Get-Date) } |
    Select-Object -First 1
if ($null -eq $certificate) {
    throw 'There is no development certificate; run scripts\New-DevCertificate.ps1 first.'
}

# Finds the newest signtool of the Windows SDK
$signtool = Get-ChildItem "${env:ProgramFiles(x86)}\Windows Kits\10\bin\*\x64\signtool.exe" |
    Sort-Object FullName -Descending |
    Select-Object -First 1
if ($null -eq $signtool) {
    throw 'signtool.exe was not found; install the Windows SDK.'
}

# Signs the package with SHA-256 and the certificate of the store, without exporting its key
& $signtool.FullName sign /fd SHA256 /sha1 $certificate.Thumbprint /s My $Package
if ($LASTEXITCODE -ne 0) {
    throw "signtool failed with code $LASTEXITCODE."
}
