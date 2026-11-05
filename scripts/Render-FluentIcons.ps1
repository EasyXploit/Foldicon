# Generates the Fluent pack icon files in src\Foldicon.FluentIcons\icons and the resource script that lists them,
# from the icons named in src\Foldicon.FluentIcons\pack.json
#
# It fetches only the SVG files it needs, at the commit that pack.json pins, and rasterizes them once per size with the
# headless mode of Chrome or Edge, in one page per size. The identifiers in pack.json are written in the desktop.ini of
# users' folders, so an icon never changes its identifier. Run it again after changing pack.json.

param(
    # Folder with a checkout of the icon repository; when it is missing, the script fetches its own
    [string]$Source
)

$ErrorActionPreference = 'Stop'
. (Join-Path $PSScriptRoot 'Common.ps1')
Add-Type -AssemblyName System.Drawing
$root = Split-Path $PSScriptRoot -Parent
$project = Join-Path $root 'src\Foldicon.FluentIcons'
$icons = Join-Path $project 'icons'
$work = Join-Path $root 'obj\fluent'

# Sizes in each icon file, and the free space between two icons on a page
$iconSizes = 16, 24, 32, 48, 64, 256
$gap = 2

# Folder colors, which the source draws in one dark tone that a dark Explorer would hide
$folderTop = '#FFDD7A'
$folderBottom = '#F5B22D'

# Fetches the SVG files and the license at the pinned commit, without the rest of the repository
function Get-Source([string]$repository, [string]$commit) {
    $folder = Join-Path $work 'source'
    if (-not (Test-Path $folder)) {
        & git clone --filter=blob:none --no-checkout --sparse $repository $folder
        if ($LASTEXITCODE -ne 0) { throw "The clone failed with code $LASTEXITCODE." }
    }

    & git -C $folder sparse-checkout set --no-cone '/LICENSE' '/assets/*/SVG/*_color.svg' '/assets/*/SVG/*_filled.svg'
    if ($LASTEXITCODE -ne 0) { throw "The sparse checkout failed with code $LASTEXITCODE." }
    & git -C $folder checkout --quiet $commit
    if ($LASTEXITCODE -ne 0) { throw "The checkout of $commit failed with code $LASTEXITCODE." }
    return $folder
}

# Returns the file name of an icon, as in "folder-add.ico"
function Get-IconFileName($icon) {
    return (($icon.asset.ToLowerInvariant() -replace '[^a-z0-9]+', '-') + '.ico')
}

# Returns an icon's SVG files by the size they were drawn for
function Get-Drawings($icon) {
    $drawings = @{}
    foreach ($file in Get-ChildItem -LiteralPath (Join-Path $Source "assets\$($icon.asset)\SVG") -Filter "*_$($icon.style).svg") {
        if ($file.Name -match "_(\d+)_$($icon.style)\.svg$") { $drawings[[int]$Matches[1]] = $file.FullName }
    }

    if ($drawings.Count -eq 0) {
        throw "The source has no drawing of $($icon.asset) in the style $($icon.style)."
    }

    return $drawings
}

# Returns the drawing made for the smallest size that covers the wanted one, or the largest one when none does, since
# the small drawings are adjusted to their own grid of pixels
function Select-Drawing([hashtable]$drawings, [int]$size) {
    $sizes = $drawings.Keys | Sort-Object
    $covering = $sizes | Where-Object { $_ -ge $size } | Select-Object -First 1
    if ($null -eq $covering) { $covering = $sizes | Select-Object -Last 1 }
    return $drawings[$covering]
}

# Returns an SVG as the address of an image, painting the one-tone folders with their colors
function Get-DrawingUri([string]$path, [string]$style) {
    $svg = [System.IO.File]::ReadAllText($path)
    if ($style -eq 'filled') {
        $gradient = "<defs><linearGradient id='tone' x1='0' y1='0' x2='0' y2='1'><stop stop-color='$folderTop'/><stop offset='1' stop-color='$folderBottom'/></linearGradient></defs>"
        $svg = ($svg -replace 'fill="#2[14]2[14]2[14]"', 'fill="url(#tone)"') -replace '(<svg[^>]*>)', "`$1$gradient"
    }

    return 'data:image/svg+xml;base64,' + [Convert]::ToBase64String([System.Text.Encoding]::UTF8.GetBytes($svg))
}

# Draws every pack icon at one size on a page, and returns the path of its screenshot
function Save-Page([object[]]$pack, [int]$size, [int]$columns) {
    $step = $size + $gap
    $rows = [int][Math]::Ceiling($pack.Count / $columns)
    $body = New-Object System.Text.StringBuilder
    for ($i = 0; $i -lt $pack.Count; $i++) {
        $uri = Get-DrawingUri (Select-Drawing $pack[$i].Drawings $size) $pack[$i].Icon.style
        $left = ($i % $columns) * $step
        $top = [Math]::Floor($i / $columns) * $step
        [void]$body.Append("<img style='position:absolute;left:${left}px;top:${top}px' width='$size' height='$size' src='$uri'>")
    }

    # Renders the page with a transparent background
    $page = Join-Path $work "page-$size.html"
    $output = Join-Path $work "page-$size.png"
    if (Test-Path $output) { Remove-Item $output }
    [System.IO.File]::WriteAllText($page, "<!doctype html><html><body style='margin:0;background:transparent'>$body</body></html>")
    $arguments = '--headless=new', '--disable-gpu', '--hide-scrollbars', "--user-data-dir=$(Join-Path $work 'profile')",
        '--default-background-color=00000000', '--force-device-scale-factor=1', "--window-size=$($columns * $step),$($rows * $step)",
        '--virtual-time-budget=10000', "--screenshot=$output", ([System.Uri]$page).AbsoluteUri
    Start-Process (Find-Browser) -ArgumentList $arguments -Wait -WindowStyle Hidden
    if (-not (Test-Path $output)) {
        throw "The browser did not write the page of $size px."
    }

    return $output
}

# Reads a page into a bitmap and its bytes in BGRA with straight alpha
function Read-Page([string]$path) {
    $source = New-Object System.Drawing.Bitmap $path
    $rectangle = New-Object System.Drawing.Rectangle 0, 0, $source.Width, $source.Height
    $bitmap = $source.Clone($rectangle, [System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
    $source.Dispose()
    $data = $bitmap.LockBits($rectangle, [System.Drawing.Imaging.ImageLockMode]::ReadOnly, $bitmap.PixelFormat)
    $bytes = New-Object byte[] ($data.Stride * $data.Height)
    [System.Runtime.InteropServices.Marshal]::Copy($data.Scan0, $bytes, 0, $bytes.Length)
    $bitmap.UnlockBits($data)
    return [pscustomobject]@{ Bitmap = $bitmap; Bytes = $bytes; Stride = $bitmap.Width * 4 }
}

# Returns the bytes of an icon file entry: PNG at 256 px, and a bitmap with an empty mask below it
function Get-IconEntry($page, [int]$left, [int]$top, [int]$size) {
    $stream = New-Object System.IO.MemoryStream
    if ($size -ge 256) {
        $rectangle = New-Object System.Drawing.Rectangle $left, $top, $size, $size
        $cell = $page.Bitmap.Clone($rectangle, [System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
        $cell.Save($stream, [System.Drawing.Imaging.ImageFormat]::Png)
        $cell.Dispose()
    }
    else {
        $writer = New-Object System.IO.BinaryWriter $stream
        foreach ($value in 40, $size, ($size * 2), 0x00200001, 0, 0, 0, 0, 0, 0) { $writer.Write([int]$value) }
        for ($y = $size - 1; $y -ge 0; $y--) {
            $writer.Write($page.Bytes, ($top + $y) * $page.Stride + $left * 4, $size * 4)
        }

        $writer.Write((New-Object byte[] ([int]([Math]::Ceiling($size / 32.0)) * 4 * $size)))
    }

    return , $stream.ToArray()
}

# Checks that the browser drew something in a page cell
function Test-Cell($page, [int]$left, [int]$top, [int]$size) {
    for ($y = 0; $y -lt $size; $y++) {
        $start = ($top + $y) * $page.Stride + $left * 4
        for ($x = 3; $x -lt $size * 4; $x += 4) {
            if ($page.Bytes[$start + $x] -ne 0) { return $true }
        }
    }

    return $false
}

# Writes the icon file, with its directory followed by its entries
function Save-Icon([object[]]$entries, [string]$path) {
    $stream = [System.IO.File]::Create($path)
    $writer = New-Object System.IO.BinaryWriter $stream
    $writer.Write([uint16]0); $writer.Write([uint16]1); $writer.Write([uint16]$iconSizes.Count)
    $offset = 6 + 16 * $iconSizes.Count
    for ($i = 0; $i -lt $iconSizes.Count; $i++) {
        $side = [byte]($iconSizes[$i] % 256)
        $writer.Write($side); $writer.Write($side); $writer.Write([byte]0); $writer.Write([byte]0)
        $writer.Write([uint16]1); $writer.Write([uint16]32)
        $writer.Write([uint32]$entries[$i].Length); $writer.Write([uint32]$offset)
        $offset += $entries[$i].Length
    }

    foreach ($entry in $entries) { $writer.Write([byte[]]$entry) }
    $writer.Close()
}

# Reads the pack and rejects a repeated identifier or file, which would write over another icon
$definition = Get-Content (Join-Path $project 'pack.json') -Raw -Encoding UTF8 | ConvertFrom-Json
foreach ($key in @($definition.icons.id), @($definition.icons | ForEach-Object { Get-IconFileName $_ })) {
    $repeated = $key | Group-Object | Where-Object { $_.Count -gt 1 }
    if ($repeated) { throw "pack.json repeats $($repeated[0].Name)." }
}

# Prepares the working folder, the source and each icon's drawings
New-Item -ItemType Directory -Force $work | Out-Null
if (-not $Source) { $Source = Get-Source $definition.repository $definition.commit }
$pack = @($definition.icons | ForEach-Object { [pscustomobject]@{ Icon = $_; Drawings = (Get-Drawings $_) } })

# Draws one page per size
$columns = [int][Math]::Ceiling([Math]::Sqrt($pack.Count))
$pages = @{}
foreach ($size in $iconSizes) {
    $pages[$size] = Read-Page (Save-Page $pack $size $columns)
    $expected = $columns * ($size + $gap)
    if ($pages[$size].Bitmap.Width -ne $expected) {
        throw "The page of $size px is $($pages[$size].Bitmap.Width) px wide, not $expected."
    }
}

# Cuts each icon out of the pages and writes its file
if (Test-Path $icons) { Remove-Item $icons -Recurse -Force }
New-Item -ItemType Directory -Force $icons | Out-Null
for ($i = 0; $i -lt $pack.Count; $i++) {
    $entries = @()
    foreach ($size in $iconSizes) {
        $left = ($i % $columns) * ($size + $gap)
        $top = [int][Math]::Floor($i / $columns) * ($size + $gap)
        if (-not (Test-Cell $pages[$size] $left $top $size)) {
            throw "$($pack[$i].Icon.asset) is empty at $size px."
        }

        $entries += , (Get-IconEntry $pages[$size] $left $top $size)
    }

    Save-Icon $entries (Join-Path $icons (Get-IconFileName $pack[$i].Icon))
}

foreach ($page in $pages.Values) { $page.Bitmap.Dispose() }

# Writes the resource script, which gives each icon group its identifier from the pack
$lines = @('// Icon groups of the Fluent pack, written by scripts/Render-FluentIcons.ps1 from pack.json', '')
foreach ($item in $pack) {
    $lines += "$($item.Icon.id) ICON `"icons/$(Get-IconFileName $item.Icon)`""
}

[System.IO.File]::WriteAllText((Join-Path $project 'Foldicon.FluentIcons.rc'), (($lines -join "`n") + "`n"))

# Keeps the source license next to the icons
Copy-Item -LiteralPath (Join-Path $Source 'LICENSE') (Join-Path $project 'LICENSE')
Write-Output "Wrote $($pack.Count) icons in $icons."
