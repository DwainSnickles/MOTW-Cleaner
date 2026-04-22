namespace MOTW_Cleaner
{
    partial class frmMOTW
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmMOTW));
            this.ribbon1 = new System.Windows.Forms.Ribbon();
            this.btnOrbOpenFolder = new System.Windows.Forms.RibbonOrbMenuItem();
            this.ribbonSeparator1 = new System.Windows.Forms.RibbonSeparator();
            this.btnOrbExtract = new System.Windows.Forms.RibbonOrbMenuItem();
            this.ribbonSeparator2 = new System.Windows.Forms.RibbonSeparator();
            this.btnOrbCloseMOTW = new System.Windows.Forms.RibbonOrbMenuItem();
            this.ribbonSeparator3 = new System.Windows.Forms.RibbonSeparator();
            this.ribbonTab1 = new System.Windows.Forms.RibbonTab();
            this.ribbonPanel1 = new System.Windows.Forms.RibbonPanel();
            this.btnBrowse = new System.Windows.Forms.RibbonButton();
            this.ribbonSeparator4 = new System.Windows.Forms.RibbonSeparator();
            this.btnExtract = new System.Windows.Forms.RibbonButton();
            this.ribbonSeparator5 = new System.Windows.Forms.RibbonSeparator();
            this.ribbonPanel2 = new System.Windows.Forms.RibbonPanel();
            this.ckbRecurse = new System.Windows.Forms.RibbonCheckBox();
            this.ckbAutoOpen = new System.Windows.Forms.RibbonCheckBox();
            this.ribbonPanel5 = new System.Windows.Forms.RibbonPanel();
            this.btnClean = new System.Windows.Forms.RibbonButton();
            this.ribbonPanel3 = new System.Windows.Forms.RibbonPanel();
            this.ribbonPanel4 = new System.Windows.Forms.RibbonPanel();
            this.btnClose = new System.Windows.Forms.RibbonButton();
            this.panel1 = new System.Windows.Forms.Panel();
            this.picBrowse = new System.Windows.Forms.PictureBox();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.lstLog = new System.Windows.Forms.ListBox();
            this.txtFolder = new System.Windows.Forms.TextBox();
            this.btnOpen = new System.Windows.Forms.Button();
            this.imageList1 = new System.Windows.Forms.ImageList(this.components);
            this.picDragDrop = new System.Windows.Forms.PictureBox();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.toolStripStatusLabel1 = new System.Windows.Forms.ToolStripStatusLabel();
            this.lblFlashLabel = new System.Windows.Forms.Label();
            this.progressExtract = new ProgressBarSample.TextProgressBar();
            this.panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picBrowse)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picDragDrop)).BeginInit();
            this.statusStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // ribbon1
            // 
            this.ribbon1.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.ribbon1.Location = new System.Drawing.Point(0, 0);
            this.ribbon1.Minimized = false;
            this.ribbon1.Name = "ribbon1";
            // 
            // 
            // 
            this.ribbon1.OrbDropDown.BorderRoundness = 8;
            this.ribbon1.OrbDropDown.Location = new System.Drawing.Point(0, 0);
            this.ribbon1.OrbDropDown.MenuItems.Add(this.btnOrbOpenFolder);
            this.ribbon1.OrbDropDown.MenuItems.Add(this.ribbonSeparator1);
            this.ribbon1.OrbDropDown.MenuItems.Add(this.btnOrbExtract);
            this.ribbon1.OrbDropDown.MenuItems.Add(this.ribbonSeparator2);
            this.ribbon1.OrbDropDown.MenuItems.Add(this.btnOrbCloseMOTW);
            this.ribbon1.OrbDropDown.MenuItems.Add(this.ribbonSeparator3);
            this.ribbon1.OrbDropDown.Name = "";
            this.ribbon1.OrbDropDown.RecentItemsCaption = "Recently Opened Zip Files and Foders";
            this.ribbon1.OrbDropDown.Size = new System.Drawing.Size(527, 213);
            this.ribbon1.OrbDropDown.TabIndex = 0;
            this.ribbon1.OrbImage = null;
            this.ribbon1.RibbonTabFont = new System.Drawing.Font("Trebuchet MS", 9F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Italic))), System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ribbon1.Size = new System.Drawing.Size(703, 154);
            this.ribbon1.TabIndex = 3;
            this.ribbon1.Tabs.Add(this.ribbonTab1);
            this.ribbon1.TabsMargin = new System.Windows.Forms.Padding(12, 26, 20, 0);
            this.ribbon1.ThemeColor = System.Windows.Forms.RibbonTheme.Blue;
            // 
            // btnOrbOpenFolder
            // 
            this.btnOrbOpenFolder.DropDownArrowDirection = System.Windows.Forms.RibbonArrowDirection.Left;
            this.btnOrbOpenFolder.Image = global::MOTW_Cleaner.Properties.Resources.Folder32;
            this.btnOrbOpenFolder.SmallImage = global::MOTW_Cleaner.Properties.Resources.Folder32;
            this.btnOrbOpenFolder.Text = "Open Folder";
            this.btnOrbOpenFolder.Click += new System.EventHandler(this.btnBrowse_Click);
            // 
            // btnOrbExtract
            // 
            this.btnOrbExtract.DropDownArrowDirection = System.Windows.Forms.RibbonArrowDirection.Left;
            this.btnOrbExtract.Image = global::MOTW_Cleaner.Properties.Resources.Zip32;
            this.btnOrbExtract.SmallImage = global::MOTW_Cleaner.Properties.Resources.Zip32;
            this.btnOrbExtract.Text = "Open Zip File";
            this.btnOrbExtract.Click += new System.EventHandler(this.btnExtract_Click);
            // 
            // btnOrbCloseMOTW
            // 
            this.btnOrbCloseMOTW.DropDownArrowDirection = System.Windows.Forms.RibbonArrowDirection.Left;
            this.btnOrbCloseMOTW.Image = global::MOTW_Cleaner.Properties.Resources.Exit32;
            this.btnOrbCloseMOTW.SmallImage = global::MOTW_Cleaner.Properties.Resources.Exit32;
            this.btnOrbCloseMOTW.Text = "Close MOTW";
            // 
            // ribbonTab1
            // 
            this.ribbonTab1.Panels.Add(this.ribbonPanel1);
            this.ribbonTab1.Panels.Add(this.ribbonPanel2);
            this.ribbonTab1.Panels.Add(this.ribbonPanel5);
            this.ribbonTab1.Panels.Add(this.ribbonPanel3);
            this.ribbonTab1.Panels.Add(this.ribbonPanel4);
            this.ribbonTab1.Text = "MOTW Zip file and Folder Unblocker";
            // 
            // ribbonPanel1
            // 
            this.ribbonPanel1.ButtonMoreEnabled = false;
            this.ribbonPanel1.ButtonMoreVisible = false;
            this.ribbonPanel1.Items.Add(this.btnBrowse);
            this.ribbonPanel1.Items.Add(this.ribbonSeparator4);
            this.ribbonPanel1.Items.Add(this.btnExtract);
            this.ribbonPanel1.Items.Add(this.ribbonSeparator5);
            this.ribbonPanel1.Text = "Select a Zip file or Folder ";
            // 
            // btnBrowse
            // 
            this.btnBrowse.Image = global::MOTW_Cleaner.Properties.Resources.Folder48;
            this.btnBrowse.MinimumSize = new System.Drawing.Size(100, 0);
            this.btnBrowse.SmallImage = ((System.Drawing.Image)(resources.GetObject("btnBrowse.SmallImage")));
            this.btnBrowse.Text = "Select Folder";
            this.btnBrowse.Click += new System.EventHandler(this.btnBrowse_Click);
            // 
            // btnExtract
            // 
            this.btnExtract.Image = global::MOTW_Cleaner.Properties.Resources.ZipFolder48;
            this.btnExtract.MinimumSize = new System.Drawing.Size(100, 0);
            this.btnExtract.SmallImage = ((System.Drawing.Image)(resources.GetObject("btnExtract.SmallImage")));
            this.btnExtract.Text = "Select Zip File";
            this.btnExtract.Click += new System.EventHandler(this.btnExtract_Click);
            // 
            // ribbonPanel2
            // 
            this.ribbonPanel2.ButtonMoreEnabled = false;
            this.ribbonPanel2.ButtonMoreVisible = false;
            this.ribbonPanel2.Items.Add(this.ckbRecurse);
            this.ribbonPanel2.Items.Add(this.ckbAutoOpen);
            this.ribbonPanel2.Text = "Set MOTW Options";
            // 
            // ckbRecurse
            // 
            this.ckbRecurse.Checked = true;
            this.ckbRecurse.Text = "Recusre all Folders and Files";
            // 
            // ckbAutoOpen
            // 
            this.ckbAutoOpen.Checked = true;
            this.ckbAutoOpen.Text = "Open Folder after cleaning";
            // 
            // ribbonPanel5
            // 
            this.ribbonPanel5.ButtonMoreEnabled = false;
            this.ribbonPanel5.ButtonMoreVisible = false;
            this.ribbonPanel5.Items.Add(this.btnClean);
            this.ribbonPanel5.Text = "Clean Files";
            // 
            // btnClean
            // 
            this.btnClean.Enabled = false;
            this.btnClean.Image = global::MOTW_Cleaner.Properties.Resources.Clean48;
            this.btnClean.MinimumSize = new System.Drawing.Size(90, 0);
            this.btnClean.SmallImage = ((System.Drawing.Image)(resources.GetObject("btnClean.SmallImage")));
            this.btnClean.Text = "Unblock";
            this.btnClean.Click += new System.EventHandler(this.btnClean_Click);
            // 
            // ribbonPanel3
            // 
            this.ribbonPanel3.ButtonMoreEnabled = false;
            this.ribbonPanel3.ButtonMoreVisible = false;
            this.ribbonPanel3.Text = "";
            // 
            // ribbonPanel4
            // 
            this.ribbonPanel4.ButtonMoreEnabled = false;
            this.ribbonPanel4.ButtonMoreVisible = false;
            this.ribbonPanel4.Items.Add(this.btnClose);
            this.ribbonPanel4.Text = "Exit MTOW App";
            // 
            // btnClose
            // 
            this.btnClose.Image = global::MOTW_Cleaner.Properties.Resources.Exit48;
            this.btnClose.MinimumSize = new System.Drawing.Size(150, 0);
            this.btnClose.SmallImage = ((System.Drawing.Image)(resources.GetObject("btnClose.SmallImage")));
            this.btnClose.Text = "Close Application";
            this.btnClose.Click += new System.EventHandler(this.btnExtract_Click);
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.lblFlashLabel);
            this.panel1.Controls.Add(this.picBrowse);
            this.panel1.Controls.Add(this.splitContainer1);
            this.panel1.Controls.Add(this.picDragDrop);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel1.Location = new System.Drawing.Point(0, 154);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(703, 263);
            this.panel1.TabIndex = 4;
            // 
            // picBrowse
            // 
            this.picBrowse.Cursor = System.Windows.Forms.Cursors.Hand;
            this.picBrowse.Image = global::MOTW_Cleaner.Properties.Resources.Browse;
            this.picBrowse.Location = new System.Drawing.Point(156, 202);
            this.picBrowse.Name = "picBrowse";
            this.picBrowse.Size = new System.Drawing.Size(100, 25);
            this.picBrowse.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.picBrowse.TabIndex = 3;
            this.picBrowse.TabStop = false;
            this.picBrowse.Click += new System.EventHandler(this.btnBrowse_Click);
            // 
            // splitContainer1
            // 
            this.splitContainer1.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(419, 0);
            this.splitContainer1.Name = "splitContainer1";
            this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.lstLog);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.txtFolder);
            this.splitContainer1.Panel2.Controls.Add(this.btnOpen);
            this.splitContainer1.Size = new System.Drawing.Size(284, 263);
            this.splitContainer1.SplitterDistance = 230;
            this.splitContainer1.TabIndex = 2;
            // 
            // lstLog
            // 
            this.lstLog.AllowDrop = true;
            this.lstLog.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lstLog.FormattingEnabled = true;
            this.lstLog.Location = new System.Drawing.Point(0, 0);
            this.lstLog.Name = "lstLog";
            this.lstLog.Size = new System.Drawing.Size(280, 226);
            this.lstLog.TabIndex = 0;
            this.lstLog.DragEnter += new System.Windows.Forms.DragEventHandler(this.lstLog_DragEnter);
            // 
            // txtFolder
            // 
            this.txtFolder.Dock = System.Windows.Forms.DockStyle.Left;
            this.txtFolder.Location = new System.Drawing.Point(0, 0);
            this.txtFolder.Multiline = true;
            this.txtFolder.Name = "txtFolder";
            this.txtFolder.Size = new System.Drawing.Size(215, 25);
            this.txtFolder.TabIndex = 1;
            this.txtFolder.TextChanged += new System.EventHandler(this.txtFolder_TextChanged);
            // 
            // btnOpen
            // 
            this.btnOpen.Dock = System.Windows.Forms.DockStyle.Right;
            this.btnOpen.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnOpen.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnOpen.ImageIndex = 1;
            this.btnOpen.ImageList = this.imageList1;
            this.btnOpen.Location = new System.Drawing.Point(215, 0);
            this.btnOpen.Name = "btnOpen";
            this.btnOpen.Size = new System.Drawing.Size(65, 25);
            this.btnOpen.TabIndex = 0;
            this.btnOpen.Text = "     Open";
            this.btnOpen.UseVisualStyleBackColor = true;
            this.btnOpen.Click += new System.EventHandler(this.btnOpen_Click);
            // 
            // imageList1
            // 
            this.imageList1.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList1.ImageStream")));
            this.imageList1.TransparentColor = System.Drawing.Color.Transparent;
            this.imageList1.Images.SetKeyName(0, "52.png");
            this.imageList1.Images.SetKeyName(1, "Files.png");
            // 
            // picDragDrop
            // 
            this.picDragDrop.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.picDragDrop.Dock = System.Windows.Forms.DockStyle.Left;
            this.picDragDrop.Image = global::MOTW_Cleaner.Properties.Resources.DragDrop;
            this.picDragDrop.Location = new System.Drawing.Point(0, 0);
            this.picDragDrop.Name = "picDragDrop";
            this.picDragDrop.Size = new System.Drawing.Size(419, 263);
            this.picDragDrop.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.picDragDrop.TabIndex = 1;
            this.picDragDrop.TabStop = false;
            this.picDragDrop.DragDrop += new System.Windows.Forms.DragEventHandler(this.picDragDrop_DragDrop);
            this.picDragDrop.DragEnter += new System.Windows.Forms.DragEventHandler(this.picDragDrop_DragEnter);
            this.picDragDrop.DragLeave += new System.EventHandler(this.picDragDrop_DragLeave);
            // 
            // statusStrip1
            // 
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripStatusLabel1});
            this.statusStrip1.Location = new System.Drawing.Point(0, 422);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(703, 22);
            this.statusStrip1.SizingGrip = false;
            this.statusStrip1.TabIndex = 5;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // toolStripStatusLabel1
            // 
            this.toolStripStatusLabel1.Name = "toolStripStatusLabel1";
            this.toolStripStatusLabel1.Size = new System.Drawing.Size(129, 17);
            this.toolStripStatusLabel1.Text = "Select Folder or Zip File";
            // 
            // lblFlashLabel
            // 
            this.lblFlashLabel.AutoSize = true;
            this.lblFlashLabel.BackColor = System.Drawing.Color.White;
            this.lblFlashLabel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lblFlashLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Italic))), System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblFlashLabel.Location = new System.Drawing.Point(78, 148);
            this.lblFlashLabel.Name = "lblFlashLabel";
            this.lblFlashLabel.Size = new System.Drawing.Size(256, 22);
            this.lblFlashLabel.TabIndex = 1;
            this.lblFlashLabel.Text = "Unblock MTOW has completed";
            this.lblFlashLabel.Visible = false;
            // 
            // progressExtract
            // 
            this.progressExtract.BackColor = System.Drawing.Color.LightCyan;
            this.progressExtract.CustomText = "No MOTW Files Selected";
            this.progressExtract.Location = new System.Drawing.Point(0, 421);
            this.progressExtract.Name = "progressExtract";
            this.progressExtract.ProgressColor = System.Drawing.Color.FromArgb(((int)(((byte)(126)))), ((int)(((byte)(180)))), ((int)(((byte)(234)))));
            this.progressExtract.Size = new System.Drawing.Size(417, 20);
            this.progressExtract.TabIndex = 2;
            this.progressExtract.TextColor = System.Drawing.Color.MidnightBlue;
            this.progressExtract.TextFont = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold);
            this.progressExtract.VisualMode = ProgressBarSample.ProgressBarDisplayMode.CurrProgress;
            // 
            // frmMOTW
            // 
            this.AllowDrop = true;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(703, 444);
            this.Controls.Add(this.progressExtract);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.ribbon1);
            this.KeyPreview = true;
            this.Name = "frmMOTW";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "MOTW Folder and Zip file cleaner";
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picBrowse)).EndInit();
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            this.splitContainer1.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.picDragDrop)).EndInit();
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private ProgressBarSample.TextProgressBar progressExtract;
        private System.Windows.Forms.Ribbon ribbon1;
        private System.Windows.Forms.RibbonTab ribbonTab1;
        private System.Windows.Forms.RibbonPanel ribbonPanel1;
        private System.Windows.Forms.RibbonOrbMenuItem btnOrbOpenFolder;
        private System.Windows.Forms.RibbonSeparator ribbonSeparator1;
        private System.Windows.Forms.RibbonOrbMenuItem btnOrbExtract;
        private System.Windows.Forms.RibbonSeparator ribbonSeparator2;
        private System.Windows.Forms.RibbonOrbMenuItem btnOrbCloseMOTW;
        private System.Windows.Forms.RibbonSeparator ribbonSeparator3;
        private System.Windows.Forms.RibbonButton btnBrowse;
        private System.Windows.Forms.RibbonSeparator ribbonSeparator4;
        private System.Windows.Forms.RibbonButton btnExtract;
        private System.Windows.Forms.RibbonSeparator ribbonSeparator5;
        private System.Windows.Forms.RibbonPanel ribbonPanel2;
        private System.Windows.Forms.RibbonCheckBox ckbRecurse;
        private System.Windows.Forms.RibbonCheckBox ckbAutoOpen;
        private System.Windows.Forms.RibbonPanel ribbonPanel3;
        private System.Windows.Forms.RibbonPanel ribbonPanel4;
        private System.Windows.Forms.RibbonButton btnClose;
        private System.Windows.Forms.RibbonPanel ribbonPanel5;
        private System.Windows.Forms.RibbonButton btnClean;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.ImageList imageList1;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.PictureBox picDragDrop;
        private System.Windows.Forms.ListBox lstLog;
        private System.Windows.Forms.TextBox txtFolder;
        private System.Windows.Forms.Button btnOpen;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel1;
        private System.Windows.Forms.PictureBox picBrowse;
        private System.Windows.Forms.Label lblFlashLabel;
    }
}

