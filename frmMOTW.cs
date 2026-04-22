using ProgressBarSample;
using Remove_Web_Properties;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Net.Mime.MediaTypeNames;

namespace MOTW_Cleaner
{
    public partial class frmMOTW : Form
    {
        public frmMOTW()
        {
            InitializeComponent();
            picDragDrop.AllowDrop = true;
            progressExtract.Visible = false;
            MotwUnblocker.Log = msg => lstLog.Items.Add(msg);
            // Hook mouse enter for the entire form and all children
            //HookMouseEnterRecursive(this);
            //this.MouseEnter += (s, e) => this.BringToFront();
        }

        //protected override void WndProc(ref Message m)
        //{
        //    const int WM_MOUSEMOVE = 0x0200;

        //    if (m.Msg == WM_MOUSEMOVE)
        //    {
        //        this.BringToFront();
        //        this.Activate();
        //    }

        //    base.WndProc(ref m);
        //}

        private async void txtFolder_TextChanged(object sender, EventArgs e)
        {
            string folder = txtFolder.Text;

            // Give drag-drop time to finish writing the path
            await Task.Delay(50);

            bool valid = Directory.Exists(folder);
            btnClean.Enabled = valid;
            btnOpen.Enabled = valid;

            if (!valid)
                return;

            // Clean up missing folders (auto-remove)
            var missing = ribbon1.OrbDropDown.RecentItems
                .OfType<RecentFolderItem>()
                .Where(i => !Directory.Exists(i.FullPath))
                .ToList();

            foreach (var dead in missing)
                ribbon1.OrbDropDown.RecentItems.Remove(dead);

            string displayName = Path.GetFileName(folder.TrimEnd(Path.DirectorySeparatorChar));

            // Check if already exists
            var existing = ribbon1.OrbDropDown.RecentItems
                .OfType<RecentFolderItem>()
                .FirstOrDefault(i => string.Equals(i.FullPath, folder, StringComparison.OrdinalIgnoreCase));

            if (existing != null)
            {
                ribbon1.OrbDropDown.RecentItems.Remove(existing);
                ribbon1.OrbDropDown.RecentItems.Insert(0, existing);
                return;
            }

            var item = new RecentFolderItem
            {
                Text = displayName,
                FullPath = folder
            };

            item.Click += RecentItem_Click;

            ribbon1.OrbDropDown.RecentItems.Insert(0, item);

            while (ribbon1.OrbDropDown.RecentItems.Count > 10)
            {
                ribbon1.OrbDropDown.RecentItems.RemoveAt(ribbon1.OrbDropDown.RecentItems.Count - 1);
            }
        }

        private void RecentItem_Click(object sender, EventArgs e)
        {
            if (sender is RecentFolderItem recent)
            {
                txtFolder.Text = recent.FullPath;

                if (Directory.Exists(recent.FullPath))
                    Process.Start("explorer.exe", recent.FullPath);
            }
        }

        #region All Button Click Events

        //private void btnExit_Click(object sender, EventArgs e)
        //{
        //    Application.Exit();
        //}

        private void btnBrowse_Click(object sender, EventArgs e)
        {
            using (var dlg = new FolderBrowserDialog())
            {
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    string folder = dlg.SelectedPath;

                    txtFolder.Text = folder;   // <-- MUST WORK

                    lstLog.Items.Add($"Folder selected: {folder}");
                    lstLog.Items.Add("Ready to clean...");

                    btnOpen.Enabled = Directory.Exists(folder);

                    if (ckbAutoOpen.Checked)
                        btnClean.PerformClick();
                }
            }
        }

        private async void btnClean_Click(object sender, EventArgs e)
        {
            string folder = txtFolder.Text;

            if (string.IsNullOrWhiteSpace(folder) || !Directory.Exists(folder))
            {
                MessageBox.Show("Please select a valid folder first.");
                return;
            }

            UnblockFiles(folder);

            // Flash the label for visual feedback
            await FlashLabel();
        }

        private void btnExtract_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Title = "Select a ZIP file to extract";
                ofd.Filter = "ZIP Files (*.zip)|*.zip";

                if (ofd.ShowDialog() != DialogResult.OK)
                    return;

                string zipPath = ofd.FileName;

                lstLog.Items.Add($"ZIP selected: {zipPath}");

                // Reuse your existing ZIP handler
                HandleZipDrop(zipPath);
            }
        }

        private void btnOpen_Click(object sender, EventArgs e)
        {
            string path = txtFolder.Text.Trim();

            // Check if textbox is empty
            if (string.IsNullOrWhiteSpace(path))
            {
                MessageBox.Show("Please enter a folder path.");
                return;
            }

            // Check if path is a valid format
            if (!System.IO.Path.IsPathRooted(path))
            {
                MessageBox.Show("The path format is not valid.");
                return;
            }

            // Check if folder exists
            if (!System.IO.Directory.Exists(path))
            {
                MessageBox.Show("The folder does not exist.");
                return;
            }

            // Open the folder
            System.Diagnostics.Process.Start("explorer.exe", path);
        }

        #endregion
        #region All Drag and Drop Methods

        private async void picDragDrop_DragDrop(object sender, DragEventArgs e)
        {
            picDragDrop.BackColor = SystemColors.Control;
            
            var dropped = (string[])e.Data.GetData(DataFormats.FileDrop);
            if (dropped == null || dropped.Length == 0)
                return;

            string path = dropped[0];

            // ZIP dropped
            if (File.Exists(path) &&
                Path.GetExtension(path).Equals(".zip", StringComparison.OrdinalIgnoreCase))
            {
                HandleZipDrop(path);
                return;
            }

            //ensure clean button is enabled
            btnClean.Enabled = true;

            // Folder dropped
            if (Directory.Exists(path))
            {
                txtFolder.Text = path;
                lstLog.Items.Add($"Folder selected from drag-drop: {path}");
                lstLog.Items.Add("Ready to clean...");

                if (ckbAutoOpen.Checked)
                    btnClean.PerformClick();

                return;
            }

            // File dropped → use its folder
            if (File.Exists(path))
            {
                string folder = Path.GetDirectoryName(path);
                txtFolder.Text = folder;

                lstLog.Items.Add($"Folder selected from drag-drop: {folder}");
                lstLog.Items.Add("Ready to clean...");

                if (ckbAutoOpen.Checked)
                    btnClean.PerformClick();
            }
        }

        private void picDragDrop_DragEnter(object sender, DragEventArgs e)
        {
            this.BringToFront();
            this.Activate();

            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effect = DragDropEffects.Copy;
                picDragDrop.BackColor = Color.FromArgb(230, 240, 255);
            }
        }

        private void picDragDrop_DragLeave(object sender, EventArgs e)
        {
            picDragDrop.BackColor = SystemColors.Control;
        }

        #endregion
        #region Misc Methods

        private void UpdateCleanButtonState()
        {
            string folder = txtFolder.Text;
            btnClean.Enabled = Directory.Exists(folder);
        }

        private IDisposable BusyCursor()
        {
            Cursor.Current = Cursors.WaitCursor;
            return new ActionOnDispose(() => Cursor.Current = Cursors.Default);
        }

        private class ActionOnDispose : IDisposable
        {
            private readonly Action _action;
            public ActionOnDispose(Action action) => _action = action;
            public void Dispose() => _action();
        }

        //https://github.com/karenpayneoregon/console-apps/tree/master/MarkOfTheWeb
        public static void UnblockFiles(string folderName)
        {
            if (!Directory.Exists(folderName))
                return;

            var start = new ProcessStartInfo
            {
                FileName = "powershell.exe",
                Arguments = $"Get-ChildItem -Path '{folderName}' -Recurse | Unblock-File",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            var process = Process.Start(start);
            if (process != null)
            {
                process.WaitForExit();
                process.Dispose();
            }
        }

        private async void HandleZipDrop(string zipPath)
        {
            using (BusyCursor())
            {
                lstLog.Items.Add($"ZIP detected: {zipPath}");

                string extractRoot = Path.Combine(
                    Path.GetDirectoryName(zipPath),
                    Path.GetFileNameWithoutExtension(zipPath) + "_EXTRACTED"
                );

                try
                {
                    if (Directory.Exists(extractRoot))
                        Directory.Delete(extractRoot, true);

                    lstLog.Items.Add("Extracting ZIP...");
                    await ExtractZipWithProgress(zipPath, extractRoot);

                    lstLog.Items.Add($"Extracted to: {extractRoot}");
                    txtFolder.Text = extractRoot;

                    btnOpen.Enabled = Directory.Exists(extractRoot);

                    lstLog.Items.Add("Cleaning extracted files...");
                    UnblockFiles(extractRoot);

                    lstLog.Items.Add("Done.");

                    if (ckbAutoOpen.Checked)
                        Process.Start("explorer.exe", extractRoot);
                }
                catch (Exception ex)
                {
                    lstLog.Items.Add($"ERROR: {ex.Message}");
                }
            }
        }

        private async Task ExtractZipWithProgress(string zipPath, string extractRoot)
        {
            using (ZipArchive archive = ZipFile.OpenRead(zipPath))
            {
                int total = archive.Entries.Count;
                int current = 0;

                progressExtract.Visible = true;
                progressExtract.BringToFront();
                progressExtract.Value = 0;
                progressExtract.Maximum = 100;
                progressExtract.VisualMode = ProgressBarDisplayMode.Percentage;

                foreach (var entry in archive.Entries)
                {
                    string fullPath = Path.Combine(extractRoot, entry.FullName);

                    if (entry.FullName.EndsWith("/"))
                    {
                        Directory.CreateDirectory(fullPath);
                    }
                    else
                    {
                        Directory.CreateDirectory(Path.GetDirectoryName(fullPath));
                        entry.ExtractToFile(fullPath, true);
                    }

                    current++;
                    int percent = (int)((current / (double)total) * 100);

                    progressExtract.Value = percent;
                    progressExtract.CustomText = $"Extracting... {percent}%";
                    progressExtract.VisualMode = ProgressBarDisplayMode.CustomText;

                    await Task.Delay(1); // keeps UI responsive
                }
            }

            progressExtract.Visible = false;
        }

        private void HookMouseEnterRecursive(Control parent)
        {
            parent.MouseEnter += (s, e) => this.BringToFront();

            foreach (Control child in parent.Controls)
                HookMouseEnterRecursive(child);
        }



        #endregion

        private void lstLog_DragEnter(object sender, DragEventArgs e)
        {
            this.BringToFront();
            picDragDrop_DragEnter(sender, e);
        }

        private async Task FlashLabel()
        {
            lblFlashLabel.Visible = true;

            Color flashColor = Color.Yellow;
            Color normalColor = Color.White;

            for (int i = 0; i < 10; i++)   // more flashes
            {
                lblFlashLabel.BackColor = flashColor;
                await Task.Delay(150);   // slower ON

                lblFlashLabel.BackColor = normalColor;
                await Task.Delay(150);   // slower OFF
            }

            lblFlashLabel.Visible = false;
        }



    }
}

