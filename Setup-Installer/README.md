# MOTW Cleaner — Setup Installer

A custom setup utility for [MOTW Cleaner](https://github.com/DwainSnickles/MOTW-Cleaner) 1.3.1.0.
It is a borderless, custom-painted "HUD" wizard written against **.NET Framework 4.8** with
**no NuGet packages and no third-party installer engine**, so the build only needs the .NET SDK.

The finished setup is a single executable with the application payload embedded — it installs
offline, asks for UAC once, and registers a matching uninstaller in **Apps & features**.

## Build

```powershell
powershell -ExecutionPolicy Bypass -File .\build.ps1
```

Output: `dist\MOTW Setup.exe` (~69 MB, payload included) plus its SHA-256 in the console.

## MSI build

```powershell
powershell -ExecutionPolicy Bypass -File .\build-msi.ps1
```

Output: `MOTW.Installer.msi` in this folder (~63 MB, payload included) plus its SHA-256.

Requires the WiX CLI (`dotnet tool install --global wix`) and its UI extension
(`wix extension add -g WixToolset.UI.wixext/5.0.2`). `build-msi.ps1` resolves the same payload,
generates the artwork on first run, and compiles with `wix build`.

The MSI installs per-machine into `%ProgramFiles%\MOTW Cleaner`, adds Start menu and desktop
shortcuts, registers `App Paths`, sets the Apps & features icon/links, and upgrades a previous
version. Uninstall is handled by Windows Installer.

Windows Installer draws its dialogs itself, so the MSI uses the HUD banner and tech-patterned
dialog surface from `tools\make-msi-art.ps1` rather than the fully custom wizard. The license
page text lives in `msi\assets\license.rtf` (placeholder terms - replace with your own EULA).

The payload is the self-contained portable build and is resolved automatically from:

1. `..\MOTW Portable\publish\MOTW-Cleaner-Portable.exe`
2. `..\MOTW-Cleaner-Portable\publish\MOTW-Cleaner-Portable.exe`

Build that project first (`dotnet publish`), or point the setup at an existing binary:

```powershell
.\build.ps1 -PayloadPath "D:\builds\MOTW-Cleaner-Portable.exe"
```

Build order matters: `build.ps1` compiles the uninstaller first, then the setup, because the
setup embeds the uninstaller executable as well as the payload and the icon.

## What the setup does

1. **Initialize** — preflight sweep: OS and build, elevation, free space on the target volume,
   embedded payload size, and any existing installation.
2. **Configure** — target folder (default `%ProgramFiles%\MOTW Cleaner`), desktop shortcut,
   Start menu shortcuts, launch-after-setup.
3. **Deploy** — streams the payload with live progress and a log, verifies the installed binary
   with SHA-256, writes the uninstaller, scrubs Mark-of-the-Web from the installed files, creates
   shortcuts, and registers:
   - `HKLM\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\MOTWCleaner`
   - `HKLM\SOFTWARE\Microsoft\Windows\CurrentVersion\App Paths\MOTW Cleaner.exe`
4. **Complete** — summary and optional launch.

Failures and user aborts roll back the files and registry keys created during that run.

## What the uninstaller does

`MOTW Cleaner Uninstall.exe` is installed next to the application (and linked from the Start menu /
Apps & features). It removes the shell shortcuts, both registry keys, and every file in the install
folder, then hands the final locked file to a temporary helper so the folder disappears cleanly.

## Layout

```
Setup-Installer/
  build.ps1                 build pipeline (uninstaller -> setup -> dist)
  build-msi.ps1             WiX MSI build -> MOTW.Installer.msi
  app.manifest              requireAdministrator + system DPI awareness
  assets/motw.ico           generated icon (6 sizes)
  msi/MOTW.Cleaner.wxs      WiX package: files, shortcuts, App Paths, WixUI
  msi/assets/               banner.bmp, dialog.bmp, license.rtf
  setup/MOTWSetup.csproj    setup executable, embeds payload + uninstaller + icon
  uninstaller/MOTWUninstall.csproj  small uninstaller, same sources, no payload
  src/
    BuildInfo.cs            product name, version, registry paths, resource names
    Theme.cs                palette, fonts, geometry/drawing helpers
    Native.cs               drag window, cue banner, elevation, DWM corners, OS string
    Shortcuts.cs            IShellLink shortcut creation
    InstallEngine.cs        install / uninstall / rollback / cleanup
    Controls.cs             neon button, toggle, input, progress bar, log, step rail, ring
    InstallerWindow.cs      shared borderless chrome, background, window buttons
    SetupForm.cs            setup phases
    UninstallForm.cs        uninstall phases
    Program.cs              entry point (SETUP / UNINSTALLER defines)
  tools/make-icon.ps1       regenerates assets/motw.ico from ..\images\logo.png
  tools/make-msi-art.ps1    regenerates msi/assets/banner.bmp + dialog.bmp
```

## Notes

- The executables are **not code-signed**; SmartScreen may warn until the binary is signed.
- .NET Framework 4.8 is required (included in Windows 10 1903+ and Windows 11).
- Version numbers live in `src/BuildInfo.cs` and in both `.csproj` files — bump them together.
- `tools\make-icon.ps1` rebuilds the icon from the repository logo (artwork cropped above the
  wordmark, composed on a dark accent-bordered tile).
- The UI exposes `--cleanup <dir>` in the uninstaller build only; that is the internal helper the
  uninstaller copies to `%TEMP%` to delete the last locked file.
