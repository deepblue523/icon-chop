namespace IconChop
{
    partial class SettingsForm
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
            tabControl = new TabControl();
            tabGeneral = new TabPage();
            chkAutoReload = new CheckBox();
            lblExplorerIntro = new Label();
            btnExplorerContext = new Button();
            lblExplorerStatus = new Label();
            tabOpenAi = new TabPage();
            grpOpenAi = new GroupBox();
            lblApiKey = new Label();
            txtApiKey = new TextBox();
            lblBaseUrl = new Label();
            txtBaseUrl = new TextBox();
            lblModel = new Label();
            cboModel = new ComboBox();
            lblSize = new Label();
            cboSize = new ComboBox();
            lblQuality = new Label();
            cboQuality = new ComboBox();
            lblOpenAiHint = new Label();
            panelFooter = new Panel();
            btnSave = new Button();
            btnCancel = new Button();
            tabControl.SuspendLayout();
            tabGeneral.SuspendLayout();
            tabOpenAi.SuspendLayout();
            grpOpenAi.SuspendLayout();
            panelFooter.SuspendLayout();
            SuspendLayout();

            // tabControl
            tabControl.Controls.Add(tabGeneral);
            tabControl.Controls.Add(tabOpenAi);
            tabControl.Dock = DockStyle.Fill;

            // tabGeneral
            tabGeneral.Padding = new Padding(8);
            tabGeneral.Text = "General";
            tabGeneral.UseVisualStyleBackColor = true;
            tabGeneral.Controls.Add(lblExplorerStatus);
            tabGeneral.Controls.Add(btnExplorerContext);
            tabGeneral.Controls.Add(lblExplorerIntro);
            tabGeneral.Controls.Add(chkAutoReload);

            // chkAutoReload
            chkAutoReload.AutoSize = true;
            chkAutoReload.Location = new Point(12, 16);
            chkAutoReload.Text = "Auto-reload when file changes";
            chkAutoReload.ForeColor = Color.FromArgb(100, 100, 100);

            // lblExplorerIntro
            lblExplorerIntro.Location = new Point(12, 44);
            lblExplorerIntro.AutoSize = true;
            lblExplorerIntro.MaximumSize = new Size(620, 0);
            lblExplorerIntro.Anchor = AnchorStyles.Top | AnchorStyles.Left;
            lblExplorerIntro.Text =
                "Add or remove \"Open with IconChop\" on the right-click menu for image files in File Explorer.";

            // btnExplorerContext
            btnExplorerContext.Location = new Point(12, 96);
            btnExplorerContext.Size = new Size(280, 30);
            btnExplorerContext.Text = "Add to Explorer context menu";
            btnExplorerContext.FlatStyle = FlatStyle.Flat;
            btnExplorerContext.BackColor = Color.FromArgb(230, 230, 230);
            btnExplorerContext.FlatAppearance.BorderColor = Color.FromArgb(180, 180, 180);
            btnExplorerContext.Cursor = Cursors.Hand;

            // lblExplorerStatus
            lblExplorerStatus.Location = new Point(12, 136);
            lblExplorerStatus.AutoSize = true;
            lblExplorerStatus.MaximumSize = new Size(620, 0);
            lblExplorerStatus.Anchor = AnchorStyles.Top | AnchorStyles.Left;
            lblExplorerStatus.ForeColor = Color.FromArgb(100, 100, 100);
            lblExplorerStatus.Text = "";

            // tabOpenAi
            tabOpenAi.Padding = new Padding(8, 8, 20, 8);
            tabOpenAi.Text = "Open AI";
            tabOpenAi.UseVisualStyleBackColor = true;
            tabOpenAi.Controls.Add(grpOpenAi);

            // grpOpenAi
            grpOpenAi.Dock = DockStyle.Fill;
            grpOpenAi.Padding = new Padding(8);
            grpOpenAi.Text = "OpenAI image API";
            grpOpenAi.Controls.Add(lblOpenAiHint);
            grpOpenAi.Controls.Add(lblQuality);
            grpOpenAi.Controls.Add(cboQuality);
            grpOpenAi.Controls.Add(lblSize);
            grpOpenAi.Controls.Add(cboSize);
            grpOpenAi.Controls.Add(lblModel);
            grpOpenAi.Controls.Add(cboModel);
            grpOpenAi.Controls.Add(lblBaseUrl);
            grpOpenAi.Controls.Add(txtBaseUrl);
            grpOpenAi.Controls.Add(lblApiKey);
            grpOpenAi.Controls.Add(txtApiKey);

            // lblApiKey
            lblApiKey.AutoSize = true;
            lblApiKey.Location = new Point(14, 28);
            lblApiKey.Text = "API key:";

            // txtApiKey
            txtApiKey.Location = new Point(120, 24);
            txtApiKey.Size = new Size(448, 23);
            txtApiKey.Anchor = AnchorStyles.Top | AnchorStyles.Left;
            txtApiKey.UseSystemPasswordChar = true;

            // lblBaseUrl
            lblBaseUrl.AutoSize = true;
            lblBaseUrl.Location = new Point(14, 58);
            lblBaseUrl.Text = "API base URL:";

            // txtBaseUrl
            txtBaseUrl.Location = new Point(120, 54);
            txtBaseUrl.Size = new Size(448, 23);
            txtBaseUrl.Anchor = AnchorStyles.Top | AnchorStyles.Left;

            // lblModel
            lblModel.AutoSize = true;
            lblModel.Location = new Point(14, 88);
            lblModel.Text = "Model:";

            // cboModel
            cboModel.DropDownStyle = ComboBoxStyle.DropDown;
            cboModel.Location = new Point(120, 84);
            cboModel.Size = new Size(448, 23);
            cboModel.Anchor = AnchorStyles.Top | AnchorStyles.Left;
            cboModel.Items.AddRange(new object[] { "dall-e-3", "dall-e-2", "gpt-image-1" });

            // lblSize
            lblSize.AutoSize = true;
            lblSize.Location = new Point(14, 118);
            lblSize.Text = "Image size:";

            // cboSize
            cboSize.DropDownStyle = ComboBoxStyle.DropDownList;
            cboSize.Location = new Point(120, 114);
            cboSize.Size = new Size(260, 23);
            cboSize.Items.AddRange(new object[]
            {
                "256x256", "512x512", "1024x1024", "1792x1024", "1024x1792"
            });

            // lblQuality
            lblQuality.AutoSize = true;
            lblQuality.Location = new Point(14, 148);
            lblQuality.Text = "Quality (DALL·E 3):";

            // cboQuality
            cboQuality.DropDownStyle = ComboBoxStyle.DropDownList;
            cboQuality.Location = new Point(120, 144);
            cboQuality.Size = new Size(260, 23);
            cboQuality.Items.AddRange(new object[] { "standard", "hd" });

            // lblOpenAiHint
            lblOpenAiHint.AutoSize = true;
            lblOpenAiHint.Location = new Point(14, 180);
            lblOpenAiHint.MaximumSize = new Size(554, 0);
            lblOpenAiHint.Anchor = AnchorStyles.Top | AnchorStyles.Left;
            lblOpenAiHint.ForeColor = Color.FromArgb(100, 100, 100);
            lblOpenAiHint.Text =
                "These values are used for POST /v1/images/generations (and compatible endpoints). " +
                "Saving writes options to icon-chop/config.json in your user profile (~). Keep that file private. " +
                "Change the base URL only if you use a proxy or Azure OpenAI–compatible host.";

            // btnSave
            // panelFooter
            panelFooter.Dock = DockStyle.Bottom;
            panelFooter.Height = 48;
            panelFooter.Padding = new Padding(12, 8, 12, 8);
            panelFooter.Controls.Add(btnCancel);
            panelFooter.Controls.Add(btnSave);

            btnSave.Location = new Point(588, 10);
            btnSave.Size = new Size(90, 28);
            btnSave.Text = "Save";
            btnSave.Anchor = AnchorStyles.Top | AnchorStyles.Left;
            btnSave.FlatStyle = FlatStyle.Flat;
            btnSave.BackColor = Color.FromArgb(0, 122, 204);
            btnSave.ForeColor = Color.White;
            btnSave.FlatAppearance.BorderSize = 0;
            btnSave.Cursor = Cursors.Hand;
            btnSave.DialogResult = DialogResult.None;

            // btnCancel
            btnCancel.Location = new Point(490, 10);
            btnCancel.Size = new Size(90, 28);
            btnCancel.Text = "Cancel";
            btnCancel.Anchor = AnchorStyles.Top | AnchorStyles.Left;
            btnCancel.FlatStyle = FlatStyle.Flat;
            btnCancel.BackColor = Color.FromArgb(230, 230, 230);
            btnCancel.FlatAppearance.BorderColor = Color.FromArgb(180, 180, 180);
            btnCancel.Cursor = Cursors.Hand;
            btnCancel.DialogResult = DialogResult.Cancel;

            // SettingsForm
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(720, 400);
            Controls.Add(tabControl);
            Controls.Add(panelFooter);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            MinimumSize = new Size(720, 400);
            MaximumSize = new Size(720, 400);
            StartPosition = FormStartPosition.CenterParent;
            Text = "Settings";
            AcceptButton = btnSave;
            CancelButton = btnCancel;

            grpOpenAi.ResumeLayout(false);
            grpOpenAi.PerformLayout();
            tabOpenAi.ResumeLayout(false);
            tabGeneral.ResumeLayout(false);
            tabControl.ResumeLayout(false);
            panelFooter.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private TabControl tabControl;
        private TabPage tabGeneral;
        private CheckBox chkAutoReload;
        private Label lblExplorerIntro;
        private Button btnExplorerContext;
        private Label lblExplorerStatus;
        private TabPage tabOpenAi;
        private GroupBox grpOpenAi;
        private Label lblApiKey;
        private TextBox txtApiKey;
        private Label lblBaseUrl;
        private TextBox txtBaseUrl;
        private Label lblModel;
        private ComboBox cboModel;
        private Label lblSize;
        private ComboBox cboSize;
        private Label lblQuality;
        private ComboBox cboQuality;
        private Label lblOpenAiHint;
        private Panel panelFooter;
        private Button btnSave;
        private Button btnCancel;
    }
}
