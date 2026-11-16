using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Windows.Forms;

namespace MOTW_Cleaner
{
    /// <summary>
    /// Portable-only additions: persists options and recent folders to a JSON
    /// file next to the exe instead of the per-user profile.
    /// </summary>
    public partial class frmMOTW
    {
        private const string ConfigFileName = "settings.json";
        private const string LegacyConfigFileName = "MOTW-Cleaner-Portable.json";

        /// <summary>
        /// Settings live under %APPDATA%\MOTW Cleaner so nothing is written next to
        /// the exe (which, when the exe sits on the Desktop, meant a file appearing
        /// on the Desktop).
        /// </summary>
        private static string ConfigPath
        {
            get
            {
                string dir = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    "MOTW Cleaner");

                Directory.CreateDirectory(dir);
                return Path.Combine(dir, ConfigFileName);
            }
        }

        private static string LegacyConfigPath => Path.Combine(AppContext.BaseDirectory, LegacyConfigFileName);

        private class PortableConfig
        {
            public bool AutoOpen { get; set; } = true;
            public bool Recurse { get; set; } = true;
            public string LastFolder { get; set; }
            public List<string> RecentFolders { get; set; } = new List<string>();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            LoadPortableConfig();
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            SavePortableConfig();
            base.OnFormClosing(e);
        }

        private void LoadPortableConfig()
        {
            // Read the current location, falling back to the old file that used to
            // sit next to the exe so existing settings are not lost.
            string configPath = ConfigPath;
            bool fromLegacyPath = false;

            if (!File.Exists(configPath) && File.Exists(LegacyConfigPath))
            {
                configPath = LegacyConfigPath;
                fromLegacyPath = true;
            }

            PortableConfig cfg;
            try
            {
                if (!File.Exists(configPath))
                    return;

                cfg = JsonSerializer.Deserialize<PortableConfig>(File.ReadAllText(configPath));
                if (cfg == null)
                    return;
            }
            catch
            {
                return;
            }

            ckbAutoOpen.Checked = cfg.AutoOpen;
            ckbRecurse.Checked = cfg.Recurse;

            foreach (var path in (cfg.RecentFolders ?? new List<string>())
                         .Where(p => !string.IsNullOrWhiteSpace(p))
                         .Take(10))
            {
                var item = new RecentFolderItem
                {
                    Text = Path.GetFileName(path.TrimEnd(Path.DirectorySeparatorChar)),
                    FullPath = path
                };
                item.Click += RecentItem_Click;
                ribbon1.OrbDropDown.RecentItems.Add(item);
            }

            // Set last so TextChanged moves it to the top of the recent list.
            if (!string.IsNullOrWhiteSpace(cfg.LastFolder))
                txtFolder.Text = cfg.LastFolder;

            // Migrated from the old beside-the-exe file: remove it so it stops
            // appearing next to the app (or on the Desktop).
            if (fromLegacyPath)
            {
                try
                {
                    File.Delete(LegacyConfigPath);
                    lstLog.Items.Add($"Settings moved to {ConfigPath}");
                }
                catch
                {
                    // Leave it alone if it cannot be removed.
                }
            }
        }

        private void SavePortableConfig()
        {
            try
            {
                var cfg = new PortableConfig
                {
                    AutoOpen = ckbAutoOpen.Checked,
                    Recurse = ckbRecurse.Checked,
                    LastFolder = Directory.Exists(txtFolder.Text) ? txtFolder.Text : null,
                    RecentFolders = ribbon1.OrbDropDown.RecentItems
                        .OfType<RecentFolderItem>()
                        .Select(i => i.FullPath)
                        .Where(p => !string.IsNullOrWhiteSpace(p) && Directory.Exists(p))
                        .Take(10)
                        .ToList()
                };

                File.WriteAllText(ConfigPath, JsonSerializer.Serialize(cfg, new JsonSerializerOptions { WriteIndented = true }));
            }
            catch
            {
                // Never let config saving break the app.
            }
        }
    }
}
