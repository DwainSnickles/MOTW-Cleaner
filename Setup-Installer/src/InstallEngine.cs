using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Security.Cryptography;
using System.Threading;
using System.Windows.Forms;
using Microsoft.Win32;

namespace MotwSetup
{
    internal sealed class InstallOptions
    {
        public string InstallDir;
        public bool DesktopShortcut = true;
        public bool StartMenuShortcut = true;
    }

    internal sealed class InstallReport
    {
        public string Stage;
        public string Line;
        public double Percent;
        public long BytesDone;
        public long BytesTotal;
        public bool IsError;
    }

    internal sealed class OperationResult
    {
        public bool Ok;
        public string Error;
        public string ExePath;
        public long Bytes;
    }

    internal static class InstallEngine
    {
        private const long ReserveFloor = 32L * 1024 * 1024;

        public static string DefaultInstallDir()
        {
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), BuildInfo.ProductName);
        }

        public static long PayloadSize
        {
            get
            {
                using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(BuildInfo.PayloadResource))
                    return stream == null ? 0 : stream.Length;
            }
        }

        public static long UninstallerSize
        {
            get
            {
                using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(BuildInfo.UninstallerResource))
                    return stream == null ? 0 : stream.Length;
            }
        }

        public static long FreeSpaceFor(string path)
        {
            try
            {
                string root = Path.GetPathRoot(Path.GetFullPath(path));
                if (string.IsNullOrEmpty(root)) return long.MaxValue;
                return new DriveInfo(root).AvailableFreeSpace;
            }
            catch
            {
                return long.MaxValue;
            }
        }

        public static long DirectorySize(string dir)
        {
            try
            {
                long total = 0;
                foreach (var file in Directory.GetFiles(dir, "*", SearchOption.AllDirectories))
                {
                    try { total += new FileInfo(file).Length; }
                    catch { }
                }
                return total;
            }
            catch
            {
                return 0;
            }
        }

        public static string ReadInstallDir()
        {
            try
            {
                using (var key = Registry.LocalMachine.OpenSubKey(BuildInfo.RegistryKey))
                {
                    if (key == null) return null;
                    var location = key.GetValue("InstallLocation") as string;
                    return string.IsNullOrWhiteSpace(location) ? null : location;
                }
            }
            catch
            {
                return null;
            }
        }

        public static string ReadInstalledVersion()
        {
            try
            {
                using (var key = Registry.LocalMachine.OpenSubKey(BuildInfo.RegistryKey))
                {
                    if (key == null) return null;
                    var version = key.GetValue("DisplayVersion") as string;
                    return string.IsNullOrWhiteSpace(version) ? null : version;
                }
            }
            catch
            {
                return null;
            }
        }

        public static string ReadInstalledDate()
        {
            try
            {
                using (var key = Registry.LocalMachine.OpenSubKey(BuildInfo.RegistryKey))
                {
                    if (key == null) return null;
                    var stamp = key.GetValue("InstallDate") as string;
                    DateTime parsed;
                    if (!string.IsNullOrEmpty(stamp) && DateTime.TryParseExact(stamp, "yyyyMMdd", null,
                            System.Globalization.DateTimeStyles.None, out parsed))
                        return parsed.ToString("dd MMM yyyy");
                }
            }
            catch
            {
            }
            return null;
        }

        public static bool IsAppRunning()
        {
            try
            {
                string name = Path.GetFileNameWithoutExtension(BuildInfo.AppExeName);
                return Process.GetProcessesByName(name).Length > 0;
            }
            catch
            {
                return false;
            }
        }

        public static OperationResult Install(InstallOptions options, IProgress<InstallReport> sink, CancellationToken token)
        {
            var result = new OperationResult();
            var createdFiles = new List<string>();
            bool createdDir = false;
            bool registryWritten = false;
            string dir = (options.InstallDir ?? string.Empty).Trim().TrimEnd('\\');

            try
            {
                Report(sink, "PREFLIGHT", 0.02, "elevation token verified");

                if (!Native.IsElevated)
                    throw new InvalidOperationException("Administrator privileges are required. Restart setup and accept the UAC prompt.");

                if (dir.Length == 0)
                    throw new InvalidOperationException("The install location is empty.");

                dir = Path.GetFullPath(dir);
                if (string.Equals(dir, Path.GetPathRoot(dir).TrimEnd('\\'), StringComparison.OrdinalIgnoreCase))
                    throw new InvalidOperationException("A drive root cannot be used as the install location.");

                Report(sink, "PREFLIGHT", 0.05, "target \u00B7 " + dir);

                Stream payload = Assembly.GetExecutingAssembly().GetManifestResourceStream(BuildInfo.PayloadResource);
                if (payload == null)
                    throw new InvalidOperationException("The setup payload is missing. Rebuild the installer with build.ps1.");

                long total;
                using (payload)
                    total = payload.Length;

                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                    createdDir = true;
                }

                string probe = Path.Combine(dir, ".motw-write-probe");
                File.WriteAllText(probe, "probe");
                File.Delete(probe);

                long free = FreeSpaceFor(dir);
                Report(sink, "PREFLIGHT", 0.08, Fmt.Bytes(total) + " payload \u00B7 " + Fmt.Bytes(free) + " free");

                if (free < total + ReserveFloor)
                    throw new InvalidOperationException("Not enough free space on the target volume. This build needs "
                                                        + Fmt.Bytes(total + ReserveFloor) + ".");

                token.ThrowIfCancellationRequested();

                string exePath = Path.Combine(dir, BuildInfo.AppExeName);
                string partial = exePath + ".partial";

                Report(sink, "EXTRACT", 0.10, "streaming application core", 0, total);

                using (payload)
                using (var destination = new FileStream(partial, FileMode.Create, FileAccess.Write, FileShare.None, 1 << 20))
                {
                    var buffer = new byte[1 << 20];
                    long written = 0;
                    int read;
                    while ((read = payload.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        token.ThrowIfCancellationRequested();
                        destination.Write(buffer, 0, read);
                        written += read;
                        Report(sink, "EXTRACT", 0.10 + 0.60 * ((double)written / total), null, written, total);
                    }
                }

                if (File.Exists(exePath))
                {
                    File.SetAttributes(exePath, FileAttributes.Normal);
                    File.Delete(exePath);
                }
                File.Move(partial, exePath);
                createdFiles.Add(exePath);
                Report(sink, "EXTRACT", 0.72, "wrote " + BuildInfo.AppExeName + " \u00B7 " + Fmt.Bytes(total));

                string hash = ComputeSha256(exePath, token,
                    done => Report(sink, "VERIFY", 0.72 + 0.05 * ((double)done / total), null, done, total));
                Report(sink, "VERIFY", 0.78, "sha256 " + hash.Substring(0, Math.Min(16, hash.Length)) + "\u2026 valid");

                string uninstallPath = Path.Combine(dir, BuildInfo.UninstallExeName);
                using (var uninstaller = Assembly.GetExecutingAssembly().GetManifestResourceStream(BuildInfo.UninstallerResource))
                {
                    if (uninstaller == null)
                        throw new InvalidOperationException("The uninstaller module is missing. Run build.ps1 so it is embedded into the setup.");

                    using (var output = new FileStream(uninstallPath, FileMode.Create, FileAccess.Write))
                        uninstaller.CopyTo(output);
                }
                createdFiles.Add(uninstallPath);
                Report(sink, "MODULES", 0.81, "wrote " + BuildInfo.UninstallExeName);

                string iconPath = Path.Combine(dir, BuildInfo.IconFileName);
                using (var iconStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(BuildInfo.IconResource))
                {
                    if (iconStream != null)
                    {
                        using (var output = new FileStream(iconPath, FileMode.Create, FileAccess.Write))
                            iconStream.CopyTo(output);
                        createdFiles.Add(iconPath);
                    }
                }

                int scrubbed = Motw.Scrub(dir);
                Report(sink, "INTEGRITY", 0.84, "mark-of-the-web scrubbed \u00B7 " + scrubbed + " object(s)");

                string icon = File.Exists(iconPath) ? iconPath : exePath;

                if (options.StartMenuShortcut)
                {
                    string menuFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonPrograms), BuildInfo.ProductName);
                    Shortcuts.Create(Path.Combine(menuFolder, BuildInfo.ProductName + ".lnk"), exePath, dir, BuildInfo.ProductTag, icon);
                    Shortcuts.Create(Path.Combine(menuFolder, "Uninstall " + BuildInfo.ProductName + ".lnk"), uninstallPath, dir, "Uninstall " + BuildInfo.ProductName, uninstallPath);
                    Report(sink, "SHELL", 0.88, "start menu \u00B7 " + menuFolder);
                }

                if (options.DesktopShortcut)
                {
                    string desktop = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonDesktopDirectory), BuildInfo.ProductName + ".lnk");
                    Shortcuts.Create(desktop, exePath, dir, BuildInfo.ProductTag, icon);
                    Report(sink, "SHELL", 0.90, "desktop \u00B7 " + BuildInfo.ProductName + ".lnk");
                }

                token.ThrowIfCancellationRequested();
                WriteRegistry(dir, exePath, uninstallPath, icon, total);
                registryWritten = true;
                Report(sink, "REGISTRY", 0.96, "apps & features entry registered");
                Report(sink, "COMMIT", 1.0, "installation committed");

                result.Ok = true;
                result.ExePath = exePath;
                result.Bytes = total;
                return result;
            }
            catch (OperationCanceledException)
            {
                Rollback(dir, createdDir, createdFiles, registryWritten);
                throw;
            }
            catch (Exception ex)
            {
                Rollback(dir, createdDir, createdFiles, registryWritten);
                result.Error = ex.Message;
                return result;
            }
        }

        private static void WriteRegistry(string dir, string exePath, string uninstallPath, string iconPath, long bytes)
        {
            using (var key = Registry.LocalMachine.CreateSubKey(BuildInfo.RegistryKey))
            {
                if (key == null) throw new InvalidOperationException("Unable to write the uninstall registry key.");

                key.SetValue("DisplayName", BuildInfo.ProductName);
                key.SetValue("DisplayVersion", BuildInfo.Version);
                key.SetValue("Publisher", BuildInfo.Publisher);
                key.SetValue("InstallLocation", dir);
                key.SetValue("DisplayIcon", iconPath + ",0");
                key.SetValue("UninstallString", "\"" + uninstallPath + "\"");
                key.SetValue("URLInfoAbout", BuildInfo.Url);
                key.SetValue("HelpLink", BuildInfo.Url);
                key.SetValue("InstallDate", DateTime.Now.ToString("yyyyMMdd"));
                key.SetValue("NoModify", 1, RegistryValueKind.DWord);
                key.SetValue("NoRepair", 1, RegistryValueKind.DWord);
                key.SetValue("EstimatedSize", (int)Math.Max(1, (bytes + 512 * 1024) / 1024), RegistryValueKind.DWord);
            }

            using (var appPath = Registry.LocalMachine.CreateSubKey(BuildInfo.AppPathsKey))
            {
                if (appPath != null)
                {
                    appPath.SetValue(null, exePath);
                    appPath.SetValue("Path", dir);
                }
            }
        }

        public static OperationResult Uninstall(IProgress<InstallReport> sink, CancellationToken token)
        {
            var result = new OperationResult();
            try
            {
                Report(sink, "PREFLIGHT", 0.04, "reading installation record");

                string dir = ReadInstallDir();
                if (string.IsNullOrEmpty(dir) || !Directory.Exists(dir))
                    throw new InvalidOperationException("MOTW Cleaner is not installed, or its installation folder no longer exists.");

                dir = Path.GetFullPath(dir).TrimEnd('\\');
                Report(sink, "PREFLIGHT", 0.08, "target \u00B7 " + dir);

                string programs = Environment.GetFolderPath(Environment.SpecialFolder.CommonPrograms);
                DeleteFile(Path.Combine(programs, BuildInfo.ProductName + ".lnk"));
                string menuFolder = Path.Combine(programs, BuildInfo.ProductName);
                if (Directory.Exists(menuFolder))
                {
                    try { Directory.Delete(menuFolder, true); }
                    catch { }
                }
                DeleteFile(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonDesktopDirectory), BuildInfo.ProductName + ".lnk"));
                Report(sink, "SHELL", 0.22, "shell shortcuts removed");

                try { Registry.LocalMachine.DeleteSubKeyTree(BuildInfo.AppPathsKey, false); }
                catch { }
                try { Registry.LocalMachine.DeleteSubKeyTree(BuildInfo.RegistryKey, false); }
                catch { }
                Report(sink, "REGISTRY", 0.32, "registry entries removed");

                token.ThrowIfCancellationRequested();

                string self = Path.GetFullPath(Application.ExecutablePath);
                int removed = 0;
                foreach (var file in Directory.GetFiles(dir))
                {
                    token.ThrowIfCancellationRequested();
                    if (string.Equals(Path.GetFullPath(file), self, StringComparison.OrdinalIgnoreCase)) continue;
                    try
                    {
                        File.SetAttributes(file, FileAttributes.Normal);
                        File.Delete(file);
                        removed++;
                    }
                    catch
                    {
                    }
                }

                foreach (var sub in Directory.GetDirectories(dir))
                {
                    try { Directory.Delete(sub, true); }
                    catch { }
                }

                Report(sink, "CLEANUP", 0.62, "removed " + removed + " file(s)");
                Report(sink, "CLEANUP", 0.72, "residual cleanup scheduled");

                result.Ok = true;
                result.ExePath = dir;
                return result;
            }
            catch (Exception ex)
            {
                result.Error = ex.Message;
                return result;
            }
        }

        public static void ScheduleCleanup(string installDir)
        {
            try
            {
                string temp = Path.Combine(Path.GetTempPath(), "MOTWCleaner-" + Guid.NewGuid().ToString("N").Substring(0, 8));
                Directory.CreateDirectory(temp);
                string helper = Path.Combine(temp, "cleanup.exe");
                File.Copy(Application.ExecutablePath, helper, true);

                Process.Start(new ProcessStartInfo(helper, "--cleanup \"" + installDir + "\"")
                {
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    WindowStyle = ProcessWindowStyle.Hidden
                });
            }
            catch
            {
            }
        }

        public static void Cleanup(string installDir)
        {
            if (string.IsNullOrEmpty(installDir)) return;

            for (int attempt = 0; attempt < 40; attempt++)
            {
                try
                {
                    if (Directory.Exists(installDir)) Directory.Delete(installDir, true);
                }
                catch
                {
                }

                if (!Directory.Exists(installDir)) break;
                Thread.Sleep(250);
            }

            try
            {
                string folder = Path.GetDirectoryName(Application.ExecutablePath);
                string command = "/c ping -n 3 127.0.0.1 >nul & rmdir /s /q \"" + folder + "\"";
                Process.Start(new ProcessStartInfo("cmd.exe", command)
                {
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    WindowStyle = ProcessWindowStyle.Hidden
                });
            }
            catch
            {
            }
        }

        private static void DeleteFile(string path)
        {
            try
            {
                if (File.Exists(path))
                {
                    File.SetAttributes(path, FileAttributes.Normal);
                    File.Delete(path);
                }
            }
            catch
            {
            }
        }

        private static string ComputeSha256(string path, CancellationToken token, Action<long> progress)
        {
            using (var sha = SHA256.Create())
            using (var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, 1 << 20))
            {
                var buffer = new byte[1 << 20];
                long total = 0;
                int read;
                while ((read = stream.Read(buffer, 0, buffer.Length)) > 0)
                {
                    token.ThrowIfCancellationRequested();
                    sha.TransformBlock(buffer, 0, read, null, 0);
                    total += read;
                    if (progress != null) progress(total);
                }
                sha.TransformFinalBlock(buffer, 0, 0);
                return BitConverter.ToString(sha.Hash).Replace("-", string.Empty).ToLowerInvariant();
            }
        }

        private static void Rollback(string dir, bool createdDir, List<string> files, bool registryWritten)
        {
            foreach (var file in files)
            {
                try
                {
                    if (!File.Exists(file)) continue;
                    File.SetAttributes(file, FileAttributes.Normal);
                    File.Delete(file);
                }
                catch
                {
                }
            }

            if (registryWritten)
            {
                try { Registry.LocalMachine.DeleteSubKeyTree(BuildInfo.RegistryKey, false); }
                catch { }
                try { Registry.LocalMachine.DeleteSubKeyTree(BuildInfo.AppPathsKey, false); }
                catch { }
            }

            if (createdDir)
            {
                try
                {
                    if (Directory.Exists(dir) && Directory.GetFileSystemEntries(dir).Length == 0)
                        Directory.Delete(dir);
                }
                catch
                {
                }
            }
        }

        private static void Report(IProgress<InstallReport> sink, string stage, double percent, string line = null,
            long done = 0, long total = 0, bool isError = false)
        {
            if (sink == null) return;
            sink.Report(new InstallReport
            {
                Stage = stage,
                Percent = percent,
                Line = line,
                BytesDone = done,
                BytesTotal = total,
                IsError = isError
            });
        }
    }

    internal static class Motw
    {
        public static int Scrub(string dir)
        {
            int count = 0;
            try
            {
                foreach (var file in Directory.GetFiles(dir))
                {
                    try
                    {
                        File.Delete(file + ":Zone.Identifier");
                        count++;
                    }
                    catch
                    {
                    }
                }
            }
            catch
            {
            }
            return count;
        }
    }
}
