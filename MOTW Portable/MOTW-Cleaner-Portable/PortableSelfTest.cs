using System;
using System.ComponentModel;
using System.Reflection;

namespace MOTW_Cleaner
{
    /// <summary>
    /// Headless smoke test: verifies embedded resources, the Ribbon control,
    /// and the form's InitializeComponent all work on .NET 8.
    /// Run: MOTW-Cleaner-Portable.exe --selftest
    /// </summary>
    internal static class PortableSelfTest
    {
        public static int Run()
        {
            int failures = 0;

            try
            {
                Console.WriteLine("== Manifest resources ==");
                foreach (var name in Assembly.GetExecutingAssembly().GetManifestResourceNames())
                    Console.WriteLine("  " + name);

                Console.WriteLine("== Properties.Resources ==");
                var img = Properties.Resources.Folder48;
                Console.WriteLine($"  Folder48: {(img == null ? "NULL" : img.Width + "x" + img.Height)}");
                if (img == null) failures++;

                Console.WriteLine("== frmMOTW resources (ComponentResourceManager) ==");
                var crm = new ComponentResourceManager(typeof(frmMOTW));
                var stream = crm.GetObject("imageList1.ImageStream");
                Console.WriteLine($"  imageList1.ImageStream: {(stream == null ? "NULL" : stream.GetType().Name)}");
                if (stream == null) failures++;

                var small = crm.GetObject("btnBrowse.SmallImage");
                Console.WriteLine($"  btnBrowse.SmallImage: {(small == null ? "NULL" : small.GetType().Name)}");
                if (small == null) failures++;

                Console.WriteLine("== Form construction (Ribbon35 + InitializeComponent) ==");
                using (var form = new frmMOTW())
                {
                    Console.WriteLine($"  Form created: {form.Text}");
                }
            }
            catch (Exception ex)
            {
                failures++;
                Console.WriteLine("SELFTEST FAILED: " + ex);
            }

            Console.WriteLine(failures == 0 ? "SELFTEST PASSED" : $"SELFTEST FAILED ({failures} problem(s))");
            return failures;
        }
    }
}
