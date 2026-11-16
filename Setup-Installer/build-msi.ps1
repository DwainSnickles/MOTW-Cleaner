[CmdletBinding()]
param(
    [string]$PayloadPath = "",
    [string]$Output = ""
)

$ErrorActionPreference = "Stop"
$here = Split-Path -Parent $MyInvocation.MyCommand.Path
Push-Location $here

try {
    if (-not (Get-Command wix -ErrorAction SilentlyContinue)) {
        throw "The WiX toolset was not found on PATH. Install it with: dotnet tool install --global wix"
    }

    $wix = (Get-Command wix).Source
    $version = "1.3.1.0"

    if ($PayloadPath -eq "") {
        $candidates = @(
            (Join-Path $here "..\MOTW Portable\publish\MOTW-Cleaner-Portable.exe"),
            (Join-Path $here "..\MOTW-Cleaner-Portable\publish\MOTW-Cleaner-Portable.exe")
        )
        foreach ($candidate in $candidates) {
            if (Test-Path $candidate) { $PayloadPath = (Resolve-Path $candidate).Path; break }
        }
    }
    else {
        $PayloadPath = (Resolve-Path $PayloadPath).Path
    }

    if ($PayloadPath -eq "" -or -not (Test-Path $PayloadPath)) {
        throw "Payload not found. Build 'MOTW Portable' first (dotnet publish) or pass -PayloadPath."
    }

    if ($Output -eq "") { $Output = Join-Path $here "MOTW.Installer.msi" }

    $artDirectory = Join-Path $here "msi\assets"
    if (-not (Test-Path (Join-Path $artDirectory "banner.bmp")) -or
        -not (Test-Path (Join-Path $artDirectory "dialog.bmp"))) {
        Write-Host "Generating MSI artwork" -ForegroundColor Cyan
        & powershell -NoProfile -ExecutionPolicy Bypass -File (Join-Path $here "tools\make-msi-art.ps1")
        if ($LASTEXITCODE -ne 0) { throw "MSI artwork generation failed." }
    }

    Write-Host ("Payload: " + $PayloadPath) -ForegroundColor DarkGray
    Write-Host "Compiling MSI" -ForegroundColor Cyan

    & $wix build (Join-Path $here "msi\MOTW.Cleaner.wxs") `
        -ext WixToolset.UI.wixext `
        -arch x64 `
        -d PayloadPath="$PayloadPath" `
        -d ProductVersion="$version" `
        -o $Output
    if ($LASTEXITCODE -ne 0) { throw "WiX build failed." }

    $file = Get-Item $Output
    $hash = (Get-FileHash $file.FullName -Algorithm SHA256).Hash
    Write-Host ""
    Write-Host ("  " + $file.FullName) -ForegroundColor Green
    Write-Host ("  {0:N1} MB  sha256 {1}" -f ($file.Length / 1MB), $hash) -ForegroundColor DarkGray
}
finally {
    Pop-Location
}
