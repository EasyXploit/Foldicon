# Generates the README images in docs\images from the sources in docs\images\src: the banner in light and dark,
# the framed screenshots and the buttons
#
# It renders them with the headless mode of Chrome or Edge, in a temporary profile, with the Inter font, which it
# downloads to obj\readme. Run it again after changing a source or a screenshot.

$ErrorActionPreference = 'Stop'
. (Join-Path $PSScriptRoot 'Common.ps1')
Add-Type -AssemblyName System.Drawing
$root = Split-Path $PSScriptRoot -Parent
$images = Join-Path $root 'docs\images'
$sources = Join-Path $images 'src'
$work = Join-Path $root 'obj\readme'

# Version of the font, pinned so the images can be repeated
$fontUrl = 'https://cdn.jsdelivr.net/npm/@fontsource-variable/inter@5.2.8/files/inter-latin-wght-normal.woff2'

# Returns the address of a local file
function Get-FileUri([string]$path) {
    return ([System.Uri]$path).AbsoluteUri
}

# Renders a page into a PNG with a transparent background, at twice the resolution
function Save-Render([string]$name, [string]$body, [string]$style, [int]$width, [int]$height, [int]$scale = 2) {
    $page = Join-Path $work "$name.html"
    $fontUri = Get-FileUri (Join-Path $work 'inter.woff2')
    $html = "<!doctype html><html><head><meta charset='utf-8'><style>@font-face{font-family:Inter;src:url('$fontUri');font-weight:100 900}html,body{margin:0;background:transparent;font-family:Inter}$style</style></head><body>$body</body></html>"
    [System.IO.File]::WriteAllText($page, $html)
    $output = Join-Path $images "$name.png"
    $arguments = '--headless=new', '--disable-gpu', '--hide-scrollbars', "--user-data-dir=$(Join-Path $work 'profile')",
        '--default-background-color=00000000', "--force-device-scale-factor=$scale", "--window-size=$width,$height",
        '--virtual-time-budget=3000', "--screenshot=$output", (Get-FileUri $page)
    Start-Process $browser -ArgumentList $arguments -Wait -WindowStyle Hidden
    if (-not (Test-Path $output)) {
        throw "The browser did not write $output."
    }
}

# Cuts away the transparent margin of a rendered image
function Save-Trimmed([string]$name) {
    $path = Join-Path $images "$name.png"
    $source = New-Object System.Drawing.Bitmap $path
    $right = 0; $bottom = 0
    for ($y = 0; $y -lt $source.Height; $y += 2) {
        for ($x = 0; $x -lt $source.Width; $x += 2) {
            if ($source.GetPixel($x, $y).A -gt 0) {
                if ($x -gt $right) { $right = $x }
                if ($y -gt $bottom) { $bottom = $y }
            }
        }
    }

    $trimmed = $source.Clone((New-Object System.Drawing.Rectangle 0, 0, ($right + 2), ($bottom + 2)), $source.PixelFormat)
    $source.Dispose()
    $trimmed.Save($path, [System.Drawing.Imaging.ImageFormat]::Png)
    $trimmed.Dispose()
}

# Prepares the working folder and the font
$browser = Find-Browser
New-Item -ItemType Directory -Force $work | Out-Null
$font = Join-Path $work 'inter.woff2'
if (-not (Test-Path $font)) {
    Invoke-WebRequest $fontUrl -OutFile $font -UseBasicParsing
}

# Renders the banner in both themes, from the screenshot of the same theme
$logoUri = Get-FileUri (Join-Path $root 'assets\logo\foldicon.svg')
$banner = (Get-Content (Join-Path $sources 'banner.svg') -Raw -Encoding UTF8) -replace '(?s)^<!--.*?-->\s*', ''
$themes = @{
    light = @{ page = '#FFFFFF'; title = '#16181D'; tagline = '#4A5261'; frame = '#FFFFFF'; a = '#FFD978'; b = '#C9B8FF' }
    dark  = @{ page = '#0D1117'; title = '#F5F7FA'; tagline = '#AEB6C2'; frame = '#1C1F24'; a = '#7A5A10'; b = '#3D2A80' }
}
foreach ($theme in $themes.Keys) {
    $colors = $themes[$theme]
    $style = ".page{fill:$($colors.page)}.title{font-size:92px;font-weight:800;letter-spacing:-3px;fill:$($colors.title)}" +
        ".tagline{font-size:36px;font-weight:500;letter-spacing:-.5px;fill:$($colors.tagline)}.frame{fill:$($colors.frame)}" +
        ".glow-a{stop-color:$($colors.a)}.glow-b{stop-color:$($colors.b)}svg{display:block}"
    $screen = Get-FileUri (Join-Path $sources "screen-a-all-$theme.png")
    Save-Render "banner-$theme" $banner.Replace('$logo$', $logoUri).Replace('$screen$', $screen) $style 1280 640
}

# Frames each screenshot with rounded corners and a soft shadow
$frame = 'img{display:block;margin:28px 48px 52px;width:884px;border-radius:12px;box-shadow:0 18px 40px rgba(11,27,51,.28)}'
foreach ($scene in 'b-search', 'c-preview', 'd-fluent') {
    foreach ($theme in 'light', 'dark') {
        $screen = Get-FileUri (Join-Path $sources "screen-$scene-$theme.png")
        Save-Render "shot-$scene-$theme" "<img src='$screen'>" $frame 980 720 1
    }
}

# Frames the Explorer strip and the context menu in each theme, each with margins for its size
$tight = 'img{display:block;margin:20px 40px 44px;border-radius:14px;box-shadow:0 16px 36px rgba(11,27,51,.28)}'
$strips = @{ 'explorer' = 'img{width:1040px}'; 'explorer-light' = 'img{background:#FFFFFF;padding:8px 35.5px}' }
foreach ($name in $strips.Keys) {
    $strip = Get-FileUri (Join-Path $sources "screen-$name.png")
    Save-Render "shot-$name" "<img src='$strip'>" "$tight $($strips[$name])" 1120 196 1
}

foreach ($theme in 'light', 'dark') {
    $menu = Get-FileUri (Join-Path $sources "screen-menu-$theme.png")
    Save-Render "shot-menu-$theme" "<img src='$menu'>" "$tight img{width:506px}" 586 522 1
}

# Renders the buttons as pills
$arrow = "<svg width='22' height='22' viewBox='0 0 24 24' fill='none' stroke='currentColor' stroke-width='2.4' stroke-linecap='round' stroke-linejoin='round'><path d='M12 4v11M7 11l5 5 5-5M5 20h14'/></svg>"
$code = "<svg width='22' height='22' viewBox='0 0 24 24' fill='none' stroke='currentColor' stroke-width='2.4' stroke-linecap='round' stroke-linejoin='round'><path d='M9 7l-5 5 5 5M15 7l5 5-5 5'/></svg>"
$pill = 'div{display:inline-flex;align-items:center;gap:10px;height:56px;padding:0 30px;box-sizing:border-box;border-radius:28px;font-size:18px;font-weight:600;white-space:nowrap}'
Save-Render 'button-download' "<div>$arrow Download for Windows</div>" "$pill div{background:#0F6CBD;color:#FFFFFF}" 640 200
Save-Trimmed 'button-download'
Save-Render 'button-source' "<div>$code Build from source</div>" "$pill div{background:#E4EDF8;color:#0F6CBD}" 640 200
Save-Trimmed 'button-source'
