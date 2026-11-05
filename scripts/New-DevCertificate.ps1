# Creates the self-signed certificate used to sign local development packages of Foldicon
#
# The subject matches the Publisher of Package.appxmanifest. The certificate stays in the store of the current user;
# with -Trust, its public part is also added to Trusted People of the machine so Windows installs the package, which
# needs an elevated PowerShell. Store builds never use it: the Store signs the packages it distributes.

[CmdletBinding()]
param(
    [switch]$Trust
)

$ErrorActionPreference = 'Stop'
. (Join-Path $PSScriptRoot 'Common.ps1')

# Reuses a valid development certificate if there is one
$existing = Get-DevCertificate

# Creates a code signing certificate valid for one year otherwise, whose private key can never leave the store
if ($null -eq $existing) {
    $existing = New-SelfSignedCertificate `
        -Type Custom `
        -Subject $DevCertificateSubject `
        -FriendlyName $DevCertificateName `
        -KeyUsage DigitalSignature `
        -KeyExportPolicy NonExportable `
        -CertStoreLocation Cert:\CurrentUser\My `
        -NotAfter (Get-Date).AddYears(1) `
        -TextExtension @('2.5.29.37={text}1.3.6.1.5.5.7.3.3', '2.5.29.19={text}')
}

# Adds the public part to Trusted People of the machine when asked
if ($Trust) {
    $file = Join-Path $env:TEMP 'foldicon-dev.cer'
    Export-Certificate -Cert $existing -FilePath $file | Out-Null
    Import-Certificate -FilePath $file -CertStoreLocation Cert:\LocalMachine\TrustedPeople | Out-Null
    Remove-Item $file
}

Write-Output "Thumbprint: $($existing.Thumbprint)"
