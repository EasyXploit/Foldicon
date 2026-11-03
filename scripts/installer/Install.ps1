# Installs Foldicon from this folder: trusts its development certificate (asking for administrator rights once) and
# installs the package with the Windows App Runtime it depends on, for the current user

$ErrorActionPreference = 'Stop'
$here = $PSScriptRoot
$certificateFile = Join-Path $here 'Foldicon.cer'
$package = Get-ChildItem $here -Filter 'Foldicon_*.msix' | Select-Object -First 1
if ($null -eq $package) {
    throw 'The Foldicon package is missing from this folder.'
}

# Trusts the certificate in Trusted People of the PC when it is not trusted yet, which needs an elevated process
$certificate = New-Object System.Security.Cryptography.X509Certificates.X509Certificate2 $certificateFile
$trusted = Get-ChildItem Cert:\LocalMachine\TrustedPeople | Where-Object { $_.Thumbprint -eq $certificate.Thumbprint }
if ($null -eq $trusted) {
    Write-Output 'Trusting the certificate of Foldicon (Windows asks for administrator rights)...'
    $command = "Import-Certificate -FilePath '$certificateFile' -CertStoreLocation Cert:\LocalMachine\TrustedPeople | Out-Null"
    $process = Start-Process powershell.exe -Verb RunAs -Wait -PassThru -ArgumentList @('-NoProfile', '-ExecutionPolicy', 'Bypass', '-Command', $command)
    if ($process.ExitCode -ne 0) {
        throw 'The certificate could not be trusted.'
    }
}

# Installs the package for the current user, not elevated, with its dependencies
Write-Output 'Installing Foldicon...'
$dependencies = @(Get-ChildItem (Join-Path $here 'Dependencies') -Filter '*.msix' | ForEach-Object { $_.FullName })
Add-AppxPackage -Path $package.FullName -DependencyPath $dependencies -ForceUpdateFromAnyVersion

Write-Output 'Foldicon is installed. If "Change folder icon..." does not show in the menu of a folder yet, sign out and in again.'
