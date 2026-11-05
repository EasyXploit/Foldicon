# Removes the context menu package for every user and stops trusting its certificate
#
# The uninstaller runs it elevated, before deleting the files. A failed step does not stop the next one.

$ErrorActionPreference = 'Continue'

# Removes the package for every user that has it
Get-AppxPackage -AllUsers -Name 'EasyXploit.Foldicon.Shell' | Remove-AppxPackage -AllUsers

# Removes this installation's certificate from the machine's Trusted People store
$certificate = New-Object System.Security.Cryptography.X509Certificates.X509Certificate2 (Join-Path $PSScriptRoot 'Foldicon.cer')
Get-ChildItem Cert:\LocalMachine\TrustedPeople |
    Where-Object { $_.Thumbprint -eq $certificate.Thumbprint } |
    Remove-Item
