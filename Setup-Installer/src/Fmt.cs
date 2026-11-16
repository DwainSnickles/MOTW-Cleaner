using System;

namespace MotwSetup
{
    internal static class Fmt
    {
        public static string Bytes(long bytes)
        {
            const double kb = 1024.0;
            const double mb = kb * 1024.0;
            const double gb = mb * 1024.0;

            if (bytes >= gb) return (bytes / gb).ToString("0.00") + " GB";
            if (bytes >= mb) return (bytes / mb).ToString("0.0") + " MB";
            if (bytes >= kb) return (bytes / kb).ToString("0.0") + " KB";
            return bytes + " B";
        }

        public static string Clock(TimeSpan span)
        {
            if (span.TotalSeconds < 1) return "0.0s";
            if (span.TotalSeconds < 60) return span.TotalSeconds.ToString("0.0") + "s";
            return (int)span.TotalMinutes + "m " + span.Seconds.ToString("00") + "s";
        }

        public static string Percent(double value)
        {
            return ((int)Math.Round(Math.Max(0, Math.Min(1, value)) * 100.0)) + "%";
        }
    }
}
