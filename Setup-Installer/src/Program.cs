using System;
using System.Windows.Forms;

namespace MotwSetup
{
    internal static class Program
    {
        [STAThread]
        private static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

#if UNINSTALLER
            if (args != null && args.Length >= 2 && string.Equals(args[0], "--cleanup", StringComparison.OrdinalIgnoreCase))
            {
                InstallEngine.Cleanup(args[1]);
                return;
            }

            Application.Run(new UninstallForm());
#else
            Application.Run(new SetupForm());
#endif
        }
    }
}
