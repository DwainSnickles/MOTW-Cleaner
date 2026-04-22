using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace ProgressBarSample
{
    public enum ProgressBarDisplayMode
    {
        NoText,
        Percentage,
        CurrProgress,
        CustomText,
        TextAndPercentage,
        TextAndCurrProgress
    }

    public class TextProgressBar : ProgressBar
    {
        // -----------------------------
        // Public Properties
        // -----------------------------

        public Font TextFont { get; set; } =
            new Font(FontFamily.GenericSansSerif, 10, FontStyle.Bold);

        private SolidBrush _textColourBrush = (SolidBrush)Brushes.Black;
        public Color TextColor
        {
            get => _textColourBrush.Color;
            set
            {
                _textColourBrush.Dispose();
                _textColourBrush = new SolidBrush(value);
                Invalidate();
            }
        }

        private Color _progressColor = Color.FromArgb(126, 180, 234); // Office Blue core
        public Color ProgressColor
        {
            get => _progressColor;
            set { _progressColor = value; Invalidate(); }
        }

        private ProgressBarDisplayMode _visualMode = ProgressBarDisplayMode.CurrProgress;
        public ProgressBarDisplayMode VisualMode
        {
            get => _visualMode;
            set { _visualMode = value; Invalidate(); }
        }

        public string CustomText { get; set; } = string.Empty;

        private string PercentageStr =>
            $"{(int)((float)Value - Minimum) / ((float)Maximum - Minimum) * 100} %";

        private string CurrProgressStr => $"{Value}/{Maximum}";

        private string TextToDraw
        {
            get
            {
                switch (VisualMode)
                {
                    case ProgressBarDisplayMode.Percentage:
                        return PercentageStr;
                    case ProgressBarDisplayMode.CurrProgress:
                        return CurrProgressStr;
                    case ProgressBarDisplayMode.TextAndCurrProgress:
                        return $"{CustomText}: {CurrProgressStr}";
                    case ProgressBarDisplayMode.TextAndPercentage:
                        return $"{CustomText}: {PercentageStr}";
                    case ProgressBarDisplayMode.CustomText:
                        return CustomText;
                    default:
                        return string.Empty;
                }
            }
        }

        // -----------------------------
        // Constructor
        // -----------------------------

        public TextProgressBar()
        {
            SetStyle(ControlStyles.UserPaint |
                     ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.ResizeRedraw, true);

            Value = Minimum;
        }

        // -----------------------------
        // Rounded Rectangle Helper
        // -----------------------------

        private GraphicsPath GetRoundRect(Rectangle rect, int radius)
        {
            int d = radius * 3;
            GraphicsPath path = new GraphicsPath();

            path.AddArc(rect.X, rect.Y, d, d, 180, 90);
            path.AddArc(rect.Right - d, rect.Y, d, d, 270, 90);
            path.AddArc(rect.Right - d, rect.Bottom - d, d, d, 0, 90);
            path.AddArc(rect.X, rect.Bottom - d, d, d, 90, 90);

            path.CloseFigure();
            return path;
        }

        // -----------------------------
        // Paint Override
        // -----------------------------

        protected override void OnPaint(PaintEventArgs e)
        {
            if (DesignMode && (Width <= 0 || Height <= 0))
                return;

            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            Rectangle rect = ClientRectangle;
            rect.Inflate(-1, -1);

            using (GraphicsPath path = GetRoundRect(rect, 6))
            {
                g.SetClip(path);
                DrawGlossyProgress(g, rect);
                g.ResetClip();

                using (Pen borderPen = new Pen(Color.FromArgb(120, Color.Gray)))
                    g.DrawPath(borderPen, path);
            }

            DrawStringIfNeeded(g);
        }

        // -----------------------------
        // Office-Inspired Glossy Fill
        // -----------------------------

        private void DrawGlossyProgress(Graphics g, Rectangle rect)
        {
            if (Value <= 0)
                return;

            Rectangle clip = new Rectangle(rect.X, rect.Y,
                (int)Math.Round(((float)Value / Maximum) * rect.Width), rect.Height);

            // Office Blue gradient stops
            Color topGloss = Color.FromArgb(220, 235, 250);  // #DCEBFA
            Color midBlue = ProgressColor;                   // #7EB4EA
            Color deepBlue = Color.FromArgb(44, 109, 184);   // #2C6DB8

            // Base gradient (deep → mid)
            using (LinearGradientBrush baseBrush = new LinearGradientBrush(
                clip, deepBlue, midBlue, LinearGradientMode.Vertical))
            {
                g.FillRectangle(baseBrush, clip);
            }

            // Top gloss
            Rectangle glossRect = new Rectangle(clip.X, clip.Y, clip.Width, clip.Height / 2);
            using (LinearGradientBrush glossBrush = new LinearGradientBrush(
                glossRect, topGloss, Color.FromArgb(0, topGloss), LinearGradientMode.Vertical))
            {
                g.FillRectangle(glossBrush, glossRect);
            }

            // Reflection band
            Rectangle reflection = new Rectangle(
                clip.X, clip.Y + (clip.Height / 3), clip.Width, clip.Height / 3);

            using (LinearGradientBrush reflectBrush = new LinearGradientBrush(
                reflection, Color.FromArgb(90, Color.White),
                Color.FromArgb(0, Color.White), LinearGradientMode.Vertical))
            {
                g.FillRectangle(reflectBrush, reflection);
            }

            // Bottom bevel
            Rectangle bevel = new Rectangle(clip.X, clip.Bottom - 4, clip.Width, 4);
            using (LinearGradientBrush bevelBrush = new LinearGradientBrush(
                bevel, deepBlue, ControlPaint.Dark(deepBlue), LinearGradientMode.Vertical))
            {
                g.FillRectangle(bevelBrush, bevel);
            }
        }

        // -----------------------------
        // Text Drawing
        // -----------------------------

        private void DrawStringIfNeeded(Graphics g)
        {
            if (VisualMode == ProgressBarDisplayMode.NoText)
                return;

            if (Width <= 5 || Height <= 5)
                return;

            try
            {
                string text = TextToDraw;
                if (string.IsNullOrEmpty(text))
                    return;

                SizeF len = g.MeasureString(text, TextFont);

                Point location = new Point(
                    (Width - (int)len.Width) / 2,
                    (Height - (int)len.Height) / 2);

                g.DrawString(text, TextFont, _textColourBrush, location);
            }
            catch
            {
                // Designer safety
            }
        }

        // -----------------------------
        // Office Blue Preset
        // -----------------------------

        public void ApplyOfficeBluePreset()
        {
            this.ProgressColor = Color.FromArgb(126, 180, 234); // #7EB4EA
            this.TextColor = Color.FromArgb(30, 30, 30);
            Invalidate();
        }

        public new void Dispose()
        {
            _textColourBrush.Dispose();
            base.Dispose();
        }
    }
}
