using System;
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

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new frmMOTW());
            return 0;
        }
    }
}
