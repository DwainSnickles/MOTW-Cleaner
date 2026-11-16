# Generates assets\motw.ico from the repository logo: the artwork is cropped
# (wordmark removed) and composed onto a dark HUD tile with an accent hairline.
[CmdletBinding()]
param(
    [string]$Source = "",
    [string]$Output = "",
    [string]$Preview = ""
)

$ErrorActionPreference = "Stop"
Add-Type -AssemblyName System.Drawing

if ([string]::IsNullOrEmpty($Source)) { $Source = Join-Path $PSScriptRoot "..\..\images\logo.png" }
if ([string]::IsNullOrEmpty($Output)) { $Output = Join-Path $PSScriptRoot "..\assets\motw.ico" }

function New-RoundedPath {
    param([System.Drawing.RectangleF]$Rect, [float]$Radius)
    $path = New-Object System.Drawing.Drawing2D.GraphicsPath
    $d = $Radius * 2
    $path.AddArc($Rect.X, $Rect.Y, $d, $d, 180, 90)
    $path.AddArc($Rect.Right - $d, $Rect.Y, $d, $d, 270, 90)
    $path.AddArc($Rect.Right - $d, $Rect.Bottom - $d, $d, $d, 0, 90)
    $path.AddArc($Rect.X, $Rect.Bottom - $d, $d, $d, 90, 90)
    $path.CloseFigure()
    return $path
}

$sourcePath = (Resolve-Path $Source).Path
$src = [System.Drawing.Bitmap]::FromFile($sourcePath)

# Keep only the folder + brush artwork, drop the "MOTWapp" wordmark.
$artHeight = [int]($src.Height * 0.70)
$art = New-Object System.Drawing.Bitmap $src.Width, $artHeight
$artGraphics = [System.Drawing.Graphics]::FromImage($art)
$artGraphics.DrawImage(
    $src,
    [System.Drawing.Rectangle]::new(0, 0, $src.Width, $artHeight),
    [System.Drawing.Rectangle]::new(0, 0, $src.Width, $artHeight),
    [System.Drawing.GraphicsUnit]::Pixel)
$artGraphics.Dispose()
$src.Dispose()

$sizes = @(256, 128, 64, 48, 32, 16)
$blobs = @()

foreach ($size in $sizes) {
    $bitmap = New-Object System.Drawing.Bitmap $size, $size
    $g = [System.Drawing.Graphics]::FromImage($bitmap)
    $g.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::AntiAlias
    $g.InterpolationMode = [System.Drawing.Drawing2D.InterpolationMode]::HighQualityBicubic
    $g.PixelOffsetMode = [System.Drawing.Drawing2D.PixelOffsetMode]::HighQuality

    $pad = [float][Math]::Max(1.0, $size * 0.02)
    $tile = [System.Drawing.RectangleF]::new($pad, $pad, $size - 2 * $pad, $size - 2 * $pad)
    $radius = [float]($size * 0.21)
    $tilePath = New-RoundedPath -Rect $tile -Radius $radius

    $fill = New-Object System.Drawing.Drawing2D.LinearGradientBrush(
        $tile,
        [System.Drawing.Color]::FromArgb(255, 13, 22, 30),
        [System.Drawing.Color]::FromArgb(255, 6, 10, 14),
        90)
    $g.FillPath($fill, $tilePath)
    $fill.Dispose()

    $inner = [int]($size * 0.66)
    $ratio = $art.Width / $art.Height
    $w = $inner
    $h = [int]($w / $ratio)
    if ($h -gt $inner) { $h = $inner; $w = [int]($h * $ratio) }
    $x = [int](($size - $w) / 2)
    $y = [int](($size - $h) / 2)
    $g.DrawImage($art, [System.Drawing.Rectangle]::new($x, $y, $w, $h))

    $stroke = [float][Math]::Max(1.0, $size / 128.0)
    $hairline = New-RoundedPath -Rect ([System.Drawing.RectangleF]::new($pad + 0.5, $pad + 0.5, $size - 2 * $pad - 1, $size - 2 * $pad - 1)) -Radius $radius
    $pen = New-Object System.Drawing.Pen ([System.Drawing.Color]::FromArgb(120, 43, 224, 255)), $stroke
    $g.DrawPath($pen, $hairline)
    $pen.Dispose()
    $g.Dispose()

    $stream = New-Object System.IO.MemoryStream
    $bitmap.Save($stream, [System.Drawing.Imaging.ImageFormat]::Png)
    $blobs += ,($stream.ToArray())

    if ($Preview -ne "" -and $size -eq 256) {
        $bitmap.Save($Preview, [System.Drawing.Imaging.ImageFormat]::Png)
    }

    $bitmap.Dispose()
}

$directory = Split-Path -Parent $Output
if (-not (Test-Path $directory)) { New-Item -ItemType Directory -Path $directory -Force | Out-Null }

$file = [System.IO.File]::Create($Output)
$writer = New-Object System.IO.BinaryWriter $file
$writer.Write([UInt16]0)
$writer.Write([UInt16]1)
$writer.Write([UInt16]$sizes.Count)

$offset = 6 + 16 * $sizes.Count
for ($i = 0; $i -lt $sizes.Count; $i++) {
    $dimension = if ($sizes[$i] -ge 256) { 0 } else { $sizes[$i] }
    $writer.Write([Byte]$dimension)
    $writer.Write([Byte]$dimension)
    $writer.Write([Byte]0)
    $writer.Write([Byte]0)
    $writer.Write([UInt16]1)
    $writer.Write([UInt16]32)
    $writer.Write([UInt32]$blobs[$i].Length)
    $writer.Write([UInt32]$offset)
    $offset += $blobs[$i].Length
}

foreach ($blob in $blobs) { $writer.Write($blob) }
$writer.Close()

Write-Output ("wrote {0} ({1} sizes)" -f $Output, $sizes.Count)
