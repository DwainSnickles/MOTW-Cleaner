using System;
using System.Runtime.InteropServices;
using System.Security.Principal;
using Microsoft.Win32;

namespace MotwSetup
{
    internal static class Native
    {
        private const int WM_NCLBUTTONDOWN = 0x00A1;
        private const int HTCAPTION = 2;
        private const int EM_SETCUEBANNER = 0x1501;
        private const int DWMWA_WINDOW_CORNER_PREFERENCE = 33;

        [DllImport("user32.dll")]
        private static extern bool ReleaseCapture();

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        private static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, string lParam);

        [DllImport("dwmapi.dll")]
        private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attribute, ref int value, int size);

        public static void DragWindow(IntPtr handle)
        {
            try
            {
                ReleaseCapture();
                SendMessage(handle, WM_NCLBUTTONDOWN, (IntPtr)HTCAPTION, IntPtr.Zero);
            }
            catch
            {
            }
        }

        public static void SetCueBanner(IntPtr handle, string text)
        {
            try
            {
                SendMessage(handle, EM_SETCUEBANNER, (IntPtr)1, text ?? string.Empty);
            }
            catch
            {
            }
        }

        public static void RoundCorners(IntPtr handle)
        {
            try
            {
                int preference = 2;
                DwmSetWindowAttribute(handle, DWMWA_WINDOW_CORNER_PREFERENCE, ref preference, sizeof(int));
            }
            catch
            {
            }
        }

        public static bool IsElevated
        {
            get
            {
                try
                {
                    using (var identity = WindowsIdentity.GetCurrent())
                        return new WindowsPrincipal(identity).IsInRole(WindowsBuiltInRole.Administrator);
                }
                catch
                {
                    return false;
                }
            }
        }

        public static string OsDescription()
        {
            string arch = Environment.Is64BitOperatingSystem ? "x64" : "x86";
            try
            {
                using (var key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion"))
                {
                    if (key != null)
                    {
                        string name = key.GetValue("ProductName") as string;
                        string build = key.GetValue("CurrentBuildNumber") as string;
                        string display = key.GetValue("DisplayVersion") as string;

                        if (string.IsNullOrEmpty(name)) name = "Windows";
                        if (string.IsNullOrEmpty(build)) build = Environment.OSVersion.Version.Build.ToString();

                        return name
                               + (string.IsNullOrEmpty(display) ? string.Empty : " " + display)
                               + " \u00B7 build " + build
                               + " \u00B7 " + arch;
                    }
                }
            }
            catch
            {
            }

            return Environment.OSVersion.VersionString + " \u00B7 " + arch;
        }
    }
}
