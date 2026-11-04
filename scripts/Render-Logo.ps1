# Generates every image of the app from assets\logo\foldicon.svg: the logos of the packages and Foldicon.ico, used by
# the executable, the window, the installer and the context menu
#
# It rasterizes the SVG once with the headless mode of Chrome or Edge, in a temporary profile, and scales that image.
# Run it again after changing the SVG.

$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Drawing
$root = Split-Path $PSScriptRoot -Parent
$svg = Join-Path $root 'assets\logo\foldicon.svg'
$assets = Join-Path $root 'src\Foldicon.App\Assets'
$work = Join-Path $root 'obj\logo'

# Sizes of the master image, of the variants of the taskbar icon and of the icon file
$masterSize = 1024
$targetSizes = 16, 24, 32, 48, 256
$iconSizes = 16, 24, 32, 48, 64, 256

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

# Draws the master image centered on a transparent canvas, at the given height of logo
function New-Canvas([System.Drawing.Image]$master, [int]$width, [int]$height, [int]$logoSize) {
    $bitmap = New-Object System.Drawing.Bitmap $width, $height, ([System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
    $graphics = [System.Drawing.Graphics]::FromImage($bitmap)
    $graphics.InterpolationMode = 'HighQualityBicubic'
    $graphics.PixelOffsetMode = 'HighQuality'
    $graphics.SmoothingMode = 'HighQuality'
    $graphics.DrawImage($master, [int](($width - $logoSize) / 2), [int](($height - $logoSize) / 2), $logoSize, $logoSize)
    $graphics.Dispose()
    return $bitmap
}

# Writes one PNG of the given size
function Save-Png([System.Drawing.Image]$master, [string]$name, [int]$width, [int]$height, [int]$logoSize) {
    $bitmap = New-Canvas $master $width $height $logoSize
    $bitmap.Save((Join-Path $assets $name), [System.Drawing.Imaging.ImageFormat]::Png)
    $bitmap.Dispose()
}

# Returns the bytes of an entry of the icon file: PNG at 256 px, and a bitmap with an empty mask below it
function Get-IconEntry([System.Drawing.Image]$master, [int]$size) {
    $bitmap = New-Canvas $master $size $size $size
    $stream = New-Object System.IO.MemoryStream
    if ($size -ge 256) {
        $bitmap.Save($stream, [System.Drawing.Imaging.ImageFormat]::Png)
    }
    else {
        $writer = New-Object System.IO.BinaryWriter $stream
        foreach ($value in 40, $size, ($size * 2), 0x00200001, 0, 0, 0, 0, 0, 0) { $writer.Write([int]$value) }
        for ($y = $size - 1; $y -ge 0; $y--) {
            for ($x = 0; $x -lt $size; $x++) { $writer.Write([int]$bitmap.GetPixel($x, $y).ToArgb()) }
        }

        $writer.Write((New-Object byte[] ([int]([Math]::Ceiling($size / 32.0)) * 4 * $size)))
    }

    $bitmap.Dispose()
    return , $stream.ToArray()
}

# Writes the icon file, with its directory followed by its entries
function Save-Icon([System.Drawing.Image]$master, [string]$path) {
    $entries = @($iconSizes | ForEach-Object { , (Get-IconEntry $master $_) })
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

# Prepares a page that shows the SVG at the size of the master image
if (Test-Path $work) { Remove-Item $work -Recurse -Force }
New-Item -ItemType Directory -Force $work | Out-Null
$page = Join-Path $work 'logo.html'
$uri = ([System.Uri]$svg).AbsoluteUri
Set-Content $page "<!doctype html><html><body style='margin:0;background:transparent'><img src='$uri' width='$masterSize' height='$masterSize' style='display:block'></body></html>" -Encoding UTF8

# Rasterizes it with a transparent background
$masterFile = Join-Path $work 'master.png'
$arguments = '--headless=new', '--disable-gpu', '--hide-scrollbars', "--user-data-dir=$(Join-Path $work 'profile')",
    '--default-background-color=00000000', "--window-size=$masterSize,$masterSize", "--screenshot=$masterFile", ([System.Uri]$page).AbsoluteUri
Start-Process (Find-Browser) -ArgumentList $arguments -Wait -WindowStyle Hidden
if (-not (Test-Path $masterFile)) {
    throw 'The browser did not write the image of the logo.'
}

$master = [System.Drawing.Image]::FromFile($masterFile)
if ($master.Width -ne $masterSize -or $master.Height -ne $masterSize) {
    throw "The image of the logo is $($master.Width) x $($master.Height), not $masterSize x $masterSize."
}

# Writes the logos of the packages
Save-Png $master 'Square44x44Logo.png' 44 44 44
Save-Png $master 'Square150x150Logo.png' 150 150 150
Save-Png $master 'StoreLogo.png' 50 50 50
Save-Png $master 'Wide310x150Logo.png' 310 150 120
Save-Png $master 'SplashScreen.png' 620 300 240
foreach ($size in $targetSizes) {
    Save-Png $master "Square44x44Logo.targetsize-$size.png" $size $size $size
    Save-Png $master "Square44x44Logo.targetsize-$($size)_altform-unplated.png" $size $size $size
}

# Writes the icon file
Save-Icon $master (Join-Path $assets 'Foldicon.ico')
$master.Dispose()
