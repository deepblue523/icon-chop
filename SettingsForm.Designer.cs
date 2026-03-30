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
            panelOpenAi = new Panel();
            grpImage = new GroupBox();
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
            grpText = new GroupBox();
            lblTextApiKey = new Label();
            txtTextApiKey = new TextBox();
            lblTextBaseUrl = new Label();
            txtTextBaseUrl = new TextBox();
            lblNamingModel = new Label();
            cboNamingModel = new ComboBox();
            lblOpenAiHint = new Label();
            panelFooter = new Panel();
            btnSave = new Button();
            btnCancel = new Button();
            tabControl.SuspendLayout();
            tabGeneral.SuspendLayout();
            tabOpenAi.SuspendLayout();
            panelOpenAi.SuspendLayout();
            grpImage.SuspendLayout();
            grpText.SuspendLayout();
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
            lblExplorerIntro.MaximumSize = new Size(680, 0);
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
            lblExplorerStatus.MaximumSize = new Size(680, 0);
            lblExplorerStatus.Anchor = AnchorStyles.Top | AnchorStyles.Left;
            lblExplorerStatus.ForeColor = Color.FromArgb(100, 100, 100);
            lblExplorerStatus.Text = "";

            // tabOpenAi
            tabOpenAi.Padding = new Padding(8, 8, 8, 8);
            tabOpenAi.Text = "Open AI";
            tabOpenAi.UseVisualStyleBackColor = true;
            tabOpenAi.Controls.Add(panelOpenAi);

            // panelOpenAi
            panelOpenAi.Dock = DockStyle.Fill;
            panelOpenAi.AutoScroll = true;
            panelOpenAi.Controls.Add(lblOpenAiHint);
            panelOpenAi.Controls.Add(grpText);
            panelOpenAi.Controls.Add(grpImage);

            // ── Image generation group ──────────────────────────────

            // grpImage
            grpImage.Location = new Point(0, 0);
            grpImage.Size = new Size(740, 178);
            grpImage.Text = "Image generation";
            grpImage.Controls.Add(lblQuality);
            grpImage.Controls.Add(cboQuality);
            grpImage.Controls.Add(lblSize);
            grpImage.Controls.Add(cboSize);
            grpImage.Controls.Add(lblModel);
            grpImage.Controls.Add(cboModel);
            grpImage.Controls.Add(lblBaseUrl);
            grpImage.Controls.Add(txtBaseUrl);
            grpImage.Controls.Add(lblApiKey);
            grpImage.Controls.Add(txtApiKey);

            // lblApiKey
            lblApiKey.AutoSize = true;
            lblApiKey.Location = new Point(14, 28);
            lblApiKey.Text = "API key:";

            // txtApiKey
            txtApiKey.Location = new Point(130, 24);
            txtApiKey.Size = new Size(510, 23);
            txtApiKey.Anchor = AnchorStyles.Top | AnchorStyles.Left;
            txtApiKey.UseSystemPasswordChar = true;

            // lblBaseUrl
            lblBaseUrl.AutoSize = true;
            lblBaseUrl.Location = new Point(14, 58);
            lblBaseUrl.Text = "API base URL:";

            // txtBaseUrl
            txtBaseUrl.Location = new Point(130, 54);
            txtBaseUrl.Size = new Size(510, 23);
            txtBaseUrl.Anchor = AnchorStyles.Top | AnchorStyles.Left;

            // lblModel
            lblModel.AutoSize = true;
            lblModel.Location = new Point(14, 88);
            lblModel.Text = "Model:";

            // cboModel
            cboModel.DropDownStyle = ComboBoxStyle.DropDown;
            cboModel.Location = new Point(130, 84);
            cboModel.Size = new Size(510, 23);
            cboModel.Anchor = AnchorStyles.Top | AnchorStyles.Left;
            cboModel.Items.AddRange(new object[] { "dall-e-3", "dall-e-2", "gpt-image-1" });

            // lblSize
            lblSize.AutoSize = true;
            lblSize.Location = new Point(14, 118);
            lblSize.Text = "Image size:";

            // cboSize
            cboSize.DropDownStyle = ComboBoxStyle.DropDownList;
            cboSize.Location = new Point(130, 114);
            cboSize.Size = new Size(260, 23);
            cboSize.Items.AddRange(new object[]
            {
                "256x256", "512x512", "1024x1024", "1792x1024", "1024x1792"
            });

            // lblQuality
            lblQuality.AutoSize = true;
            lblQuality.Location = new Point(14, 148);
            lblQuality.Text = "Quality (DALL\u00B7E 3):";

            // cboQuality
            cboQuality.DropDownStyle = ComboBoxStyle.DropDownList;
            cboQuality.Location = new Point(130, 144);
            cboQuality.Size = new Size(260, 23);
            cboQuality.Items.AddRange(new object[] { "standard", "hd" });

            // ── Text / Chat group ───────────────────────────────────

            // grpText
            grpText.Location = new Point(0, 186);
            grpText.Size = new Size(740, 118);
            grpText.Text = "Text / Chat completions";
            grpText.Controls.Add(lblNamingModel);
            grpText.Controls.Add(cboNamingModel);
            grpText.Controls.Add(lblTextBaseUrl);
            grpText.Controls.Add(txtTextBaseUrl);
            grpText.Controls.Add(lblTextApiKey);
            grpText.Controls.Add(txtTextApiKey);

            // lblTextApiKey
            lblTextApiKey.AutoSize = true;
            lblTextApiKey.Location = new Point(14, 28);
            lblTextApiKey.Text = "API key:";

            // txtTextApiKey
            txtTextApiKey.Location = new Point(130, 24);
            txtTextApiKey.Size = new Size(510, 23);
            txtTextApiKey.Anchor = AnchorStyles.Top | AnchorStyles.Left;
            txtTextApiKey.UseSystemPasswordChar = true;

            // lblTextBaseUrl
            lblTextBaseUrl.AutoSize = true;
            lblTextBaseUrl.Location = new Point(14, 58);
            lblTextBaseUrl.Text = "API base URL:";

            // txtTextBaseUrl
            txtTextBaseUrl.Location = new Point(130, 54);
            txtTextBaseUrl.Size = new Size(510, 23);
            txtTextBaseUrl.Anchor = AnchorStyles.Top | AnchorStyles.Left;

            // lblNamingModel
            lblNamingModel.AutoSize = true;
            lblNamingModel.Location = new Point(14, 88);
            lblNamingModel.Text = "Model:";

            // cboNamingModel
            cboNamingModel.DropDownStyle = ComboBoxStyle.DropDown;
            cboNamingModel.Location = new Point(130, 84);
            cboNamingModel.Size = new Size(260, 23);
            cboNamingModel.Anchor = AnchorStyles.Top | AnchorStyles.Left;
            cboNamingModel.Items.AddRange(new object[] { "gpt-4o-mini", "gpt-4o", "gpt-4.1-mini", "gpt-4.1-nano" });

            // ── Hint label ──────────────────────────────────────────

            // lblOpenAiHint
            lblOpenAiHint.AutoSize = true;
            lblOpenAiHint.Location = new Point(4, 312);
            lblOpenAiHint.MaximumSize = new Size(730, 0);
            lblOpenAiHint.Anchor = AnchorStyles.Top | AnchorStyles.Left;
            lblOpenAiHint.ForeColor = Color.FromArgb(100, 100, 100);
            lblOpenAiHint.Text =
                "Image settings are used for generation. Text/chat settings are used by " +
                "features like Auto-name (icon description). Leave text API key and base URL " +
                "blank to reuse the image values. " +
                "Settings are saved to icon-chop/config.json in your user profile (~). " +
                "Keep that file private.";

            // ── Footer ──────────────────────────────────────────────

            // panelFooter
            panelFooter.Dock = DockStyle.Bottom;
            panelFooter.Height = 48;
            panelFooter.Padding = new Padding(12, 8, 12, 8);
            panelFooter.Controls.Add(btnCancel);
            panelFooter.Controls.Add(btnSave);

            // btnSave
            btnSave.Location = new Point(660, 10);
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
            btnCancel.Location = new Point(562, 10);
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
            ClientSize = new Size(792, 528);
            Controls.Add(tabControl);
            Controls.Add(panelFooter);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            MinimumSize = new Size(792, 528);
            MaximumSize = new Size(792, 528);
            StartPosition = FormStartPosition.CenterParent;
            Text = "Settings";
            AcceptButton = btnSave;
            CancelButton = btnCancel;

            grpImage.ResumeLayout(false);
            grpImage.PerformLayout();
            grpText.ResumeLayout(false);
            grpText.PerformLayout();
            panelOpenAi.ResumeLayout(false);
            panelOpenAi.PerformLayout();
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
        private Panel panelOpenAi;
        private GroupBox grpImage;
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
        private GroupBox grpText;
        private Label lblTextApiKey;
        private TextBox txtTextApiKey;
        private Label lblTextBaseUrl;
        private TextBox txtTextBaseUrl;
        private Label lblNamingModel;
        private ComboBox cboNamingModel;
        private Label lblOpenAiHint;
        private Panel panelFooter;
        private Button btnSave;
        private Button btnCancel;
    }
}
