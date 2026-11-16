using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MotwSetup
{
    internal sealed class SetupForm : InstallerWindow
    {
        private enum Phase
        {
            Scan,
            Configure,
            Install,
            Complete,
            Failed
        }

        private sealed class ScanRow
        {
            public string Label;
            public string Value;
            public bool Ready;
        }

        private const int RailW = 248;
        private const int CX = 300;
        private const int ContentTop = 106;
        private const int FooterPad = 44;

        private readonly StepRail _rail;
        private readonly ProgressRing _ring;
        private readonly NeonProgress _bar;
        private readonly ConsolePanel _log;
        private readonly NeonInput _path;
        private readonly NeonButton _browse;
        private readonly NeonButton _primary;
        private readonly NeonButton _secondary;
        private readonly NeonButton _abort;
        private readonly NeonToggle _desktop;
        private readonly NeonToggle _startMenu;
        private readonly NeonToggle _launch;
        private readonly List<ScanRow> _rows = new List<ScanRow>();

        private Phase _phase = Phase.Scan;
        private bool _scanning;
        private bool _scanDone;
        private int _scanIndex;
        private string _existing;
        private double _progress;
        private string _stage = "PREFLIGHT";
        private long _bytesDone;
        private long _bytesTotal;
        private TimeSpan _elapsed;
        private Stopwatch _clock;
        private CancellationTokenSource _cts;
        private string _error;
        private string _installedExe;
        private string _installedDir;
        private long _installedBytes;

        public SetupForm()
            : base(1040, 660, BuildInfo.Title, "SETUP UTILITY \u00B7 v" + BuildInfo.Version)
        {
            _rail = new StepRail
            {
                Steps = new[] { "INITIALIZE", "CONFIGURE", "DEPLOY", "COMPLETE" },
                Footer = "MOTW // SETUP",
                Location = new Point(0, HeaderH),
                Size = new Size(RailW, ClientSize.Height - HeaderH - 96)
            };
            Controls.Add(_rail);

            _ring = new ProgressRing
            {
                Caption = "PREFLIGHT",
                Location = new Point(CX, 176),
                Size = new Size(190, 190)
            };
            Controls.Add(_ring);

            _bar = new NeonProgress { Location = new Point(CX, 240), Size = new Size(CW, 10) };
            Controls.Add(_bar);

            _log = new ConsolePanel { Location = new Point(CX, 288), Size = new Size(CW, 266) };
            Controls.Add(_log);

            _path = new NeonInput
            {
                Location = new Point(CX, 210),
                Size = new Size(CW - 120, 44),
                Placeholder = InstallEngine.DefaultInstallDir()
            };
            _path.Text = InstallEngine.DefaultInstallDir();
            Controls.Add(_path);

            _browse = new NeonButton
            {
                Text = "BROWSE",
                Variant = ButtonVariant.Ghost,
                Location = new Point(CX + CW - 108, 210),
                Size = new Size(108, 44)
            };
            _browse.Click += (sender, args) => Browse();
            Controls.Add(_browse);

            _desktop = new NeonToggle { Text = "Create a desktop shortcut", Location = new Point(CX, 326) };
            _startMenu = new NeonToggle { Text = "Add MOTW Cleaner to the Start menu", Location = new Point(CX, 360) };
            _launch = new NeonToggle { Text = "Launch MOTW Cleaner after setup", Location = new Point(CX, 394) };
            Controls.Add(_desktop);
            Controls.Add(_startMenu);
            Controls.Add(_launch);

            _primary = new NeonButton { Text = "CONTINUE", Enabled = false };
            _primary.Click += (sender, args) => PrimaryAction();

            _secondary = new NeonButton { Text = "BACK", Variant = ButtonVariant.Ghost };
            _secondary.Click += (sender, args) => SecondaryAction();

            _abort = new NeonButton { Text = "ABORT", Variant = ButtonVariant.Ghost };
            _abort.Click += (sender, args) => Abort();

            Controls.Add(_primary);
            Controls.Add(_secondary);
            Controls.Add(_abort);

            LayoutButtons();
            UpdateControls();
        }

        private int CW
        {
            get { return ClientSize.Width - CX - FooterPad; }
        }

        protected override async void OnShown(EventArgs e)
        {
            base.OnShown(e);
#if DEBUG
            if (_renderFrozen) return;
#endif
            await RunScan();
        }

        private async Task RunScan()
        {
            if (_rows.Count == 0) BuildScanRows();

            _scanning = true;
            _scanDone = false;
            _scanIndex = 0;
            _phase = Phase.Scan;

            _rail.Active = 0;
            _ring.Reset();
            _ring.Spin = true;
            _ring.Value = 0;
            _ring.Caption = "PREFLIGHT";

            UpdateControls();
            Invalidate();

            for (int i = 0; i < _rows.Count; i++)
            {
                _scanIndex = i;
                _ring.Value = (double)i / _rows.Count;
                Invalidate();

                await Task.Delay(i == 0 ? 320 : 190);
                _rows[i].Ready = true;
                Invalidate();

                await Task.Delay(90);
            }

            _scanIndex = _rows.Count;
            _scanning = false;
            _scanDone = true;
            _ring.Value = 1;

            UpdateControls();
            Invalidate();
        }

        private void BuildScanRows()
        {
            _rows.Clear();
            _existing = InstallEngine.ReadInstalledVersion();

            string target = InstallEngine.DefaultInstallDir();

            _rows.Add(new ScanRow
            {
                Label = "HOST SYSTEM",
                Value = Native.OsDescription()
            });
            _rows.Add(new ScanRow
            {
                Label = "PRIVILEGE LEVEL",
                Value = Native.IsElevated ? "Administrator \u00B7 token elevated" : "Standard user \u00B7 elevation required"
            });
            _rows.Add(new ScanRow
            {
                Label = "TARGET VOLUME",
                Value = Path.GetPathRoot(target) + " \u00B7 " + Fmt.Bytes(InstallEngine.FreeSpaceFor(target)) + " free"
            });
            _rows.Add(new ScanRow
            {
                Label = "DEPLOYMENT PACKAGE",
                Value = "Embedded \u00B7 " + Fmt.Bytes(InstallEngine.PayloadSize)
            });
            _rows.Add(new ScanRow
            {
                Label = "EXISTING INSTALLATION",
                Value = string.IsNullOrEmpty(_existing)
                    ? "None detected"
                    : "v" + _existing + " \u00B7 " + InstallEngine.ReadInstallDir()
            });
        }

        private void LayoutButtons()
        {
            int y = ClientSize.Height - 72;

            _primary.Size = new Size(190, 44);
            _primary.Location = new Point(ClientSize.Width - FooterPad - 190, y);

            _secondary.Size = new Size(132, 44);
            _secondary.Location = new Point(_primary.Left - 12 - 132, y);

            _abort.Size = new Size(132, 44);
            _abort.Location = new Point(ClientSize.Width - FooterPad - 132, y);
        }

        private void UpdateControls()
        {
            bool scan = _phase == Phase.Scan;
            bool configure = _phase == Phase.Configure;
            bool install = _phase == Phase.Install;
            bool complete = _phase == Phase.Complete;
            bool failed = _phase == Phase.Failed;

            _ring.Visible = scan || complete;
            _bar.Visible = install;
            _log.Visible = install;

            _path.Visible = configure;
            _browse.Visible = configure;
            _desktop.Visible = configure;
            _startMenu.Visible = configure;

            _launch.Visible = configure || complete;
            _launch.Location = complete ? new Point(CX + 232, 442) : new Point(CX, 394);
            _launch.Text = complete ? "Launch MOTW Cleaner now" : "Launch MOTW Cleaner after setup";

            _primary.Visible = !install;
            _secondary.Visible = configure || complete || failed;
            _abort.Visible = install;

            if (scan)
            {
                _primary.Text = "CONTINUE";
                _primary.Enabled = _scanDone;
                _secondary.Text = "BACK";
            }
            else if (configure)
            {
                _primary.Text = string.IsNullOrEmpty(_existing) ? "INSTALL" : "UPGRADE";
                _primary.Enabled = true;
                _primary.Variant = ButtonVariant.Primary;
                _secondary.Text = "BACK";
            }
            else if (complete)
            {
                _primary.Text = "FINISH";
                _primary.Enabled = true;
                _primary.Variant = ButtonVariant.Primary;
                _secondary.Text = "CLOSE";
            }
            else if (failed)
            {
                _primary.Text = "RETRY";
                _primary.Enabled = true;
                _primary.Variant = ButtonVariant.Primary;
                _secondary.Text = "CLOSE";
            }

            _rail.Invalidate();
        }

        private void PrimaryAction()
        {
            switch (_phase)
            {
                case Phase.Scan:
                    if (_scanDone) GoConfigure();
                    break;
                case Phase.Configure:
                    StartInstall();
                    break;
                case Phase.Complete:
                    Finish();
                    break;
                case Phase.Failed:
                    StartInstall();
                    break;
            }
        }

        private void SecondaryAction()
        {
            if (_phase == Phase.Configure)
            {
                _phase = Phase.Scan;
                _rail.Active = 0;
                _ring.Reset();
                _ring.Spin = true;
                _ring.Value = _scanDone ? 1 : 0;
                UpdateControls();
                Invalidate();
                return;
            }

            Close();
        }

        private void Abort()
        {
            if (_cts != null && !_cts.IsCancellationRequested) _cts.Cancel();
        }

        private void Browse()
        {
            using (var dialog = new FolderBrowserDialog())
            {
                dialog.Description = "Select the folder where MOTW Cleaner should be installed.";
                dialog.ShowNewFolderButton = true;

                string current = (_path.Text ?? string.Empty).Trim();
                if (Directory.Exists(current)) dialog.SelectedPath = current;

                if (dialog.ShowDialog(this) != DialogResult.OK || string.IsNullOrWhiteSpace(dialog.SelectedPath)) return;

                string chosen = dialog.SelectedPath.TrimEnd('\\');
                if (!string.Equals(Path.GetFileName(chosen), BuildInfo.ProductName, StringComparison.OrdinalIgnoreCase))
                    chosen = Path.Combine(chosen, BuildInfo.ProductName);

                _path.Text = chosen;
            }
        }

        private void GoConfigure()
        {
            _phase = Phase.Configure;
            _rail.Active = 1;

            if (string.IsNullOrWhiteSpace(_path.Text)) _path.Text = InstallEngine.DefaultInstallDir();

            UpdateControls();
            Invalidate();
            _path.FocusInput();
        }

        private async void StartInstall()
        {
            _phase = Phase.Install;
            _rail.Active = 2;

            _progress = 0;
            _stage = "PREFLIGHT";
            _bytesDone = 0;
            _bytesTotal = InstallEngine.PayloadSize;
            _elapsed = TimeSpan.Zero;
            _error = null;

            _log.Clear();
            _log.Append("deployment sequence started");

            _clock = Stopwatch.StartNew();
            _cts = new CancellationTokenSource();

            UpdateControls();
            Invalidate();

            var options = new InstallOptions
            {
                InstallDir = _path.Text,
                DesktopShortcut = _desktop.Checked,
                StartMenuShortcut = _startMenu.Checked
            };

            var reporter = new Progress<InstallReport>(OnInstallReport);

            try
            {
                var result = await Task.Run(() => InstallEngine.Install(options, reporter, _cts.Token));
                if (_clock != null) _clock.Stop();

                if (!result.Ok) throw new InvalidOperationException(result.Error);

                _installedExe = result.ExePath;
                _installedDir = Path.GetDirectoryName(result.ExePath);
                _installedBytes = result.Bytes;

                _ring.Reset();
                _ring.Spin = false;
                _ring.Value = 1;
                _ring.Caption = "VERIFIED";

                _phase = Phase.Complete;
                _rail.Active = _rail.Steps.Length;

                UpdateControls();
                Invalidate();
            }
            catch (OperationCanceledException)
            {
                Halt("Deployment aborted. Partial files were removed.");
            }
            catch (Exception ex)
            {
                Halt(ex.Message);
            }
        }

        private void Halt(string message)
        {
            if (_clock != null) _clock.Stop();

            _error = message;
            _phase = Phase.Failed;
            _rail.Active = 2;

            _ring.Reset();
            _ring.Spin = false;
            _log.Append("halted \u00B7 " + message, Theme.Danger);

            UpdateControls();
            Invalidate();
        }

        private void OnInstallReport(InstallReport report)
        {
            if (report == null) return;

            if (!string.IsNullOrEmpty(report.Stage)) _stage = report.Stage;
            if (report.BytesTotal > 0) _bytesTotal = report.BytesTotal;
            if (report.BytesDone > 0) _bytesDone = report.BytesDone;
            _progress = report.Percent;
            _bar.Value = _progress;

            if (!string.IsNullOrEmpty(report.Line))
            {
                Color tone = report.IsError
                    ? Theme.Danger
                    : report.Line.IndexOf("committed", StringComparison.OrdinalIgnoreCase) >= 0
                        ? Theme.Lime
                        : Theme.Muted;
                _log.Append(report.Line, tone);
            }

            Invalidate();
        }

        private void Finish()
        {
            if (_launch.Checked && !string.IsNullOrEmpty(_installedExe))
            {
                try
                {
                    Process.Start(new ProcessStartInfo(_installedExe)
                    {
                        WorkingDirectory = _installedDir,
                        UseShellExecute = true
                    });
                }
                catch
                {
                }
            }

            Close();
        }

        protected override void OnCloseRequested()
        {
            if (_phase == Phase.Install && _cts != null && !_cts.IsCancellationRequested)
            {
                _cts.Cancel();
                return;
            }

            base.OnCloseRequested();
        }

        protected override bool OnPrimaryKey()
        {
            if (!_primary.Visible || !_primary.Enabled) return false;
            PrimaryAction();
            return true;
        }

        protected override void OnTick()
        {
            if (_phase == Phase.Install && _clock != null) _elapsed = _clock.Elapsed;
            _ring.Advance(0.033f);
            _bar.Advance(0.033f);
            _rail.Advance(0.033f);
        }

        protected override void PaintContent(Graphics g)
        {
            switch (_phase)
            {
                case Phase.Scan:
                    PaintScan(g);
                    break;
                case Phase.Configure:
                    PaintConfigure(g);
                    break;
                case Phase.Install:
                    PaintInstall(g);
                    break;
                case Phase.Complete:
                    PaintComplete(g);
                    break;
                default:
                    PaintFailed(g);
                    break;
            }
        }

        private void DrawHead(Graphics g, string title, string subtitle)
        {
            Theme.Draw(g, title, Theme.Head(20f), Theme.Text, CX - 2f, ContentTop - 8f);
            Theme.Draw(g, subtitle, Theme.Ui(9.2f), Theme.Muted, CX, ContentTop + 34f);
            Theme.Stroke(g, CX, ContentTop + 64f, ClientSize.Width - FooterPad, ContentTop + 64f, Theme.LineSoft, 1f);
        }

        private void PaintScan(Graphics g)
        {
            DrawHead(g, "SYSTEM INITIALIZATION", "Preparing this machine for deployment.");

            for (int i = 0; i < _rows.Count; i++)
            {
                var row = _rows[i];
                float y = 196f + i * 48f;
                bool current = !row.Ready && i == _scanIndex && _scanning;

                Color labelColor = row.Ready ? Theme.Dim : current ? Theme.Alpha(Theme.Accent, 200) : Theme.Alpha(Theme.Dim, 120);
                Theme.Tracked(g, row.Label, Theme.Mono(7f), labelColor, CX + 238f, y, 1.5f);

                string value = row.Ready ? row.Value : current ? "checking\u2026" : "\u2014";
                Color valueColor = row.Ready ? Theme.Text : current ? Theme.Muted : Theme.Alpha(Theme.Dim, 140);
                Theme.DrawClipped(g, value, Theme.Ui(9.8f), valueColor,
                    new RectangleF(CX + 238f, y + 14f, ClientSize.Width - FooterPad - (CX + 238f), 20f));

                Color dotColor = row.Ready ? Theme.Lime : current ? Theme.Accent : Theme.Line;
                int alpha = row.Ready ? 210 : current ? (int)(120 + 110 * Math.Abs(Math.Sin(Time * 3.2f))) : 255;
                using (var dot = new SolidBrush(Theme.Alpha(dotColor, alpha)))
                    g.FillEllipse(dot, CX + 222f, y + 6f, 5f, 5f);

                if (i < _rows.Count - 1)
                    Theme.Stroke(g, CX + 238f, y + 38f, ClientSize.Width - FooterPad, y + 38f, Theme.LineSoft, 1f);
            }

            PaintFooter(g, _scanDone ? "ENVIRONMENT VERIFIED" : "PREFLIGHT SWEEP RUNNING\u2026",
                _scanDone ? Theme.Lime : Theme.Accent);
        }

        private void PaintConfigure(Graphics g)
        {
            bool upgrade = !string.IsNullOrEmpty(_existing);

            DrawHead(g,
                upgrade ? "UPGRADE INSTALLATION" : "INSTALLATION PROFILE",
                upgrade
                    ? "An existing build was detected. This deployment replaces it in place."
                    : "Choose the target location and how MOTW Cleaner hooks into Windows.");

            DrawSectionLabel(g, "INSTALL LOCATION", CX, 192);

            long required = InstallEngine.PayloadSize + 32L * 1024 * 1024;
            Theme.Draw(g, "Requires " + Fmt.Bytes(required) + " free \u00B7 installed for all users",
                Theme.Ui(8.6f), Theme.Dim, CX, 262);

            Theme.Stroke(g, CX, 288, CX + CW, 288, Theme.LineSoft, 1f);
            DrawSectionLabel(g, "OPTIONS", CX, 306);

            var panel = new RectangleF(CX, 436, CW, 118);
            using (var path = Theme.Cut(panel, 10f))
            {
                using (var fill = new SolidBrush(Theme.Alpha(Theme.Panel, 165)))
                    g.FillPath(fill, path);
                using (var pen = new Pen(Theme.LineSoft, 1f))
                    g.DrawPath(pen, path);
            }

            DrawSectionLabel(g, "PACKAGE MANIFEST", CX + 18, 450);

            int shortcuts = (_desktop.Checked ? 1 : 0) + (_startMenu.Checked ? 2 : 0);
            ManifestRow(g, CX + 18, 478, "APPLICATION CORE", Fmt.Bytes(InstallEngine.PayloadSize));
            ManifestRow(g, CX + 18, 508, "SHELL INTEGRATION", shortcuts == 0 ? "none" : shortcuts + " shortcuts");
            ManifestRow(g, CX + CW / 2f + 6f, 478, "UNINSTALLER", Fmt.Bytes(Math.Max(1, InstallEngine.UninstallerSize)));
            ManifestRow(g, CX + CW / 2f + 6f, 508, "REGISTRY", "2 keys");

            PaintFooter(g, upgrade ? "READY TO UPGRADE" : "PROFILE READY", Theme.Accent);
        }

        private void ManifestRow(Graphics g, float x, float y, string label, string value)
        {
            Theme.Tracked(g, label, Theme.Mono(6.9f), Theme.Alpha(Theme.Dim, 220), x, y, 1.3f);
            Theme.Draw(g, value, Theme.Ui(8.8f), Theme.Muted, x + 148f, y - 4f);
        }

        private void PaintInstall(Graphics g)
        {
            DrawHead(g, "DEPLOYING PAYLOAD", "Streaming the application core into the target volume.");

            string percent = ((int)Math.Round(_progress * 100.0)).ToString();
            var big = Theme.Head(38f);
            Theme.Draw(g, percent, big, Theme.Text, CX - 2f, 172f);
            float width = g.MeasureString(percent, big).Width;
            Theme.Draw(g, "%", Theme.Head(15f), Theme.Alpha(Theme.Accent, 200), CX + width, 194f);

            Theme.Tracked(g, _stage, Theme.Mono(7.4f), Theme.Lime, CX + width + 26f, 200f, 1.7f);

            string written = Fmt.Bytes(_bytesDone) + " / " + Fmt.Bytes(_bytesTotal);
            float writtenWidth = Theme.TrackedWidth(g, written, Theme.Mono(7.2f), 1.2f);
            Theme.Tracked(g, written, Theme.Mono(7.2f), Theme.Muted, ClientSize.Width - FooterPad - writtenWidth, 182f, 1.2f);

            string elapsed = "ELAPSED " + Fmt.Clock(_elapsed);
            float elapsedWidth = Theme.TrackedWidth(g, elapsed, Theme.Mono(7.2f), 1.2f);
            Theme.Tracked(g, elapsed, Theme.Mono(7.2f), Theme.Dim, ClientSize.Width - FooterPad - elapsedWidth, 200f, 1.2f);

            PaintFooter(g, "WRITING " + Fmt.Bytes(_bytesDone), Theme.Accent);
        }

        private void PaintComplete(Graphics g)
        {
            DrawHead(g, "INSTALLATION COMPLETE", "MOTW Cleaner is registered and ready to unblock downloaded files.");

            float x = CX + 232f;
            Theme.Draw(g, BuildInfo.ProductName, Theme.Head(16f), Theme.Text, x, 182f);
            Theme.Draw(g, BuildInfo.ProductTag + " \u00B7 v" + BuildInfo.Version, Theme.Ui(9.2f), Theme.Muted, x, 210f);

            Detail(g, x, 262f, "LOCATION", _installedDir);
            Detail(g, x, 302f, "DISK USAGE", Fmt.Bytes(_installedBytes));
            Detail(g, x, 342f, "SHELL", "2 shortcuts \u00B7 apps & features");

            Theme.Stroke(g, x, 390f, ClientSize.Width - FooterPad, 390f, Theme.LineSoft, 1f);
            Theme.Draw(g, "Right-click any downloaded file to unblock it faster.", Theme.Ui(8.8f), Theme.Dim, x, 404f);

            PaintFooter(g, "DEPLOYMENT VERIFIED", Theme.Lime);
        }

        private void Detail(Graphics g, float x, float y, string label, string value)
        {
            Theme.Tracked(g, label, Theme.Mono(6.9f), Theme.Dim, x, y, 1.4f);
            Theme.DrawClipped(g, value ?? "\u2014", Theme.Ui(9.4f), Theme.Text,
                new RectangleF(x, y + 14f, ClientSize.Width - FooterPad - x, 20f));
        }

        private void PaintFailed(Graphics g)
        {
            DrawHead(g, "DEPLOYMENT HALTED", "The installer stopped before the payload was fully committed.");

            var warn = new RectangleF(CX, 182f, 54f, 54f);
            using (var path = Theme.Round(warn, 12f))
            {
                using (var fill = new SolidBrush(Theme.Alpha(Theme.Warn, 26)))
                    g.FillPath(fill, path);
                using (var pen = new Pen(Theme.Alpha(Theme.Warn, 160), 1f))
                    g.DrawPath(pen, path);
            }

            using (var pen = new Pen(Theme.Warn, 2.4f) { StartCap = LineCap.Round, EndCap = LineCap.Round })
            {
                g.DrawLine(pen, CX + 27f, 195f, CX + 27f, 215f);
                g.DrawEllipse(pen, CX + 25f, 221f, 4f, 4f);
            }

            Theme.Draw(g, "INSTALLATION ABORTED", Theme.UiBold(10.5f), Theme.Warn, CX + 72f, 194f);
            Theme.Wrapped(g, _error ?? "Unknown error.", Theme.Mono(8.4f), Theme.Muted,
                new RectangleF(CX + 72f, 216f, ClientSize.Width - FooterPad - (CX + 72f), 90f));

            PaintFooter(g, "SEQUENCE HALTED", Theme.Danger);
        }

#if DEBUG
        private bool _renderFrozen;

        internal void PrepareForRender(string phase)
        {
            _renderFrozen = true;
            BuildScanRows();
            for (int i = 0; i < _rows.Count; i++) _rows[i].Ready = true;
            _scanDone = true;
            _scanning = false;
            _scanIndex = _rows.Count;

            _phase = Phase.Configure;
            _rail.Active = 1;

            if (string.Equals(phase, "install", StringComparison.OrdinalIgnoreCase))
            {
                _phase = Phase.Install;
                _rail.Active = 2;
                _progress = 0.62;
                _bar.Value = _progress;
                _bytesDone = (long)(InstallEngine.PayloadSize * 0.62);
                _bytesTotal = InstallEngine.PayloadSize;
                _stage = "EXTRACT";
                _elapsed = TimeSpan.FromSeconds(2.4);
                _log.Append("elevation token verified");
                _log.Append("target \u00B7 " + InstallEngine.DefaultInstallDir());
                _log.Append("68.7 MB payload \u00B7 118.4 GB free");
                _log.Append("streaming application core");
                _log.Append("wrote MOTW Cleaner.exe \u00B7 68.7 MB");
                _log.Append("sha256 1f4c8be9a0d25733\u2026 valid", Theme.Lime);
            }
            else if (string.Equals(phase, "complete", StringComparison.OrdinalIgnoreCase))
            {
                _phase = Phase.Complete;
                _rail.Active = _rail.Steps.Length;
                _installedDir = InstallEngine.DefaultInstallDir();
                _installedExe = Path.Combine(_installedDir, BuildInfo.AppExeName);
                _installedBytes = InstallEngine.PayloadSize;
                _ring.Value = 1;
                _ring.Complete = true;
                _ring.Spin = false;
                _ring.Caption = "VERIFIED";
            }
            else if (string.Equals(phase, "failed", StringComparison.OrdinalIgnoreCase))
            {
                _phase = Phase.Failed;
                _rail.Active = 2;
                _error = "Access to the path 'C:\\Program Files\\MOTW Cleaner\\MOTW Cleaner.exe' is denied.";
            }
            else if (string.Equals(phase, "scan", StringComparison.OrdinalIgnoreCase))
            {
                _phase = Phase.Scan;
                _rail.Active = 0;
                _ring.Value = 1;
                _scanDone = true;
            }

            UpdateControls();
            OnTick();
        }
#endif
    }
}
