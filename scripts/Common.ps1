# Functions and names shared by the scripts of the repository; each script loads it with a dot

# Subject and name of the development certificate; the subject matches the Publisher of Package.appxmanifest
$DevCertificateSubject = 'CN=EasyXploit'
$DevCertificateName = 'Foldicon development'

# Public timestamp service used when signing; it only receives a hash of the package
$TimestampServer = 'http://timestamp.digicert.com'

# Finds an installed browser based on Chromium
function Find-Browser {
    foreach ($name in 'msedge.exe', 'chrome.exe') {
        foreach ($hive in 'HKLM:', 'HKCU:') {
            $key = "$hive\Software\Microsoft\Windows\CurrentVersion\App Paths\$name"
            if (Test-Path $key) { return (Get-ItemProperty $key).'(default)' }
        }
    }

    throw 'Neither Edge nor Chrome was found.'
}

# Returns the valid development certificate of the store of the current user, or nothing when there is none
function Get-DevCertificate {
    return Get-ChildItem Cert:\CurrentUser\My |
        Where-Object { $_.Subject -eq $DevCertificateSubject -and $_.FriendlyName -eq $DevCertificateName -and $_.NotAfter -gt (Get-Date) } |
        Select-Object -First 1
}

# Returns the newest copy of a tool of the Windows SDK
function Find-SdkTool([string]$name) {
    $tool = Get-ChildItem "${env:ProgramFiles(x86)}\Windows Kits\10\bin\*\x64\$name" | Sort-Object FullName -Descending | Select-Object -First 1
    if ($null -eq $tool) {
        throw "$name was not found; install the Windows SDK."
    }

    return $tool.FullName
}
