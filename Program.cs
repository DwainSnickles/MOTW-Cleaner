using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MOTW_Cleaner
{
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Files, ZIPs and folders dropped onto the exe icon arrive as arguments.
            string initialPath = args == null
                ? null
                : args.FirstOrDefault(a => !string.IsNullOrWhiteSpace(a) &&
                                           !a.StartsWith("--") &&
                                           (File.Exists(a) || Directory.Exists(a)));

            Application.Run(new frmMOTW(initialPath));
        }
    }
}
