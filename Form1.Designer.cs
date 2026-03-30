namespace IconChop
{
    partial class Form1
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();

            panelTop = new Panel();
            menuStripMain = new MenuStrip();
            mnuFile = new ToolStripMenuItem();
            mnuLoadImage = new ToolStripMenuItem();
            mnuFileSep1 = new ToolStripSeparator();
            mnuSaveImages = new ToolStripMenuItem();
            mnuImage = new ToolStripMenuItem();
            mnuImageGenerate = new ToolStripMenuItem();
            mnuTools = new ToolStripMenuItem();
            mnuSettings = new ToolStripMenuItem();
            lblSizes = new Label();
            panelSizes = new FlowLayoutPanel();
            lblOutputDir = new Label();
            cboOutputDir = new ComboBox();
            lblOutputFormat = new Label();
            cboOutputFormat = new ComboBox();
            lblOutputPrefix = new Label();
            txtOutputPrefix = new TextBox();
            btnBrowse = new Button();

            chkAutoName = new CheckBox();
            splitMain = new SplitContainer();
            lblSource = new Label();
            panelSourceScroll = new Panel();
            picSource = new PictureBox();
            lblPreviewCount = new Label();
            panelPreviewTop = new Panel();
            btnSelectAll = new Button();
            btnDeselectAll = new Button();
            flowPreview = new FlowLayoutPanel();

            panelTop.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)splitMain).BeginInit();
            splitMain.Panel1.SuspendLayout();
            splitMain.Panel2.SuspendLayout();
            splitMain.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)picSource).BeginInit();
            SuspendLayout();

            // menuStripMain
            menuStripMain.Dock = DockStyle.Top;
            menuStripMain.Items.AddRange(new ToolStripItem[] { mnuFile, mnuImage, mnuTools });
            menuStripMain.Location = new Point(0, 0);
            menuStripMain.Size = new Size(1280, 24);
            menuStripMain.TabIndex = 0;
            menuStripMain.Text = "menuStripMain";

            // mnuFile
            mnuFile.DropDownItems.AddRange(new ToolStripItem[] { mnuLoadImage, mnuFileSep1, mnuSaveImages });
            mnuFile.Text = "&File";

            // mnuLoadImage
            mnuLoadImage.Text = "Load &Image...";
            mnuLoadImage.Click += BtnLoadImage_Click;

            // mnuSaveImages
            mnuSaveImages.Text = "&Save Images";
            mnuSaveImages.ShortcutKeys = Keys.Control | Keys.S;
            mnuSaveImages.Click += BtnChop_Click;

            // mnuImage
            mnuImage.DropDownItems.AddRange(new ToolStripItem[] { mnuImageGenerate });
            mnuImage.Text = "&Image";

            // mnuImageGenerate
            mnuImageGenerate.Text = "&Generate…";
            mnuImageGenerate.Click += MnuImageGenerate_Click;

            // mnuTools
            mnuTools.DropDownItems.AddRange(new ToolStripItem[] { mnuSettings });
            mnuTools.Text = "&Tools";

            // mnuSettings
            mnuSettings.Text = "&Settings...";
            mnuSettings.Click += BtnSettings_Click;

            // panelTop

            panelTop.Controls.Add(chkAutoName);
            panelTop.Controls.Add(btnBrowse);
            panelTop.Controls.Add(txtOutputPrefix);
            panelTop.Controls.Add(lblOutputPrefix);
            panelTop.Controls.Add(cboOutputDir);
            panelTop.Controls.Add(cboOutputFormat);
            panelTop.Controls.Add(lblOutputFormat);
            panelTop.Controls.Add(lblOutputDir);
            panelTop.Controls.Add(panelSizes);
            panelTop.Controls.Add(lblSizes);
            panelTop.Dock = DockStyle.Top;
            panelTop.Location = new Point(0, 0);
            // Fit content only — extra height here showed as empty gray above Source Image
            panelTop.Size = new Size(1280, 110);
            panelTop.BackColor = Color.FromArgb(248, 248, 248);
            panelTop.BorderStyle = BorderStyle.FixedSingle;

            // lblSizes
            lblSizes.AutoSize = true;
            lblSizes.Location = new Point(14, 12);
            lblSizes.Text = "Output Sizes:";
            lblSizes.Font = new Font(SystemFonts.DefaultFont.FontFamily, 9F, FontStyle.Bold);

            // panelSizes
            panelSizes.Location = new Point(114, 8);
            panelSizes.Size = new Size(800, 30);
            panelSizes.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            panelSizes.WrapContents = false;

            int[] defaultSizes = [16, 24, 32, 48, 64, 128, 256, 512];
            foreach (int sz in defaultSizes)
            {
                var cb = new CheckBox
                {
                    Text = $"{sz}\u00d7{sz}",
                    Tag = sz,
                    AutoSize = true,
                    Checked = sz is 32 or 48 or 64,
                    Margin = new Padding(2, 4, 10, 4)
                };
                panelSizes.Controls.Add(cb);
            }

            // lblOutputDir
            lblOutputDir.AutoSize = true;
            lblOutputDir.Location = new Point(14, 46);
            lblOutputDir.Text = "Output Folder:";
            lblOutputDir.Font = new Font(SystemFonts.DefaultFont.FontFamily, 9F, FontStyle.Bold);

            // cboOutputDir
            cboOutputDir.Location = new Point(114, 43);
            cboOutputDir.Size = new Size(720, 23);
            cboOutputDir.Anchor = AnchorStyles.Top | AnchorStyles.Left;
            cboOutputDir.DropDownStyle = ComboBoxStyle.DropDown;
            cboOutputDir.MaxDropDownItems = 12;
            cboOutputDir.BackColor = Color.White;

            // lblOutputFormat (below Output Folder row)
            lblOutputFormat.AutoSize = true;
            lblOutputFormat.Location = new Point(14, 76);
            lblOutputFormat.Text = "Format:";
            lblOutputFormat.Font = new Font(SystemFonts.DefaultFont.FontFamily, 9F, FontStyle.Bold);

            // cboOutputFormat
            cboOutputFormat.DropDownStyle = ComboBoxStyle.DropDownList;
            cboOutputFormat.Location = new Point(114, 73);
            cboOutputFormat.Size = new Size(140, 23);
            cboOutputFormat.Items.AddRange(new object[] { "PNG only", "ICO only", "PNG and ICO" });
            cboOutputFormat.SelectedIndex = 0;
            cboOutputFormat.BackColor = Color.White;

            // lblOutputPrefix
            lblOutputPrefix.AutoSize = true;
            lblOutputPrefix.Location = new Point(844, 76);
            lblOutputPrefix.Text = "Prefix:";
            lblOutputPrefix.Font = new Font(SystemFonts.DefaultFont.FontFamily, 9F, FontStyle.Bold);

            // txtOutputPrefix
            txtOutputPrefix.Location = new Point(893, 73);
            txtOutputPrefix.Size = new Size(110, 23);
            txtOutputPrefix.MaxLength = 80;
            txtOutputPrefix.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            txtOutputPrefix.BackColor = Color.White;

            // btnBrowse
            btnBrowse.Location = new Point(840, 41);
            btnBrowse.Size = new Size(90, 28);
            btnBrowse.Text = "Browse...";
            btnBrowse.FlatStyle = FlatStyle.Flat;
            btnBrowse.BackColor = Color.FromArgb(230, 230, 230);
            btnBrowse.FlatAppearance.BorderColor = Color.FromArgb(180, 180, 180);
            btnBrowse.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnBrowse.Cursor = Cursors.Hand;
            btnBrowse.Click += BtnBrowse_Click;

            // chkAutoName
            chkAutoName.AutoSize = true;
            chkAutoName.Location = new Point(270, 76);
            chkAutoName.Text = "Auto-name (AI)";
            chkAutoName.ForeColor = Color.FromArgb(80, 80, 80);
            chkAutoName.Cursor = Cursors.Hand;

            // splitMain
            splitMain.Dock = DockStyle.Fill;
            splitMain.Orientation = Orientation.Vertical;
            splitMain.SplitterDistance = 620;
            splitMain.SplitterWidth = 5;
            splitMain.BackColor = Color.FromArgb(200, 200, 200);

            // splitMain.Panel1 — Source image
            splitMain.Panel1.Controls.Add(panelSourceScroll);
            splitMain.Panel1.Controls.Add(lblSource);

            // lblSource
            lblSource.Dock = DockStyle.Top;
            lblSource.Text = "  Source Image";
            lblSource.Height = 26;
            lblSource.TextAlign = ContentAlignment.MiddleLeft;
            lblSource.Font = new Font(SystemFonts.DefaultFont.FontFamily, 8.5F, FontStyle.Bold);
            lblSource.ForeColor = Color.FromArgb(60, 60, 60);
            lblSource.BackColor = Color.FromArgb(230, 230, 230);

            // panelSourceScroll
            panelSourceScroll.Dock = DockStyle.Fill;
            panelSourceScroll.AutoScroll = true;
            panelSourceScroll.BackColor = Color.FromArgb(45, 45, 48);

            // picSource
            picSource.Location = new Point(0, 0);
            picSource.SizeMode = PictureBoxSizeMode.Zoom;
            picSource.BackColor = Color.FromArgb(45, 45, 48);
            picSource.Dock = DockStyle.Fill;

            panelSourceScroll.Controls.Add(picSource);

            // splitMain.Panel2 — Preview
            splitMain.Panel2.Controls.Add(flowPreview);
            splitMain.Panel2.Controls.Add(panelPreviewTop);

            // panelPreviewTop
            panelPreviewTop.Dock = DockStyle.Top;
            panelPreviewTop.Height = 36;
            panelPreviewTop.BackColor = Color.FromArgb(230, 230, 230);
            panelPreviewTop.Controls.Add(lblPreviewCount);
            panelPreviewTop.Controls.Add(btnSelectAll);
            panelPreviewTop.Controls.Add(btnDeselectAll);

            // lblPreviewCount (label first so buttons draw on top)
            lblPreviewCount.Anchor = AnchorStyles.Top | AnchorStyles.Left;
            lblPreviewCount.Location = new Point(8, 8);
            lblPreviewCount.AutoSize = false;
            lblPreviewCount.Height = 22;
            lblPreviewCount.Width = 520;
            lblPreviewCount.Text = "  Preview";
            lblPreviewCount.TextAlign = ContentAlignment.MiddleLeft;
            lblPreviewCount.Font = new Font(SystemFonts.DefaultFont.FontFamily, 8.5F, FontStyle.Bold);
            lblPreviewCount.ForeColor = Color.FromArgb(60, 60, 60);
            lblPreviewCount.BackColor = Color.Transparent;

            // btnSelectAll (positioned on Resize; initial position for ~640px panel)
            btnSelectAll.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnSelectAll.Location = new Point(452, 5);
            btnSelectAll.Size = new Size(82, 26);
            btnSelectAll.Text = "Select All";
            btnSelectAll.FlatStyle = FlatStyle.Flat;
            btnSelectAll.BackColor = Color.FromArgb(240, 240, 240);
            btnSelectAll.FlatAppearance.BorderColor = Color.FromArgb(180, 180, 180);
            btnSelectAll.Cursor = Cursors.Hand;
            btnSelectAll.Click += BtnSelectAll_Click;

            // btnDeselectAll (positioned on Resize; initial position for ~640px panel)
            btnDeselectAll.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnDeselectAll.Location = new Point(540, 5);
            btnDeselectAll.Size = new Size(82, 26);
            btnDeselectAll.Text = "Deselect All";
            btnDeselectAll.FlatStyle = FlatStyle.Flat;
            btnDeselectAll.BackColor = Color.FromArgb(240, 240, 240);
            btnDeselectAll.FlatAppearance.BorderColor = Color.FromArgb(180, 180, 180);
            btnDeselectAll.Cursor = Cursors.Hand;
            btnDeselectAll.Click += BtnDeselectAll_Click;

            // flowPreview
            flowPreview.Dock = DockStyle.Fill;
            flowPreview.AutoScroll = true;
            flowPreview.BackColor = Color.White;
            flowPreview.Padding = new Padding(6);

            // Form1
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1280, 860);
            Controls.Add(splitMain);
            Controls.Add(panelTop);
            Controls.Add(menuStripMain);
            MainMenuStrip = menuStripMain;
            MinimumSize = new Size(900, 600);
            StartPosition = FormStartPosition.CenterScreen;
            Text = "IconChop \u2014 Icon Sheet Slicer";
            AllowDrop = true;

            panelTop.ResumeLayout(false);
            panelTop.PerformLayout();
            splitMain.Panel1.ResumeLayout(false);
            splitMain.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitMain).EndInit();
            splitMain.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)picSource).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private MenuStrip menuStripMain;
        private ToolStripMenuItem mnuFile;
        private ToolStripMenuItem mnuLoadImage;
        private ToolStripSeparator mnuFileSep1;
        private ToolStripMenuItem mnuSaveImages;
        private ToolStripMenuItem mnuImage;
        private ToolStripMenuItem mnuImageGenerate;
        private ToolStripMenuItem mnuTools;
        private ToolStripMenuItem mnuSettings;
        private Panel panelTop;
        private Label lblSizes;
        private FlowLayoutPanel panelSizes;
        private Label lblOutputDir;
        private ComboBox cboOutputDir;
        private Label lblOutputFormat;
        private ComboBox cboOutputFormat;
        private Label lblOutputPrefix;
        private TextBox txtOutputPrefix;
        private Button btnBrowse;

        private CheckBox chkAutoName;
        private SplitContainer splitMain;
        private Label lblSource;
        private Panel panelSourceScroll;
        private PictureBox picSource;
        private Label lblPreviewCount;
        private Panel panelPreviewTop;
        private Button btnSelectAll;
        private Button btnDeselectAll;
        private FlowLayoutPanel flowPreview;
    }
}
