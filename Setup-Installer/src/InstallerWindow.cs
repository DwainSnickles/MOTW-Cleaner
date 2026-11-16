using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace MotwSetup
{
    internal abstract class InstallerWindow : Form
    {
        protected const int HeaderH = 76;
        private const int HoverNone = 0;
        private const int HoverMin = 1;
        private const int HoverClose = 2;

        private readonly Bitmap _background;
        private readonly Timer _clock;
        private readonly string _title;
        private readonly string _subtitle;
        private int _hoverZone = HoverNone;

        protected float Time;

        protected InstallerWindow(int width, int height, string title, string subtitle)
        {
            _title = title;
            _subtitle = subtitle;

            AutoScaleMode = AutoScaleMode.None;
            FormBorderStyle = FormBorderStyle.None;
            StartPosition = FormStartPosition.CenterScreen;
            ClientSize = new Size(width, height);
            BackColor = Theme.Bg;
            Font = Theme.Ui(9f);
            KeyPreview = true;
            DoubleBuffered = true;
            MaximizeBox = false;
            MinimizeBox = false;
            Text = BuildInfo.ProductName;

            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, true);

            _background = BuildBackground(width, height);
            _clock = new Timer { Interval = 33 };
            _clock.Tick += (sender, args) =>
            {
                Time += 0.033f;
                OnTick();
                Invalidate();
            };
            _clock.Start();
        }

        protected abstract void PaintContent(Graphics g);

        protected virtual void OnTick()
        {
        }

        protected virtual bool OnPrimaryKey()
        {
            return false;
        }

        protected virtual void OnCloseRequested()
        {
            Close();
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            Native.RoundCorners(Handle);
        }

        protected override CreateParams CreateParams
        {
            get
            {
                var parameters = base.CreateParams;
                parameters.ClassStyle |= 0x00020000; // CS_DROPSHADOW
                return parameters;
            }
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            if (_background != null) e.Graphics.DrawImageUnscaled(_background, 0, 0);
            else e.Graphics.Clear(Theme.Bg);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            Theme.SetQuality(g);
            PaintChrome(g);
            PaintContent(g);
        }

        private void PaintChrome(Graphics g)
        {
            Theme.Stroke(g, 0f, HeaderH - 1f, ClientSize.Width, HeaderH - 1f, Theme.Line, 1f);

            float sweepX = (Time * 230f) % (ClientSize.Width + 360f) - 180f;
            var sweep = new RectangleF(sweepX - 160f, HeaderH - 3f, 320f, 2f);
            using (var brush = new LinearGradientBrush(sweep, Theme.Accent, Theme.Accent, LinearGradientMode.Horizontal))
            {
                brush.InterpolationColors = new ColorBlend(3)
                {
                    Colors = new[]
                    {
                        Color.FromArgb(0, Theme.Accent),
                        Color.FromArgb(150, Theme.Accent),
                        Color.FromArgb(0, Theme.Accent)
                    },
                    Positions = new[] { 0f, 0.5f, 1f }
                };
                g.FillRectangle(brush, sweep);
            }

            var mark = new RectangleF(30f, 19f, 38f, 38f);
            using (var path = Theme.Round(mark, 9f))
            {
                using (var fill = new LinearGradientBrush(mark, Theme.PanelHi, Theme.Panel, 90f))
                    g.FillPath(fill, path);
                using (var pen = new Pen(Theme.Alpha(Theme.Accent, 95), 1f))
                    g.DrawPath(pen, path);
            }

            Theme.CornerBrackets(g, RectangleF.Inflate(mark, -9f, -9f), 4.5f, Theme.Alpha(Theme.Accent, 190), 1.2f);
            using (var pen = new Pen(Theme.Lime, 2.2f) { StartCap = LineCap.Round, EndCap = LineCap.Round, LineJoin = LineJoin.Round })
            {
                g.DrawLines(pen, new[]
                {
                    new PointF(mark.X + 12f, mark.Y + 21f),
                    new PointF(mark.X + 17f, mark.Y + 26f),
                    new PointF(mark.X + 27f, mark.Y + 14f)
                });
            }

            Theme.Tracked(g, _title, Theme.Head(13.5f), Theme.Text, 82f, 21f, 2.6f);
            Theme.Tracked(g, _subtitle, Theme.Mono(7.1f), Theme.Alpha(Theme.Dim, 225), 83f, 46f, 1.5f);

            bool elevated = Native.IsElevated;
            string chip = elevated ? "ELEVATED" : "STANDARD";
            Color chipColor = elevated ? Theme.Lime : Theme.Warn;
            float chipWidth = Theme.TrackedWidth(g, chip, Theme.Mono(6.9f), 1.4f);
            float chipX = ClientSize.Width - 126f - chipWidth;
            using (var dot = new SolidBrush(Theme.Alpha(chipColor, 210)))
                g.FillEllipse(dot, chipX - 12f, 35f, 5f, 5f);
            Theme.Tracked(g, chip, Theme.Mono(6.9f), chipColor, chipX, 31f, 1.4f);

            var minimize = new Rectangle(ClientSize.Width - 92, 0, 46, HeaderH);
            var close = new Rectangle(ClientSize.Width - 46, 0, 46, HeaderH);

            if (_hoverZone == HoverMin)
                using (var fill = new SolidBrush(Theme.Alpha(Theme.Accent, 20)))
                    g.FillRectangle(fill, minimize);
            if (_hoverZone == HoverClose)
                using (var fill = new SolidBrush(Theme.Alpha(Theme.Danger, 42)))
                    g.FillRectangle(fill, close);

            Theme.Stroke(g, minimize.X + 17f, 38f, minimize.X + 29f, 38f,
                _hoverZone == HoverMin ? Theme.Text : Theme.Muted, 1.4f);

            float cx = close.X + 18f;
            float cy = 33f;
            Color glyph = _hoverZone == HoverClose ? Theme.Danger : Theme.Muted;
            Theme.Stroke(g, cx, cy, cx + 9f, cy + 9f, glyph, 1.4f);
            Theme.Stroke(g, cx + 9f, cy, cx, cy + 9f, glyph, 1.4f);
        }

        protected void PaintFooter(Graphics g, string status, Color statusColor)
        {
            int height = ClientSize.Height;
            Theme.Stroke(g, 0f, height - 96f, ClientSize.Width, height - 96f, Theme.LineSoft, 1f);

            using (var dot = new SolidBrush(Theme.Alpha(statusColor, 220)))
                g.FillEllipse(dot, 30f, height - 52f, 5f, 5f);
            Theme.Tracked(g, status, Theme.Mono(7.1f), statusColor, 44f, height - 55f, 1.5f);
        }

        protected static void DrawSectionLabel(Graphics g, string text, float x, float y)
        {
            Theme.Tracked(g, text, Theme.Mono(7.1f), Theme.Dim, x, y, 1.6f);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            int zone = HoverNone;
            if (e.Y < HeaderH)
            {
                if (e.X >= ClientSize.Width - 46) zone = HoverClose;
                else if (e.X >= ClientSize.Width - 92) zone = HoverMin;
            }

            if (zone != _hoverZone)
            {
                _hoverZone = zone;
                Cursor = zone == HoverNone ? Cursors.Default : Cursors.Hand;
                Invalidate(new Rectangle(0, 0, ClientSize.Width, HeaderH));
            }
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            if (_hoverZone != HoverNone)
            {
                _hoverZone = HoverNone;
                Cursor = Cursors.Default;
                Invalidate(new Rectangle(0, 0, ClientSize.Width, HeaderH));
            }
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            if (e.Button != MouseButtons.Left || e.Y >= HeaderH) return;

            if (e.X >= ClientSize.Width - 46)
            {
                OnCloseRequested();
                return;
            }

            if (e.X >= ClientSize.Width - 92)
            {
                WindowState = FormWindowState.Minimized;
                return;
            }

            Native.DragWindow(Handle);
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.Escape)
            {
                OnCloseRequested();
                return true;
            }

            if (keyData == Keys.Enter && OnPrimaryKey()) return true;

            return base.ProcessCmdKey(ref msg, keyData);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_clock != null) _clock.Dispose();
                if (_background != null) _background.Dispose();
            }
            base.Dispose(disposing);
        }

        private static Bitmap BuildBackground(int width, int height)
        {
            var bitmap = new Bitmap(width, height);
            using (var g = Graphics.FromImage(bitmap))
            {
                Theme.SetQuality(g);

                using (var gradient = new LinearGradientBrush(
                    new Rectangle(0, -140, width, height + 140),
                    Color.FromArgb(10, 16, 22), Color.FromArgb(4, 7, 10), 90f))
                {
                    g.FillRectangle(gradient, 0, 0, width, height);
                }

                using (var pen = new Pen(Color.FromArgb(8, 19, 27)))
                {
                    for (int x = 0; x <= width; x += 40) g.DrawLine(pen, x, 0, x, height);
                    for (int y = 0; y <= height; y += 40) g.DrawLine(pen, 0, y, width, y);
                }

                using (var bloom = new LinearGradientBrush(
                    new Rectangle(0, 0, width, height),
                    Color.FromArgb(24, Theme.Accent), Color.FromArgb(0, Theme.Accent), 30f))
                {
                    g.FillRectangle(bloom, 0, 0, width, height);
                }

                using (var bloom = new LinearGradientBrush(
                    new Rectangle(0, 0, width, height),
                    Color.FromArgb(0, Theme.Lime), Color.FromArgb(10, Theme.Lime), 330f))
                {
                    g.FillRectangle(bloom, 0, 0, width, height);
                }

                using (var vignette = new LinearGradientBrush(
                    new Rectangle(0, height - 220, width, 220),
                    Color.FromArgb(0, 0, 0, 0), Color.FromArgb(115, 0, 0, 0), 90f))
                {
                    g.FillRectangle(vignette, 0, height - 220, width, 220);
                }
            }
            return bitmap;
        }
    }
}
