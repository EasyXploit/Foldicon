# Builds the installers of the current version and publishes them as a GitHub release
#
# It needs the GitHub CLI, signed in with gh auth login. The version comes from Package.appxmanifest and the notes from
# its section in CHANGELOG.md; run Set-Version.ps1 first to publish a new one.

[CmdletBinding()]
param(
    # Publishes without building, with the installers already in dist
    [switch]$SkipBuild
)

$ErrorActionPreference = 'Stop'
$root = Split-Path $PSScriptRoot -Parent
$dist = Join-Path $root 'dist'

# Stops unless the code to publish is already on GitHub
if ((git -C $root branch --show-current) -ne 'main' -or (git -C $root status --porcelain)) {
    throw 'Publish from a clean main branch.'
}

git -C $root fetch origin main
$commit = git -C $root rev-parse HEAD
if ($commit -ne (git -C $root rev-parse origin/main)) {
    throw 'Push main before publishing.'
}

# Reads the version and refuses one that was already published
[xml]$manifest = Get-Content (Join-Path $root 'src\Foldicon.App\Package.appxmanifest')
$version = ($manifest.Package.Identity.Version -split '\.')[0..2] -join '.'
$tag = "v$version"
if (git -C $root ls-remote --tags origin "refs/tags/$tag") {
    throw "$tag is already published; run Set-Version.ps1 with a new version."
}

# Takes the notes from the changelog, which must describe the version
$changelog = Get-Content (Join-Path $root 'CHANGELOG.md') -Raw
$section = [regex]::Match($changelog, "(?ms)^## \[$([regex]::Escape($version))\].*?\r?\n(.*?)(?=^## \[|\z)")
if (-not $section.Success) {
    throw "CHANGELOG.md has no section for $version."
}

$notes = Join-Path $dist 'release-notes.md'
New-Item -ItemType Directory -Force $dist | Out-Null
[System.IO.File]::WriteAllText($notes, $section.Groups[1].Value.Trim())

# Builds the installers
if (-not $SkipBuild) {
    & (Join-Path $PSScriptRoot 'Build-Installer.ps1') | Out-Null
}

$installers = 'x64', 'arm64' | ForEach-Object { Join-Path $dist "Foldicon-Setup-$version-$_.exe" }
foreach ($installer in $installers) {
    if (-not (Test-Path $installer)) {
        throw "$installer is missing."
    }
}

# Lists their hashes, so anyone can check a download
$checksums = Join-Path $dist 'SHA256SUMS.txt'
$lines = $installers | ForEach-Object { '{0}  {1}' -f (Get-FileHash $_ -Algorithm SHA256).Hash.ToLowerInvariant(), (Split-Path $_ -Leaf) }
[System.IO.File]::WriteAllLines($checksums, [string[]]$lines)

# Publishes the release on the commit, as a prerelease before version 1
$options = @()
if ($version.StartsWith('0.')) {
    $options += '--prerelease'
}

& gh release create $tag @installers $checksums --repo EasyXploit/Foldicon --target $commit --title "Foldicon $version" --notes-file $notes @options
if ($LASTEXITCODE -ne 0) {
    throw "gh failed with code $LASTEXITCODE."
}
