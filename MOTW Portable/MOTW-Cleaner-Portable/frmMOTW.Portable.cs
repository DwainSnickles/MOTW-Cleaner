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
        private const string ConfigFileName = "MOTW-Cleaner-Portable.json";
        private static string ConfigPath => Path.Combine(AppContext.BaseDirectory, ConfigFileName);

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
            PortableConfig cfg;
            try
            {
                if (!File.Exists(ConfigPath))
                    return;

                cfg = JsonSerializer.Deserialize<PortableConfig>(File.ReadAllText(ConfigPath));
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
