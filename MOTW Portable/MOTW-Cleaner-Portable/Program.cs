using System;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace MOTW_Cleaner
{
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the portable application.
        /// </summary>
        [STAThread]
        static int Main(string[] args)
        {
            if (args.Any(a => a.Equals("--selftest", StringComparison.OrdinalIgnoreCase)))
            {
                return PortableSelfTest.Run();
            }

            // Files, ZIPs and folders dropped onto the exe icon arrive as arguments.
            string initialPath = args.FirstOrDefault(a => !string.IsNullOrWhiteSpace(a) &&
                                                          !a.StartsWith("--") &&
                                                          (File.Exists(a) || Directory.Exists(a)));

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new frmMOTW(initialPath));
            return 0;
        }
    }
}
