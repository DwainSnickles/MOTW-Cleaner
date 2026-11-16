using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace MotwSetup
{
    internal enum ButtonVariant
    {
        Primary,
        Ghost,
        Danger
    }

    internal sealed class NeonButton : Control
    {
        private bool _hover;
        private bool _down;
        private ButtonVariant _variant = ButtonVariant.Primary;

        public NeonButton()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            SetStyle(ControlStyles.SupportsTransparentBackColor, true);
            BackColor = Color.Transparent;
            Cursor = Cursors.Hand;
            Font = Theme.UiBold(8.6f);
            Size = new Size(170, 44);
            TabStop = false;
        }

        public ButtonVariant Variant
        {
            get { return _variant; }
            set { _variant = value; Invalidate(); }
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            _hover = true;
            Invalidate();
            base.OnMouseEnter(e);
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            _hover = false;
            _down = false;
            Invalidate();
            base.OnMouseLeave(e);
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                _down = true;
                Invalidate();
            }
            base.OnMouseDown(e);
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            _down = false;
            Invalidate();
            base.OnMouseUp(e);
        }

        protected override void OnEnabledChanged(EventArgs e)
        {
            Invalidate();
            base.OnEnabledChanged(e);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            Theme.SetQuality(g);

            Color accent = _variant == ButtonVariant.Danger ? Theme.Danger : Theme.Accent;
            var rect = new RectangleF(0.5f, 0.5f, Width - 1f, Height - 1f);

            using (var path = Theme.Cut(rect, 9f))
            {
                bool primary = _variant == ButtonVariant.Primary;
                int fill = !Enabled ? 5 : primary ? (_hover ? 62 : 32) : (_hover ? 24 : 8);
                if (Enabled && _down) fill += 20;

                using (var brush = new LinearGradientBrush(rect, Theme.Alpha(accent, fill), Theme.Alpha(accent, fill / 6), 90f))
                    g.FillPath(brush, path);

                if (Enabled && (_hover || primary))
                {
                    for (int i = 4; i >= 1; i--)
                    {
                        using (var glow = new Pen(Theme.Alpha(accent, 9), i * 2f))
                            g.DrawPath(glow, path);
                    }
                }

                int stroke = !Enabled ? 45 : primary || _hover ? 190 : 95;
                using (var pen = new Pen(Theme.Alpha(accent, stroke), 1f))
                    g.DrawPath(pen, path);
            }

            Color text;
            if (!Enabled) text = Theme.Dim;
            else if (_variant == ButtonVariant.Danger) text = Theme.Danger;
            else if (_variant == ButtonVariant.Ghost) text = _hover ? Theme.Text : Theme.Muted;
            else text = _hover ? Color.White : Theme.Accent;

            const float tracking = 1.4f;
            float width = Theme.TrackedWidth(g, Text, Font, tracking);
            float height = g.MeasureString(Text, Font).Height;
            Theme.Tracked(g, Text, Font, text, (Width - width) / 2f, (Height - height) / 2f + 1f, tracking);

            if (Enabled && _variant == ButtonVariant.Primary)
            {
                using (var pen = new Pen(Theme.Alpha(Theme.Accent, 130), 2f))
                    g.DrawLine(pen, 11f, Height - 1f, 11f + (Width - 22f) / 4f, Height - 1f);
            }
        }
    }

    internal sealed class NeonToggle : Control
    {
        private bool _checked = true;
        private float _anim = 1f;
        private bool _hover;
        private readonly Timer _timer;

        public event EventHandler CheckedChanged;

        public NeonToggle()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            SetStyle(ControlStyles.SupportsTransparentBackColor, true);
            BackColor = Color.Transparent;
            Cursor = Cursors.Hand;
            Height = 30;
            Width = 380;

            _timer = new Timer { Interval = 16 };
            _timer.Tick += (sender, args) =>
            {
                float target = _checked ? 1f : 0f;
                if (Math.Abs(_anim - target) < 0.06f)
                {
                    _anim = target;
                    _timer.Stop();
                }
                else
                {
                    _anim += (target - _anim) * 0.32f;
                }
                Invalidate();
            };
        }

        public bool Checked
        {
            get { return _checked; }
            set
            {
                if (_checked == value) return;
                _checked = value;
                _timer.Start();
                Invalidate();
                var handler = CheckedChanged;
                if (handler != null) handler(this, EventArgs.Empty);
            }
        }

        protected override void OnClick(EventArgs e)
        {
            Checked = !Checked;
            base.OnClick(e);
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            _hover = true;
            Invalidate();
            base.OnMouseEnter(e);
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            _hover = false;
            Invalidate();
            base.OnMouseLeave(e);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            Theme.SetQuality(g);

            const float trackWidth = 42f;
            const float trackHeight = 22f;
            float top = (Height - trackHeight) / 2f;

            var track = new RectangleF(1f, top, trackWidth, trackHeight);
            using (var path = Theme.Round(track, trackHeight / 2f))
            {
                Color tone = _checked ? Theme.Accent : Theme.Line;
                using (var fill = new SolidBrush(Theme.Alpha(tone, _checked ? 44 : 34)))
                    g.FillPath(fill, path);
                using (var pen = new Pen(Theme.Alpha(tone, _checked ? 200 : 150 + (_hover ? 45 : 0)), 1.2f))
                    g.DrawPath(pen, path);
            }

            float knobX = 12f + 20f * _anim;
            float knobY = top + trackHeight / 2f;
            if (_checked)
            {
                using (var glow = new SolidBrush(Theme.Alpha(Theme.Accent, 40)))
                    g.FillEllipse(glow, knobX - 11f, knobY - 11f, 22f, 22f);
            }
            using (var knob = new SolidBrush(_checked ? Theme.Accent : Theme.Dim))
                g.FillEllipse(knob, knobX - 7f, knobY - 7f, 14f, 14f);
            using (var shine = new SolidBrush(Theme.Alpha(Color.White, _checked ? 150 : 60)))
                g.FillEllipse(shine, knobX - 4f, knobY - 5f, 5f, 4f);

            Color label = !Enabled ? Theme.Dim : _checked ? Theme.Text : _hover ? Theme.Text : Theme.Muted;
            var size = g.MeasureString(Text, Theme.Ui(9.4f));
            Theme.Draw(g, Text, Theme.Ui(9.4f), label, 58f, (Height - size.Height) / 2f + 1f);
        }
    }

    internal sealed class NeonInput : Control
    {
        private readonly TextBox _textBox;
        private bool _focused;
        private string _placeholder;

        public NeonInput()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            SetStyle(ControlStyles.SupportsTransparentBackColor, true);
            BackColor = Color.Transparent;

            _textBox = new TextBox
            {
                BorderStyle = BorderStyle.None,
                BackColor = Theme.Panel,
                ForeColor = Theme.Text,
                Font = Theme.Ui(9.4f)
            };
            _textBox.GotFocus += (sender, args) => { _focused = true; Invalidate(); };
            _textBox.LostFocus += (sender, args) => { _focused = false; Invalidate(); };
            Controls.Add(_textBox);

            Height = 44;
        }

        public string Placeholder
        {
            get { return _placeholder; }
            set
            {
                _placeholder = value;
                if (_textBox.IsHandleCreated) Native.SetCueBanner(_textBox.Handle, value);
            }
        }

        public override string Text
        {
            get { return _textBox.Text; }
            set { _textBox.Text = value ?? string.Empty; }
        }

        public void FocusInput()
        {
            _textBox.Focus();
            _textBox.SelectAll();
        }

        protected override void OnCreateControl()
        {
            base.OnCreateControl();
            var handle = _textBox.Handle;
            if (_placeholder != null) Native.SetCueBanner(handle, _placeholder);
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            if (_textBox == null) return;
            int height = _textBox.PreferredHeight;
            _textBox.SetBounds(16, Math.Max(4, (Height - height) / 2 + 1), Math.Max(10, Width - 32), height);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            Theme.SetQuality(g);

            var rect = new RectangleF(0.5f, 0.5f, Width - 1f, Height - 1f);
            using (var path = Theme.Cut(rect, 9f))
            {
                using (var fill = new SolidBrush(Theme.Panel))
                    g.FillPath(fill, path);
                using (var pen = new Pen(_focused ? Theme.Alpha(Theme.Accent, 200) : Theme.Line, 1f))
                    g.DrawPath(pen, path);
            }

            if (_focused)
            {
                using (var pen = new Pen(Theme.Accent, 2f))
                    g.DrawLine(pen, 12f, Height - 1f, 34f, Height - 1f);
            }
        }
    }

    internal sealed class NeonProgress : Control
    {
        private double _value;
        private float _phase;

        public NeonProgress()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            SetStyle(ControlStyles.SupportsTransparentBackColor, true);
            BackColor = Color.Transparent;
        }

        public double Value
        {
            get { return _value; }
            set
            {
                _value = Math.Max(0.0, Math.Min(1.0, value));
                Invalidate();
            }
        }

        public void Advance(float delta)
        {
            _phase += delta;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            Theme.SetQuality(g);

            int segments = Math.Max(20, Width / 13);
            float gap = 3f;
            float width = (Width - gap * (segments - 1)) / segments;
            float height = Height;

            double lit = _value * segments;
            float shimmer = (_phase * 26f) % 1f;

            for (int i = 0; i < segments; i++)
            {
                float x = i * (width + gap);
                float partial = (float)Math.Max(0.0, Math.Min(1.0, lit - i));
                var rect = new RectangleF(x, 0, width, height);

                if (partial <= 0f)
                {
                    using (var fill = new SolidBrush(Theme.LineSoft))
                    using (var path = Theme.Round(rect, 2f))
                        g.FillPath(fill, path);
                    continue;
                }

                Color tone = Blend(Theme.Accent, Theme.Lime, (float)i / Math.Max(1, segments - 1));
                var bar = new RectangleF(x, 0, width * Math.Max(0.15f, partial), height);
                using (var fill = new SolidBrush(tone))
                using (var path = Theme.Round(bar, 2f))
                    g.FillPath(fill, path);

                if (i == (int)Math.Floor(lit) || i == segments - 1)
                {
                    using (var glow = new SolidBrush(Theme.Alpha(tone, 45)))
                        g.FillEllipse(glow, x - 4f, -4f, width + 8f, height + 8f);
                }

                float local = i / (float)segments;
                if (Math.Abs(local - shimmer) < 0.04f)
                {
                    using (var shine = new SolidBrush(Theme.Alpha(Color.White, 26)))
                    using (var path = Theme.Round(bar, 2f))
                        g.FillPath(shine, path);
                }
            }
        }

        private static Color Blend(Color from, Color to, float t)
        {
            t = Math.Max(0f, Math.Min(1f, t));
            return Color.FromArgb(
                (int)(from.R + (to.R - from.R) * t),
                (int)(from.G + (to.G - from.G) * t),
                (int)(from.B + (to.B - from.B) * t));
        }
    }

    internal sealed class ConsolePanel : Control
    {
        private sealed class Row
        {
            public string Time;
            public string Text;
            public Color Color;
        }

        private readonly List<Row> _rows = new List<Row>();

        public ConsolePanel()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            SetStyle(ControlStyles.SupportsTransparentBackColor, true);
            BackColor = Color.Transparent;
        }

        public void Append(string text, Color? color = null)
        {
            if (text == null) return;
            _rows.Add(new Row
            {
                Time = DateTime.Now.ToString("HH:mm:ss"),
                Text = text,
                Color = color.HasValue ? color.Value : Theme.Muted
            });
            if (_rows.Count > 300) _rows.RemoveAt(0);
            Invalidate();
        }

        public void Clear()
        {
            _rows.Clear();
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            Theme.SetQuality(g);

            var frame = new RectangleF(0.5f, 0.5f, Width - 1f, Height - 1f);
            using (var path = Theme.Cut(frame, 10f))
            {
                using (var fill = new SolidBrush(Theme.Alpha(Theme.Panel, 170)))
                    g.FillPath(fill, path);
                using (var pen = new Pen(Theme.LineSoft, 1f))
                    g.DrawPath(pen, path);
            }

            Theme.Tracked(g, "INSTALL LOG", Theme.Mono(7f), Theme.Dim, 16f, 14f, 1.6f);
            string counter = _rows.Count + " EVENTS";
            float counterWidth = Theme.TrackedWidth(g, counter, Theme.Mono(7f), 1.2f);
            Theme.Tracked(g, counter, Theme.Mono(7f), Theme.Alpha(Theme.Dim, 160), Width - 16f - counterWidth, 14f, 1.2f);
            Theme.Stroke(g, 16f, 32f, Width - 16f, 32f, Theme.LineSoft, 1f);

            const float lineHeight = 19f;
            int capacity = Math.Max(1, (int)((Height - 48f) / lineHeight));
            int start = Math.Max(0, _rows.Count - capacity);

            for (int i = start; i < _rows.Count; i++)
            {
                float y = 40f + (i - start) * lineHeight;
                var row = _rows[i];
                Theme.Draw(g, row.Time, Theme.Mono(7.2f), Theme.Alpha(Theme.Dim, 200), 16f, y);
                Theme.DrawClipped(g, row.Text, Theme.Mono(7.8f), row.Color, new RectangleF(84f, y - 1f, Width - 100f, lineHeight));
            }
        }
    }

    internal sealed class StepRail : Control
    {
        private float _pulse;

        public string[] Steps = new string[0];
        public int Active;
        public string Footer;

        public StepRail()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            SetStyle(ControlStyles.SupportsTransparentBackColor, true);
            BackColor = Color.Transparent;
        }

        public void Advance(float delta)
        {
            _pulse += delta;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            Theme.SetQuality(g);

            Theme.Stroke(g, Width - 0.5f, 0f, Width - 0.5f, Height, Theme.LineSoft, 1f);

            const float markerX = 34f;
            const float startY = 60f;
            const float gap = 78f;

            if (Steps.Length > 1)
            {
                Theme.Stroke(g, markerX, startY + 1f, markerX, startY + (Steps.Length - 1) * gap + 1f, Theme.LineSoft, 1f);
                if (Active > 0)
                {
                    float end = startY + Math.Min(Active, Steps.Length - 1) * gap + 1f;
                    Theme.Stroke(g, markerX, startY + 1f, markerX, end, Theme.Alpha(Theme.Accent, 130), 1f);
                }
            }

            for (int i = 0; i < Steps.Length; i++)
            {
                float cy = startY + i * gap;
                bool done = i < Active;
                bool active = i == Active;
                Color tone = done ? Theme.Lime : active ? Theme.Accent : Theme.Dim;

                float radius = 8f;
                var diamond = new[]
                {
                    new PointF(markerX, cy - radius),
                    new PointF(markerX + radius, cy),
                    new PointF(markerX, cy + radius),
                    new PointF(markerX - radius, cy)
                };

                if (active)
                {
                    float ripple = 12f + 3f * (float)Math.Sin(_pulse * 2.4f);
                    using (var ring = new Pen(Theme.Alpha(Theme.Accent, 40), 1f))
                        g.DrawEllipse(ring, markerX - ripple, cy - ripple, ripple * 2, ripple * 2);
                }

                if (done || active)
                {
                    using (var fill = new SolidBrush(Theme.Alpha(tone, done ? 90 : 70)))
                        g.FillPolygon(fill, diamond);
                }
                using (var pen = new Pen(Theme.Alpha(tone, done || active ? 230 : 150), 1.3f))
                    g.DrawPolygon(pen, diamond);

                string index = (i + 1).ToString("00");
                Theme.Tracked(g, index, Theme.Mono(7f), Theme.Alpha(tone, done || active ? 210 : 130), markerX + 24f, cy - 16f, 1.2f);
                Theme.Tracked(g, Steps[i], Theme.UiBold(8.8f), done ? Theme.Muted : active ? Theme.Text : Theme.Dim,
                    markerX + 24f, cy - 3f, 1.3f);
            }

            if (!string.IsNullOrEmpty(Footer))
            {
                Theme.Stroke(g, 28f, Height - 66f, Width - 32f, Height - 66f, Theme.LineSoft, 1f);
                Theme.Tracked(g, Footer, Theme.Mono(6.8f), Theme.Alpha(Theme.Dim, 200), 30f, Height - 50f, 1.1f);
            }
        }
    }

    internal sealed class ProgressRing : Control
    {
        private float _phase;
        private float _enter;

        public double Value;
        public string Caption = "PREFLIGHT";
        public bool Complete;
        public bool Spin = true;

        public ProgressRing()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            SetStyle(ControlStyles.SupportsTransparentBackColor, true);
            BackColor = Color.Transparent;
        }

        public void Advance(float delta)
        {
            _phase += delta;
            if (Complete && _enter < 1f) _enter = Math.Min(1f, _enter + delta * 2.4f);
        }

        public void Reset()
        {
            _enter = 0f;
            Complete = false;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            Theme.SetQuality(g);

            float cx = Width / 2f;
            float cy = Height / 2f;
            float radius = Math.Min(Width, Height) / 2f - 26f;

            for (int i = 0; i < 60; i++)
            {
                double angle = i * Math.PI / 30.0 - Math.PI / 2.0;
                float length = i % 5 == 0 ? 5f : 2.5f;
                float x1 = cx + (float)Math.Cos(angle) * (radius + 8f);
                float y1 = cy + (float)Math.Sin(angle) * (radius + 8f);
                float x2 = cx + (float)Math.Cos(angle) * (radius + 8f + length);
                float y2 = cy + (float)Math.Sin(angle) * (radius + 8f + length);
                Theme.Stroke(g, x1, y1, x2, y2, Theme.Alpha(Theme.Line, i % 5 == 0 ? 230 : 150), 1f);
            }

            using (var track = new Pen(Theme.Line, 2f))
                g.DrawEllipse(track, cx - radius, cy - radius, radius * 2f, radius * 2f);

            if (Spin && !Complete)
            {
                float start = (_phase * 130f) % 360f;
                using (var pen = new Pen(Theme.Alpha(Theme.Accent, 110), 2f))
                    g.DrawArc(pen, cx - radius, cy - radius, radius * 2f, radius * 2f, start, 68f);
            }

            float sweep = (float)(Math.Max(0.0, Math.Min(1.0, Value)) * 360.0);
            Color tone = Complete ? Theme.Lime : Theme.Accent;

            if (sweep > 0.4f)
            {
                for (int i = 3; i >= 1; i--)
                {
                    using (var glow = new Pen(Theme.Alpha(tone, 12), 2f + i * 3f))
                        g.DrawArc(glow, cx - radius, cy - radius, radius * 2f, radius * 2f, -90f, Math.Min(359.9f, sweep));
                }
                using (var pen = new Pen(tone, 2.6f) { StartCap = LineCap.Round, EndCap = LineCap.Round })
                    g.DrawArc(pen, cx - radius, cy - radius, radius * 2f, radius * 2f, -90f, Math.Min(359.9f, sweep));
            }

            if (Complete)
            {
                float alpha = Math.Max(0f, Math.Min(1f, (_enter - 0.15f) / 0.85f));
                if (alpha > 0f)
                {
                    using (var path = new GraphicsPath())
                    {
                        float s = radius * 0.52f;
                        path.AddLines(new[]
                        {
                            new PointF(cx - s, cy + s * 0.04f),
                            new PointF(cx - s * 0.28f, cy + s * 0.62f),
                            new PointF(cx + s * 0.92f, cy - s * 0.56f)
                        });
                        using (var glow = new Pen(Theme.Alpha(Theme.Lime, (int)(28 * alpha)), 9f))
                        using (var pen = new Pen(Theme.Alpha(Theme.Lime, (int)(255 * alpha)), 4f) { StartCap = LineCap.Round, EndCap = LineCap.Round, LineJoin = LineJoin.Round })
                        {
                            g.DrawPath(glow, path);
                            g.DrawPath(pen, path);
                        }
                    }
                }

                Theme.TrackedCentered(g, Caption, Theme.Mono(7f), Theme.Alpha(Theme.Lime, 210), 0, cy + radius * 0.66f, Width, 1.6f);
            }
            else
            {
                string percent = ((int)Math.Round(Value * 100.0)).ToString();
                var font = Theme.Head(23f);
                float width = g.MeasureString(percent, font).Width;
                Theme.Draw(g, percent, font, Theme.Text, cx - width / 2f, cy - 22f);

                string percentSign = "%";
                Theme.Draw(g, percentSign, Theme.Head(10f), Theme.Alpha(Theme.Accent, 190), cx + width / 2f + 3f, cy - 4f);

                string caption = Caption;
                float captionWidth = Theme.TrackedWidth(g, caption, Theme.Mono(6.8f), 1.5f);
                Theme.Tracked(g, caption, Theme.Mono(6.8f), Theme.Dim, cx - captionWidth / 2f, cy + 18f, 1.5f);
            }
        }
    }
}
