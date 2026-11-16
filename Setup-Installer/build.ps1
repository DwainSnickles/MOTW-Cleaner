[CmdletBinding()]
param(
    [string]$Configuration = "Release",
    [string]$PayloadPath = ""
)

$ErrorActionPreference = "Stop"
$here = Split-Path -Parent $MyInvocation.MyCommand.Path
Push-Location $here

try {
    if (-not (Get-Command dotnet -ErrorAction SilentlyContinue)) {
        throw "The .NET SDK (dotnet) was not found on PATH."
    }

    $uninstallerOut = Join-Path $here "artifacts\uninstaller"
    $setupOut = Join-Path $here "artifacts\setup"
    $distOut = Join-Path $here "dist"

    Remove-Item $uninstallerOut, $setupOut -Recurse -Force -ErrorAction SilentlyContinue

    Write-Host "[1/3] Building uninstaller" -ForegroundColor Cyan
    & dotnet build (Join-Path $here "uninstaller\MOTWUninstall.csproj") -c $Configuration -o $uninstallerOut --nologo -v minimal
    if ($LASTEXITCODE -ne 0) { throw "Uninstaller build failed." }

    New-Item -ItemType Directory -Path (Join-Path $here "artifacts") -Force | Out-Null
    Copy-Item (Join-Path $uninstallerOut "MOTW Cleaner Uninstall.exe") (Join-Path $here "artifacts\MOTW Cleaner Uninstall.exe") -Force

    Write-Host "[2/3] Building setup (payload embedded)" -ForegroundColor Cyan
    $buildArgs = @(
        (Join-Path $here "setup\MOTWSetup.csproj"),
        "-c", $Configuration,
        "-o", $setupOut,
        "--nologo",
        "-v", "minimal"
    )
    if ($PayloadPath -ne "") { $buildArgs += "-p:PayloadPath=$PayloadPath" }

    & dotnet build @buildArgs
    if ($LASTEXITCODE -ne 0) { throw "Setup build failed." }

    Write-Host "[3/3] Publishing" -ForegroundColor Cyan
    New-Item -ItemType Directory -Path $distOut -Force | Out-Null
    Copy-Item (Join-Path $setupOut "MOTW Setup.exe") (Join-Path $distOut "MOTW Setup.exe") -Force

    $exe = Get-Item (Join-Path $distOut "MOTW Setup.exe")
    $hash = (Get-FileHash $exe.FullName -Algorithm SHA256).Hash.ToLowerInvariant()

    Write-Host ""
    Write-Host ("  " + $exe.FullName) -ForegroundColor Green
    Write-Host ("  {0:N1} MB  sha256 {1}" -f ($exe.Length / 1MB), $hash) -ForegroundColor DarkGray
}
finally {
    Pop-Location
}
