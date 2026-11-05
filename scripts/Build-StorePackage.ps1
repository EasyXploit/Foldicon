# Builds dist\store\*.msixupload, with x64 and ARM64, to upload to the Microsoft Store
#
# The identity comes from Partner Center, in Product identity. The manifest of the repository keeps the development
# identity: this script swaps it only during the build. The Store signs the package, so it needs no certificate.

param(
    [Parameter(Mandatory = $true)]
    [string]$IdentityName,

    [Parameter(Mandatory = $true)]
    [string]$Publisher,

    [string]$PublisherDisplayName = 'EasyXploit'
)

$ErrorActionPreference = 'Stop'
$root = Split-Path $PSScriptRoot -Parent
$manifestPath = Join-Path $root 'src\Foldicon.App\Package.appxmanifest'

# Finds MSBuild, which also builds the C++ projects
$vswhere = "${env:ProgramFiles(x86)}\Microsoft Visual Studio\Installer\vswhere.exe"
$msbuild = & $vswhere -latest -requires Microsoft.Component.MSBuild -find 'MSBuild\**\Bin\MSBuild.exe' | Select-Object -First 1
if ($null -eq $msbuild) {
    throw 'MSBuild was not found; install Visual Studio with the .NET desktop and C++ desktop workloads.'
}

# Builds the native parts of both platforms, which the package copies next to the app
foreach ($platform in 'x64', 'ARM64') {
    foreach ($project in 'Foldicon.ContextMenu', 'Foldicon.FluentIcons') {
        & $msbuild (Join-Path $root "src\$project\$project.vcxproj") -p:Configuration=Release -p:Platform=$platform -nologo -v:m
        if ($LASTEXITCODE -ne 0) {
            throw "The build of $project for $platform failed with code $LASTEXITCODE."
        }
    }
}

# Builds the package with the identity of the Store, and always puts the manifest back
$original = [System.IO.File]::ReadAllText($manifestPath)
try {
    [xml]$manifest = $original
    $manifest.Package.Identity.Name = $IdentityName
    $manifest.Package.Identity.Publisher = $Publisher
    $manifest.Package.Properties.PublisherDisplayName = $PublisherDisplayName
    $manifest.Save($manifestPath)

    & $msbuild (Join-Path $root 'src\Foldicon.App\Foldicon.App.csproj') -restore -p:Configuration=Release -p:Platform=x64 `
        -p:UapAppxPackageBuildMode=StoreUpload -p:AppxBundle=Always "-p:AppxBundlePlatforms=x64|ARM64" `
        -p:GenerateAppxPackageOnBuild=true "-p:AppxPackageDir=$(Join-Path $root 'dist\store')\" -nologo -v:m
    if ($LASTEXITCODE -ne 0) {
        throw "The build of the package failed with code $LASTEXITCODE."
    }
}
finally {
    [System.IO.File]::WriteAllText($manifestPath, $original)
}

Get-ChildItem (Join-Path $root 'dist\store') -Filter *.msixupload -Recurse | Select-Object -ExpandProperty FullName
