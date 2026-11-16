using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MotwSetup
{
    internal sealed class UninstallForm : InstallerWindow
    {
        private enum Phase
        {
            Confirm,
            Removing,
            Done,
            Failed
        }

        private const int CX = 56;
        private const int ContentTop = 106;
        private const int FooterPad = 44;

        private readonly ProgressRing _ring;
        private readonly NeonProgress _bar;
        private readonly ConsolePanel _log;
        private readonly NeonButton _primary;
        private readonly NeonButton _secondary;

        private Phase _phase = Phase.Confirm;
        private readonly string _dir;
        private readonly string _version;
        private readonly string _installed;
        private readonly long _size;

        private string _error;
        private string _block;
        private double _progress;
        private string _stage = "PREFLIGHT";
        private long _bytesDone;
        private long _bytesTotal;
        private TimeSpan _elapsed;
        private Stopwatch _clock;
        private CancellationTokenSource _cts;

        public UninstallForm()
            : base(880, 600, BuildInfo.Title, "UNINSTALLER \u00B7 v" + BuildInfo.Version)
        {
            _dir = InstallEngine.ReadInstallDir();
            _version = InstallEngine.ReadInstalledVersion();
            _installed = InstallEngine.ReadInstalledDate();
            _size = string.IsNullOrEmpty(_dir) ? 0 : InstallEngine.DirectorySize(_dir);

            _ring = new ProgressRing
            {
                Caption = "REMOVED",
                Location = new Point(CX, 180),
                Size = new Size(180, 180),
                Visible = false
            };
            Controls.Add(_ring);

            _bar = new NeonProgress { Location = new Point(CX, 226), Size = new Size(CW, 10), Visible = false };
            Controls.Add(_bar);

            _log = new ConsolePanel { Location = new Point(CX, 268), Size = new Size(CW, 240), Visible = false };
            Controls.Add(_log);

            _primary = new NeonButton { Text = "REMOVE", Variant = ButtonVariant.Danger, Size = new Size(190, 44) };
            _primary.Click += (sender, args) => PrimaryAction();

            _secondary = new NeonButton { Text = "CANCEL", Variant = ButtonVariant.Ghost, Size = new Size(132, 44) };
            _secondary.Click += (sender, args) => SecondaryAction();

            Controls.Add(_primary);
            Controls.Add(_secondary);

            LayoutButtons();

            if (string.IsNullOrEmpty(_dir) || !Directory.Exists(_dir))
            {
                _phase = Phase.Failed;
                _error = "MOTW Cleaner is not installed on this computer, or its installation folder was moved.";
            }

            UpdateControls();
        }

        private int CW
        {
            get { return ClientSize.Width - CX * 2; }
        }

        private void LayoutButtons()
        {
            int y = ClientSize.Height - 72;

            _primary.Size = new Size(190, 44);
            _primary.Location = new Point(ClientSize.Width - FooterPad - 190, y);

            _secondary.Size = new Size(132, 44);
            _secondary.Location = new Point(_primary.Left - 12 - 132, y);
        }

        private void UpdateControls()
        {
            bool confirm = _phase == Phase.Confirm;
            bool removing = _phase == Phase.Removing;
            bool done = _phase == Phase.Done;
            bool failed = _phase == Phase.Failed;

            _ring.Visible = done;
            _bar.Visible = removing;
            _log.Visible = removing;

            _primary.Visible = !removing;
            _secondary.Visible = !removing;

            if (confirm)
            {
                _primary.Text = "REMOVE";
                _primary.Variant = ButtonVariant.Danger;
                _primary.Enabled = true;
                _secondary.Text = "CANCEL";
            }
            else if (done)
            {
                _primary.Text = "CLOSE";
                _primary.Variant = ButtonVariant.Primary;
                _primary.Enabled = true;
                _secondary.Visible = false;
            }
            else if (failed)
            {
                _primary.Text = "CLOSE";
                _primary.Variant = ButtonVariant.Primary;
                _primary.Enabled = true;
                _secondary.Visible = false;
            }
        }

        private void PrimaryAction()
        {
            switch (_phase)
            {
                case Phase.Confirm:
                    StartRemove();
                    break;
                case Phase.Done:
                case Phase.Failed:
                    Close();
                    break;
            }
        }

        private void SecondaryAction()
        {
            Close();
        }

        private async void StartRemove()
        {
            if (InstallEngine.IsAppRunning())
            {
                _block = "MOTW Cleaner is running. Close it, then remove again.";
                Invalidate();
                return;
            }

            _block = null;
            _phase = Phase.Removing;
            _progress = 0;
            _stage = "PREFLIGHT";
            _bytesDone = 0;
            _bytesTotal = InstallEngine.DirectorySize(_dir);
            _elapsed = TimeSpan.Zero;

            _log.Clear();
            _log.Append("removal sequence started");

            _clock = Stopwatch.StartNew();
            _cts = new CancellationTokenSource();

            UpdateControls();
            Invalidate();

            var reporter = new Progress<InstallReport>(report =>
            {
                if (report == null) return;
                if (!string.IsNullOrEmpty(report.Stage)) _stage = report.Stage;
                if (report.BytesTotal > 0) _bytesTotal = report.BytesTotal;
                if (report.BytesDone > 0) _bytesDone = report.BytesDone;
                _progress = report.Percent;
                _bar.Value = _progress;
                if (!string.IsNullOrEmpty(report.Line))
                    _log.Append(report.Line, report.IsError ? Theme.Danger : Theme.Muted);
                Invalidate();
            });

            try
            {
                var result = await Task.Run(() => InstallEngine.Uninstall(reporter, _cts.Token));
                if (_clock != null) _clock.Stop();

                if (!result.Ok) throw new InvalidOperationException(result.Error);

                InstallEngine.ScheduleCleanup(_dir);

                _ring.Reset();
                _ring.Spin = false;
                _ring.Value = 1;
                _ring.Caption = "REMOVED";
                _phase = Phase.Done;

                UpdateControls();
                Invalidate();
            }
            catch (Exception ex)
            {
                if (_clock != null) _clock.Stop();
                _error = ex.Message;
                _phase = Phase.Failed;
                UpdateControls();
                Invalidate();
            }
        }

        protected override void OnCloseRequested()
        {
            if (_phase == Phase.Removing && _cts != null && !_cts.IsCancellationRequested)
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
            if (_phase == Phase.Removing && _clock != null) _elapsed = _clock.Elapsed;
            _ring.Advance(0.033f);
            _bar.Advance(0.033f);
        }

        protected override void PaintContent(Graphics g)
        {
            switch (_phase)
            {
                case Phase.Confirm:
                    PaintConfirm(g);
                    break;
                case Phase.Removing:
                    PaintRemoving(g);
                    break;
                case Phase.Done:
                    PaintDone(g);
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

        private void PaintConfirm(Graphics g)
        {
            DrawHead(g, "REMOVE MOTW CLEANER", "This deletes the installation, its shortcuts and the registry entries.");

            var panel = new RectangleF(CX, 192f, CW, 148f);
            using (var path = Theme.Cut(panel, 10f))
            {
                using (var fill = new SolidBrush(Theme.Alpha(Theme.Panel, 165)))
                    g.FillPath(fill, path);
                using (var pen = new Pen(Theme.LineSoft, 1f))
                    g.DrawPath(pen, path);
            }

            DrawSectionLabel(g, "INSTALLATION RECORD", CX + 20f, 208f);

            Record(g, CX + 20f, 238f, "LOCATION", _dir);
            Record(g, CX + 20f, 278f, "VERSION", string.IsNullOrEmpty(_version) ? "\u2014" : "v" + _version);
            Record(g, CX + CW / 2f + 6f, 238f, "INSTALLED", string.IsNullOrEmpty(_installed) ? "\u2014" : _installed);
            Record(g, CX + CW / 2f + 6f, 278f, "DISK USAGE", Fmt.Bytes(_size));

            Bullet(g, 362f, "Application files and shell shortcuts are deleted.");
            Bullet(g, 390f, "The Apps & features entry and App Paths registration are removed.");
            Bullet(g, 418f, "Files you already unblocked elsewhere are not affected.");

            if (_block != null)
                Theme.Draw(g, _block, Theme.Ui(9f), Theme.Danger, CX, 462f);

            PaintFooter(g, "CONFIRM TO CONTINUE", Theme.Warn);
        }

        private void Record(Graphics g, float x, float y, string label, string value)
        {
            Theme.Tracked(g, label, Theme.Mono(6.9f), Theme.Dim, x, y, 1.4f);
            Theme.DrawClipped(g, value ?? "\u2014", Theme.Ui(9.2f), Theme.Text,
                new RectangleF(x, y + 14f, CW / 2f - 26f, 20f));
        }

        private void Bullet(Graphics g, float y, string text)
        {
            var diamond = new[]
            {
                new PointF(CX + 4f, y + 1f),
                new PointF(CX + 9f, y + 6f),
                new PointF(CX + 4f, y + 11f),
                new PointF(CX - 1f, y + 6f)
            };
            using (var pen = new Pen(Theme.Alpha(Theme.Accent, 170), 1.2f))
                g.DrawPolygon(pen, diamond);
            Theme.Draw(g, text, Theme.Ui(9.2f), Theme.Muted, CX + 22f, y - 4f);
        }

        private void PaintRemoving(Graphics g)
        {
            DrawHead(g, "REMOVING INSTALLATION", "Deleting files, shortcuts and registry entries.");

            string percent = ((int)Math.Round(_progress * 100.0)).ToString();
            var big = Theme.Head(38f);
            Theme.Draw(g, percent, big, Theme.Text, CX - 2f, 158f);
            float width = g.MeasureString(percent, big).Width;
            Theme.Draw(g, "%", Theme.Head(15f), Theme.Alpha(Theme.Accent, 200), CX + width, 180f);
            Theme.Tracked(g, _stage, Theme.Mono(7.4f), Theme.Lime, CX + width + 26f, 184f, 1.7f);

            string elapsed = "ELAPSED " + Fmt.Clock(_elapsed);
            float elapsedWidth = Theme.TrackedWidth(g, elapsed, Theme.Mono(7.2f), 1.2f);
            Theme.Tracked(g, elapsed, Theme.Mono(7.2f), Theme.Dim, ClientSize.Width - FooterPad - elapsedWidth, 184f, 1.2f);

            PaintFooter(g, "REMOVAL IN PROGRESS", Theme.Accent);
        }

        private void PaintDone(Graphics g)
        {
            DrawHead(g, "REMOVAL COMPLETE", "MOTW Cleaner has been removed from this computer.");

            float x = CX + 218f;
            Theme.Draw(g, "Everything is clean.", Theme.Head(15f), Theme.Text, x, 196f);
            Theme.Draw(g, "The installation folder, shortcuts and registry entries are gone.",
                Theme.Ui(9.2f), Theme.Muted, x, 226f);
            Theme.Draw(g, "A background helper finishes deleting the last locked file.",
                Theme.Ui(9.2f), Theme.Dim, x, 248f);

            PaintFooter(g, "SYSTEM CLEAN", Theme.Lime);
        }

        private void PaintFailed(Graphics g)
        {
            DrawHead(g, "REMOVAL HALTED", "The uninstaller could not finish the sequence.");

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

            Theme.Draw(g, "UNINSTALL ABORTED", Theme.UiBold(10.5f), Theme.Warn, CX + 72f, 194f);
            Theme.Wrapped(g, _error ?? "Unknown error.", Theme.Mono(8.4f), Theme.Muted,
                new RectangleF(CX + 72f, 216f, ClientSize.Width - FooterPad - (CX + 72f), 90f));

            PaintFooter(g, "SEQUENCE HALTED", Theme.Danger);
        }

#if DEBUG
        internal void PrepareForRender(string phase)
        {
            if (string.Equals(phase, "confirm", StringComparison.OrdinalIgnoreCase))
            {
                _phase = Phase.Confirm;
                _block = null;
            }
            else if (string.Equals(phase, "done", StringComparison.OrdinalIgnoreCase))
            {
                _phase = Phase.Done;
                _ring.Value = 1;
                _ring.Complete = true;
                _ring.Spin = false;
            }
            else if (string.Equals(phase, "removing", StringComparison.OrdinalIgnoreCase))
            {
                _phase = Phase.Removing;
                _progress = 0.58;
                _bar.Value = _progress;
                _stage = "REGISTRY";
                _elapsed = TimeSpan.FromSeconds(1.2);
                _log.Append("reading installation record");
                _log.Append("target \u00B7 " + _dir);
                _log.Append("shell shortcuts removed");
                _log.Append("registry entries removed");
            }
            UpdateControls();
            OnTick();
            OnTick();
        }
#endif
    }
}
