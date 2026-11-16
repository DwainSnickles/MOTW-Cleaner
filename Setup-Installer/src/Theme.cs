using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;

namespace MotwSetup
{
    internal static class Theme
    {
        public static readonly Color Bg = Color.FromArgb(6, 10, 14);
        public static readonly Color Panel = Color.FromArgb(10, 17, 23);
        public static readonly Color PanelHi = Color.FromArgb(14, 24, 33);
        public static readonly Color Line = Color.FromArgb(24, 40, 52);
        public static readonly Color LineSoft = Color.FromArgb(17, 28, 37);
        public static readonly Color Accent = Color.FromArgb(43, 224, 255);
        public static readonly Color Lime = Color.FromArgb(155, 232, 75);
        public static readonly Color Warn = Color.FromArgb(255, 180, 84);
        public static readonly Color Danger = Color.FromArgb(255, 92, 108);
        public static readonly Color Text = Color.FromArgb(232, 244, 250);
        public static readonly Color Muted = Color.FromArgb(120, 146, 163);
        public static readonly Color Dim = Color.FromArgb(72, 90, 106);

        private static readonly Dictionary<string, Font> FontCache = new Dictionary<string, Font>();
        private static string _display;
        private static string _mono;

        private static readonly StringFormat NoWrap = new StringFormat(StringFormat.GenericTypographic)
        {
            FormatFlags = StringFormatFlags.NoWrap,
            Trimming = StringTrimming.None
        };

        private static readonly StringFormat Ellipsis = new StringFormat(StringFormat.GenericTypographic)
        {
            FormatFlags = StringFormatFlags.NoWrap,
            Trimming = StringTrimming.EllipsisCharacter
        };

        public static string DisplayFamily
        {
            get
            {
                if (_display == null) _display = Pick("Bahnschrift", "Segoe UI Semibold", "Segoe UI");
                return _display;
            }
        }

        public static string MonoFamily
        {
            get
            {
                if (_mono == null) _mono = Pick("Consolas", "Lucida Console", "Courier New");
                return _mono;
            }
        }

        private static string Pick(params string[] families)
        {
            foreach (var family in families)
            {
                try
                {
                    using (var probe = new FontFamily(family))
                    {
                        if (string.Equals(probe.Name, family, StringComparison.OrdinalIgnoreCase)) return family;
                    }
                }
                catch
                {
                    // Font family is not installed, try the next candidate.
                }
            }
            return "Segoe UI";
        }

        public static Font Head(float size)
        {
            return Get(DisplayFamily, size, FontStyle.Regular);
        }

        public static Font Ui(float size)
        {
            return Get("Segoe UI", size, FontStyle.Regular);
        }

        public static Font UiBold(float size)
        {
            return Get("Segoe UI", size, FontStyle.Bold);
        }

        public static Font Mono(float size)
        {
            return Get(MonoFamily, size, FontStyle.Regular);
        }

        private static Font Get(string family, float size, FontStyle style)
        {
            string key = family + "|" + size.ToString("0.##") + "|" + (int)style;
            Font font;
            if (!FontCache.TryGetValue(key, out font))
            {
                font = new Font(family, size, style, GraphicsUnit.Point);
                FontCache[key] = font;
            }
            return font;
        }

        public static Color Alpha(Color color, int alpha)
        {
            return Color.FromArgb(Math.Max(0, Math.Min(255, alpha)), color.R, color.G, color.B);
        }

        public static void SetQuality(Graphics g)
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;
        }

        public static GraphicsPath Round(RectangleF rect, float radius)
        {
            var path = new GraphicsPath();
            float d = radius * 2f;
            if (d <= 0f || rect.Width <= 0f || rect.Height <= 0f)
            {
                path.AddRectangle(rect);
                return path;
            }
            path.AddArc(rect.X, rect.Y, d, d, 180f, 90f);
            path.AddArc(rect.Right - d, rect.Y, d, d, 270f, 90f);
            path.AddArc(rect.Right - d, rect.Bottom - d, d, d, 0f, 90f);
            path.AddArc(rect.X, rect.Bottom - d, d, d, 90f, 90f);
            path.CloseFigure();
            return path;
        }

        public static GraphicsPath Cut(RectangleF rect, float cut)
        {
            var path = new GraphicsPath();
            path.AddPolygon(new[]
            {
                new PointF(rect.Left + cut, rect.Top),
                new PointF(rect.Right, rect.Top),
                new PointF(rect.Right, rect.Bottom - cut),
                new PointF(rect.Right - cut, rect.Bottom),
                new PointF(rect.Left, rect.Bottom),
                new PointF(rect.Left, rect.Top + cut)
            });
            path.CloseFigure();
            return path;
        }

        public static void Draw(Graphics g, string text, Font font, Color color, float x, float y)
        {
            using (var brush = new SolidBrush(color))
                g.DrawString(text, font, brush, x, y, NoWrap);
        }

        public static void DrawClipped(Graphics g, string text, Font font, Color color, RectangleF rect)
        {
            using (var brush = new SolidBrush(color))
                g.DrawString(text, font, brush, rect, Ellipsis);
        }

        public static void Wrapped(Graphics g, string text, Font font, Color color, RectangleF rect)
        {
            using (var brush = new SolidBrush(color))
                g.DrawString(text, font, brush, rect);
        }

        public static void Tracked(Graphics g, string text, Font font, Color color, float x, float y, float tracking)
        {
            using (var brush = new SolidBrush(color))
            {
                float cx = x;
                for (int i = 0; i < text.Length; i++)
                {
                    string glyph = text[i].ToString();
                    g.DrawString(glyph, font, brush, cx, y, NoWrap);
                    cx += g.MeasureString(glyph, font, PointF.Empty, NoWrap).Width + tracking;
                }
            }
        }

        public static float TrackedWidth(Graphics g, string text, Font font, float tracking)
        {
            if (string.IsNullOrEmpty(text)) return 0f;
            float width = 0f;
            for (int i = 0; i < text.Length; i++)
                width += g.MeasureString(text[i].ToString(), font, PointF.Empty, NoWrap).Width + tracking;
            return Math.Max(0f, width - tracking);
        }

        public static void TrackedCentered(Graphics g, string text, Font font, Color color, float x, float y, float width, float tracking)
        {
            float w = TrackedWidth(g, text, font, tracking);
            Tracked(g, text, font, color, x + (width - w) / 2f, y, tracking);
        }

        public static void Stroke(Graphics g, float x1, float y1, float x2, float y2, Color color, float width)
        {
            using (var pen = new Pen(color, width))
                g.DrawLine(pen, x1, y1, x2, y2);
        }

        public static void CornerBrackets(Graphics g, RectangleF rect, float len, Color color, float width)
        {
            using (var pen = new Pen(color, width))
            {
                g.DrawLine(pen, rect.Left, rect.Top, rect.Left + len, rect.Top);
                g.DrawLine(pen, rect.Left, rect.Top, rect.Left, rect.Top + len);
                g.DrawLine(pen, rect.Right, rect.Top, rect.Right - len, rect.Top);
                g.DrawLine(pen, rect.Right, rect.Top, rect.Right, rect.Top + len);
                g.DrawLine(pen, rect.Left, rect.Bottom, rect.Left + len, rect.Bottom);
                g.DrawLine(pen, rect.Left, rect.Bottom, rect.Left, rect.Bottom - len);
                g.DrawLine(pen, rect.Right, rect.Bottom, rect.Right - len, rect.Bottom);
                g.DrawLine(pen, rect.Right, rect.Bottom, rect.Right, rect.Bottom - len);
            }
        }
    }
}
