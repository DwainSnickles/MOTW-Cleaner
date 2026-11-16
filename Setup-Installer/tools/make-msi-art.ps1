# Generates the MSI dialog artwork used by the WiX UI:
#   banner.bmp  493 x 58   dark HUD strip shown at the top of every MSI dialog
#   dialog.bmp  493 x 312  light tech surface behind the Welcome / Finish text
# MSI dialogs draw their text in system colors, so the large surface stays light
# while the branding lives in the banner strip.
[CmdletBinding()]
param(
    [string]$OutputDirectory = ""
)

$ErrorActionPreference = "Stop"
Add-Type -AssemblyName System.Drawing

if ([string]::IsNullOrEmpty($OutputDirectory)) {
    $OutputDirectory = Join-Path $PSScriptRoot "..\msi\assets"
}
if (-not (Test-Path $OutputDirectory)) { New-Item -ItemType Directory -Path $OutputDirectory -Force | Out-Null }

function Draw-Tracked {
    param(
        [System.Drawing.Graphics]$Graphics,
        [string]$Text,
        [System.Drawing.Font]$Font,
        [System.Drawing.Color]$Color,
        [float]$X,
        [float]$Y,
        [float]$Tracking
    )
    $brush = New-Object System.Drawing.SolidBrush $Color
    $format = [System.Drawing.StringFormat]::GenericTypographic
    $cursor = $X
    foreach ($ch in $Text.ToCharArray()) {
        $glyph = $ch.ToString()
        $Graphics.DrawString($glyph, $Font, $brush, $cursor, $Y, $format)
        $cursor += $Graphics.MeasureString($glyph, $Font, [System.Drawing.PointF]::Empty, $format).Width + $Tracking
    }
    $brush.Dispose()
}

function Draw-Mark {
    param([System.Drawing.Graphics]$Graphics, [System.Drawing.RectangleF]$Rect)
    $path = New-Object System.Drawing.Drawing2D.GraphicsPath
    $d = 9
    $path.AddArc($Rect.X, $Rect.Y, $d, $d, 180, 90)
    $path.AddArc($Rect.Right - $d, $Rect.Y, $d, $d, 270, 90)
    $path.AddArc($Rect.Right - $d, $Rect.Bottom - $d, $d, $d, 0, 90)
    $path.AddArc($Rect.X, $Rect.Bottom - $d, $d, $d, 90, 90)
    $path.CloseFigure()
    $fill = New-Object System.Drawing.SolidBrush ([System.Drawing.Color]::FromArgb(255, 13, 22, 30))
    $Graphics.FillPath($fill, $path)
    $fill.Dispose()
    $pen = New-Object System.Drawing.Pen ([System.Drawing.Color]::FromArgb(150, 43, 224, 255)), 1
    $Graphics.DrawPath($pen, $path)
    $pen.Dispose()

    $pen = New-Object System.Drawing.Pen ([System.Drawing.Color]::FromArgb(210, 155, 232, 75)), 2
    $pen.StartCap = [System.Drawing.Drawing2D.LineCap]::Round
    $pen.EndCap = [System.Drawing.Drawing2D.LineCap]::Round
    $Graphics.DrawLines($pen, @(
        (New-Object System.Drawing.PointF ($Rect.X + 9), ($Rect.Y + 16)),
        (New-Object System.Drawing.PointF ($Rect.X + 13.5), ($Rect.Y + 21)),
        (New-Object System.Drawing.PointF ($Rect.X + 22), ($Rect.Y + 10))
    ))
    $pen.Dispose()
    $path.Dispose()
}

# ---------- banner ----------
$banner = New-Object System.Drawing.Bitmap 493, 58, ([System.Drawing.Imaging.PixelFormat]::Format24bppRgb)
$g = [System.Drawing.Graphics]::FromImage($banner)
$g.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::AntiAlias
$g.TextRenderingHint = [System.Drawing.Text.TextRenderingHint]::ClearTypeGridFit

$bg = New-Object System.Drawing.Drawing2D.LinearGradientBrush(
    (New-Object System.Drawing.Rectangle 0, 0, 493, 58),
    [System.Drawing.Color]::FromArgb(255, 11, 18, 25),
    [System.Drawing.Color]::FromArgb(255, 5, 9, 13),
    90)
$g.FillRectangle($bg, 0, 0, 493, 58)
$bg.Dispose()

$grid = New-Object System.Drawing.Pen ([System.Drawing.Color]::FromArgb(255, 12, 24, 33)), 1
for ($x = 0; $x -le 493; $x += 16) { $g.DrawLine($grid, $x, 0, $x, 58) }
for ($y = 0; $y -le 58; $y += 16) { $g.DrawLine($grid, 0, $y, 493, $y) }
$grid.Dispose()

# accent hairline along the bottom edge
$line = New-Object System.Drawing.Drawing2D.LinearGradientBrush(
    (New-Object System.Drawing.Rectangle 0, 55, 493, 3),
    [System.Drawing.Color]::FromArgb(255, 43, 224, 255),
    [System.Drawing.Color]::FromArgb(255, 155, 232, 75),
    0)
$g.FillRectangle($line, 0, 56, 493, 2)
$line.Dispose()

Draw-Mark -Graphics $g -Rect (New-Object System.Drawing.RectangleF 13, 12, 34, 34)

$titleFont = New-Object System.Drawing.Font "Segoe UI", 12.5, ([System.Drawing.FontStyle]::Bold), ([System.Drawing.GraphicsUnit]::Point)
Draw-Tracked -Graphics $g -Text "MOTW CLEANER" -Font $titleFont -Color ([System.Drawing.Color]::FromArgb(255, 232, 244, 250)) -X 58 -Y 9 -Tracking 2.2
$titleFont.Dispose()

$subFont = New-Object System.Drawing.Font "Consolas", 6.8, ([System.Drawing.FontStyle]::Regular), ([System.Drawing.GraphicsUnit]::Point)
Draw-Tracked -Graphics $g -Text "MSI DEPLOYMENT  |  v1.3.1.0" -Font $subFont -Color ([System.Drawing.Color]::FromArgb(255, 118, 144, 162)) -X 59 -Y 34 -Tracking 1.1
$subFont.Dispose()

$chipFont = New-Object System.Drawing.Font "Consolas", 6.4, ([System.Drawing.FontStyle]::Regular), ([System.Drawing.GraphicsUnit]::Point)
$chipText = "WINDOWS INSTALLER"
$chipWidth = $g.MeasureString($chipText, $chipFont).Width + 16
$chipRect = New-Object System.Drawing.RectangleF (493 - 14 - $chipWidth), 18, $chipWidth, 20
$chipPath = New-Object System.Drawing.Drawing2D.GraphicsPath
$chipPath.AddRectangle($chipRect)
$chipFill = New-Object System.Drawing.SolidBrush ([System.Drawing.Color]::FromArgb(255, 14, 25, 34))
$g.FillRectangle($chipFill, $chipRect)
$chipFill.Dispose()
$chipPen = New-Object System.Drawing.Pen ([System.Drawing.Color]::FromArgb(255, 30, 54, 70)), 1
$g.DrawRectangle($chipPen, $chipRect.X, $chipRect.Y, $chipRect.Width, $chipRect.Height)
$chipPen.Dispose()
$g.DrawString($chipText, $chipFont, (New-Object System.Drawing.SolidBrush ([System.Drawing.Color]::FromArgb(255, 108, 190, 208))), $chipRect.X + 8, $chipRect.Y + 5)
$chipFont.Dispose()
$g.Dispose()

$bannerPath = Join-Path $OutputDirectory "banner.bmp"
$banner.Save($bannerPath, [System.Drawing.Imaging.ImageFormat]::Bmp)
$banner.Dispose()

# ---------- dialog surface ----------
$surface = New-Object System.Drawing.Bitmap 493, 312, ([System.Drawing.Imaging.PixelFormat]::Format24bppRgb)
$g = [System.Drawing.Graphics]::FromImage($surface)
$g.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::AntiAlias

$g.Clear([System.Drawing.Color]::FromArgb(255, 246, 250, 252))

$grid = New-Object System.Drawing.Pen ([System.Drawing.Color]::FromArgb(255, 234, 242, 247)), 1
for ($x = 0; $x -le 493; $x += 17) { $g.DrawLine($grid, $x, 0, $x, 312) }
for ($y = 0; $y -le 312; $y += 17) { $g.DrawLine($grid, 0, $y, 493, $y) }
$grid.Dispose()

# soft corner brackets and a faint ring motif, kept away from the text column
$bracket = New-Object System.Drawing.Pen ([System.Drawing.Color]::FromArgb(255, 173, 214, 226)), 1
$g.DrawLine($bracket, 8, 8, 34, 8)
$g.DrawLine($bracket, 8, 8, 8, 34)
$g.DrawLine($bracket, 485, 304, 459, 304)
$g.DrawLine($bracket, 485, 304, 485, 278)
$bracket.Dispose()

$ring = New-Object System.Drawing.Pen ([System.Drawing.Color]::FromArgb(255, 226, 237, 243)), 1
$g.DrawEllipse($ring, 26, 186, 132, 132)
$g.DrawEllipse($ring, 44, 204, 96, 96)
$ring.Dispose()

$dot = New-Object System.Drawing.SolidBrush ([System.Drawing.Color]::FromArgb(255, 43, 224, 255))
$g.FillEllipse($dot, 90, 250, 4, 4)
$dot.Dispose()

$edge = New-Object System.Drawing.Pen ([System.Drawing.Color]::FromArgb(255, 219, 231, 238)), 1
$g.DrawRectangle($edge, 0, 0, 492, 311)
$edge.Dispose()

$g.Dispose()
$dialogPath = Join-Path $OutputDirectory "dialog.bmp"
$surface.Save($dialogPath, [System.Drawing.Imaging.ImageFormat]::Bmp)
$surface.Dispose()

Write-Output ("wrote {0}" -f $bannerPath)
Write-Output ("wrote {0}" -f $dialogPath)
