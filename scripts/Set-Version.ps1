# Writes a new version of Foldicon in every file that holds it

[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [ValidatePattern('^\d+\.\d+\.\d+$')]
    [string]$Version
)

$ErrorActionPreference = 'Stop'
$root = Split-Path $PSScriptRoot -Parent

# Replaces one pattern in a file, keeping the rest of it untouched
function Set-Text([string]$file, [string]$pattern, [string]$replacement) {
    $path = Join-Path $root $file
    $text = [System.IO.File]::ReadAllText($path)
    if ($text -notmatch $pattern) {
        throw "$file does not hold a version."
    }

    [System.IO.File]::WriteAllText($path, ($text -replace $pattern, $replacement))
}

Set-Text 'src\Foldicon.App\Package.appxmanifest' '(<Identity [^>]*Version=")[\d.]+(")' "`${1}$Version.0`${2}"
Set-Text 'Directory.Build.props' '<Version>[\d.]+</Version>' "<Version>$Version</Version>"
Set-Text 'packaging\windows\foldicon.iss' '(#define AppVersion ")[\d.]+(")' "`${1}$Version`${2}"
Set-Text 'README.md' 'version [\d.]+\.</sub>' "version $Version.</sub>"
